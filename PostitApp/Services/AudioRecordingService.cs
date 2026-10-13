using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;

namespace PostitApp.Services
{
    public class AudioRecordingService : IDisposable
    {
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _writer;
        private string? _currentRecordingPath;
        private DispatcherTimer? _recordingTimer;
        private DateTime _recordingStartTime;

        private WaveOutEvent? _waveOut;
        private AudioFileReader? _audioReader;
        private DispatcherTimer? _playbackTimer;

        public bool IsRecording { get; private set; }
        public bool IsPlaying => _waveOut?.PlaybackState == PlaybackState.Playing;
        public bool IsPaused => _waveOut?.PlaybackState == PlaybackState.Paused;

        public TimeSpan RecordingDuration { get; private set; } = TimeSpan.Zero;
        public TimeSpan PlaybackCurrentTime => _audioReader?.CurrentTime ?? TimeSpan.Zero;
        public TimeSpan PlaybackTotalTime => _audioReader?.TotalTime ?? TimeSpan.Zero;

        public event EventHandler<float>? PeakLevelChanged;
        public event EventHandler<TimeSpan>? RecordingTick;
        public event EventHandler<TimeSpan>? PlaybackTick;
        public event EventHandler? RecordingCompleted;
        public event EventHandler? PlaybackCompleted;
        public event EventHandler<string>? ErrorOccurred;

        public AudioRecordingService()
        {
            _recordingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _recordingTimer.Tick += (s, e) =>
            {
                if (IsRecording)
                {
                    RecordingDuration = DateTime.Now - _recordingStartTime;
                    DispatchUI(() => RecordingTick?.Invoke(this, RecordingDuration));
                }
            };

            _playbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _playbackTimer.Tick += (s, e) =>
            {
                if (IsPlaying)
                {
                    DispatchUI(() => PlaybackTick?.Invoke(this, PlaybackCurrentTime));
                }
            };
        }

