using System;
using System.Globalization;
using System.Speech.Recognition;
using System.Windows;

namespace PostitApp.Services
{
    public class SpeechTranscriptionService : IDisposable
    {
        private SpeechRecognitionEngine? _engine;
        public bool IsListening { get; private set; }
        public bool IsAvailable { get; private set; }

        public event EventHandler<string>? TextRecognized;
        public event EventHandler<string>? HypothesisRecognized;
        public event EventHandler<string>? ErrorOccurred;

        public SpeechTranscriptionService()
        {
            InitializeEngine();
        }

        private void InitializeEngine()
        {
            try
            {
                // Intentar cargar reconocedor en español (es-ES) o el predeterminado del sistema
                CultureInfo culture;
                try
                {
                    culture = new CultureInfo("es-ES");
                    _engine = new SpeechRecognitionEngine(culture);
                }
                catch
                {
                    _engine = new SpeechRecognitionEngine();
                }

                _engine.LoadGrammar(new DictationGrammar());
                _engine.SetInputToDefaultAudioDevice();

                _engine.SpeechRecognized += (s, e) =>
                {
                    if (e.Result != null && !string.IsNullOrWhiteSpace(e.Result.Text))
                    {
                        var text = e.Result.Text.Trim();
                        DispatchUI(() => TextRecognized?.Invoke(this, text));
                    }
                };

                _engine.SpeechHypothesized += (s, e) =>
                {
                    if (e.Result != null && !string.IsNullOrWhiteSpace(e.Result.Text))
                    {
                        var text = e.Result.Text.Trim();
                        DispatchUI(() => HypothesisRecognized?.Invoke(this, text));
                    }
                };

                _engine.RecognizeCompleted += (s, e) =>
                {
                    IsListening = false;
                };

                IsAvailable = true;
            }
            catch (Exception ex)
            {
                IsAvailable = false;
                System.Diagnostics.Debug.WriteLine($"Reconocimiento de voz no disponible: {ex.Message}");
            }
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

        public bool StartListening()
        {
            if (!IsAvailable || _engine == null || IsListening) return false;
            try
            {
                _engine.RecognizeAsync(RecognizeMode.Multiple);
                IsListening = true;
                return true;
            }
            catch (Exception ex)
            {
                IsListening = false;
                DispatchUI(() => ErrorOccurred?.Invoke(this, $"Error en transcripción: {ex.Message}"));
                return false;
            }
        }

        public void StopListening()
        {
            if (!IsListening || _engine == null) return;
            try
            {
                _engine.RecognizeAsyncStop();
            }
            catch { }
            finally
            {
                IsListening = false;
            }
        }

        public void Dispose()
        {
            StopListening();
            _engine?.Dispose();
            _engine = null;
        }
    }
}