        private static void DispatchUI(Action action)
        {
            var app = Application.Current;
            if (app != null && app.Dispatcher != null && !app.Dispatcher.CheckAccess())
            {
                app.Dispatcher.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        public bool StartRecording(string destinationWavPath)
        {
            try
            {
                StopPlayback();

                if (IsRecording)
                {
                    StopRecording();
                }

                var dir = Path.GetDirectoryName(destinationWavPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                _currentRecordingPath = destinationWavPath;
                _waveIn = new WaveInEvent
                {
                    DeviceNumber = 0,
                    WaveFormat = new WaveFormat(44100, 16, 1) // 44.1 kHz, 16 bits, mono
                };

                _writer = new WaveFileWriter(_currentRecordingPath, _waveIn.WaveFormat);

                _waveIn.DataAvailable += (s, a) =>
                {
                    if (_writer != null && a.BytesRecorded > 0)
                    {
                        try
                        {
                            _writer.Write(a.Buffer, 0, a.BytesRecorded);

                            // Calcular nivel de pico para visualizador
                            float max = 0;
                            for (int index = 0; index < a.BytesRecorded; index += 2)
                            {
                                short sample = (short)((a.Buffer[index + 1] << 8) | a.Buffer[index]);
                                float absSample = Math.Abs(sample / 32768f);
                                if (absSample > max) max = absSample;
                            }

                            DispatchUI(() => PeakLevelChanged?.Invoke(this, max));
                        }
                        catch { }
                    }
                };

                _waveIn.RecordingStopped += (s, a) =>
                {
                    try
                    {
                        _writer?.Flush();
                        _writer?.Dispose();
                        _writer = null;

                        _waveIn?.Dispose();
                        _waveIn = null;
                    }
                    catch { }

                    IsRecording = false;

                    DispatchUI(() =>
                    {
                        _recordingTimer?.Stop();
                        RecordingCompleted?.Invoke(this, EventArgs.Empty);

                        if (a.Exception != null)
                        {
                            ErrorOccurred?.Invoke(this, a.Exception.Message);
                        }
                    });
                };

                _recordingStartTime = DateTime.Now;
                RecordingDuration = TimeSpan.Zero;
                IsRecording = true;
                _waveIn.StartRecording();
                _recordingTimer?.Start();
                return true;
            }
            catch (Exception ex)
            {
                IsRecording = false;
                _recordingTimer?.Stop();
                CleanUpRecording();
                DispatchUI(() => ErrorOccurred?.Invoke(this, $"No se pudo iniciar la grabación: {ex.Message}"));
                return false;
            }
        }

        public void StopRecording()
        {
            if (!IsRecording || _waveIn == null) return;
            try
            {
                _waveIn.StopRecording();
            }
            catch (Exception ex)
            {
                DispatchUI(() => ErrorOccurred?.Invoke(this, ex.Message));
                CleanUpRecording();
            }
        }

        public void CancelRecording()
        {
            var pathToDelete = _currentRecordingPath;
            StopRecording();
            try
            {
                if (!string.IsNullOrEmpty(pathToDelete) && File.Exists(pathToDelete))
                {
                    File.Delete(pathToDelete);
                }
            }
            catch { }
        }

        private void CleanUpRecording()
        {
            try
            {
                _writer?.Dispose();
                _writer = null;
                _waveIn?.Dispose();
                _waveIn = null;
                IsRecording = false;
                _recordingTimer?.Stop();
            }
            catch { }
        }

        public bool Play(string wavPath)
        {
            try
            {
                if (!File.Exists(wavPath))
                {
                    DispatchUI(() => ErrorOccurred?.Invoke(this, "El archivo de audio no existe."));
                    return false;
                }

                if (IsPaused && _audioReader != null && _waveOut != null)
                {
                    _waveOut.Play();
                    _playbackTimer?.Start();
                    return true;
                }

                StopPlayback();

                _audioReader = new AudioFileReader(wavPath);
                _waveOut = new WaveOutEvent();
                _waveOut.Init(_audioReader);

                _waveOut.PlaybackStopped += (s, e) =>
                {
                    DispatchUI(() =>
                    {
                        _playbackTimer?.Stop();
                        PlaybackCompleted?.Invoke(this, EventArgs.Empty);
                        if (_audioReader != null)
                        {
                            _audioReader.Position = 0;
                        }
                    });
                };

                _waveOut.Play();
                _playbackTimer?.Start();
                return true;
            }
            catch (Exception ex)
            {
                StopPlayback();
                DispatchUI(() => ErrorOccurred?.Invoke(this, $"Error al reproducir audio: {ex.Message}"));
                return false;
            }
        }

        public void Pause()
        {
            if (IsPlaying && _waveOut != null)
            {
                _waveOut.Pause();
                _playbackTimer?.Stop();
            }
        }

        public void StopPlayback()
        {
            try
            {
                _playbackTimer?.Stop();
                if (_waveOut != null)
                {
                    _waveOut.Stop();
                    _waveOut.Dispose();
                    _waveOut = null;
                }
                if (_audioReader != null)
                {
                    _audioReader.Dispose();
                    _audioReader = null;
                }
            }
            catch { }
        }

        public void Seek(TimeSpan target)
        {
            if (_audioReader != null)
            {
                if (target < TimeSpan.Zero) target = TimeSpan.Zero;
                if (target > _audioReader.TotalTime) target = _audioReader.TotalTime;
                _audioReader.CurrentTime = target;
            }
        }

        public static TimeSpan GetDuration(string wavPath)
        {
            try
            {
                if (File.Exists(wavPath))
                {
                    using var reader = new AudioFileReader(wavPath);
                    return reader.TotalTime;
                }
            }
            catch { }
            return TimeSpan.Zero;
        }

        public void Dispose()
        {
            StopRecording();
            StopPlayback();
            _recordingTimer?.Stop();
            _playbackTimer?.Stop();
        }
    }
}
