using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PostitApp.Helpers;
using PostitApp.Models;
using PostitApp.Services;
using PostitApp.ViewModels;

namespace PostitApp.Views
{
    public partial class NoteWindow : Window
    {
        // ==================== CAMPOS ====================

        private readonly NoteViewModel _viewModel;
        public string NoteId => _viewModel?.Id ?? string.Empty;

        // Servicio de audio autónomo (grabación directa por micrófono y reproducción)
        private readonly AudioRecordingService _audioService = new();
        private readonly SpeechTranscriptionService _transcriptionService = new();
        private bool _isUserDraggingSlider = false;

        // Alarma
        private DispatcherTimer? _timerAlarma;
        private DispatcherTimer? _timerSonido;

        // Animación de grabación
        private DispatcherTimer? _recBlinkTimer;
        private bool _dotVisible = true;

        // ==================== CONSTRUCTOR ====================

        public NoteWindow(NoteModel modelo)
        {
            _viewModel = new NoteViewModel(modelo);
            DataContext = _viewModel;

            InitializeComponent();

            Left    = modelo.PosX;
            Top     = modelo.PosY;
            Width   = modelo.Ancho;
            Height  = modelo.Alto;
            Opacity = modelo.Opacidad;
            Topmost = modelo.SiempreAlFrente;

            Loaded          += NoteWindow_Loaded;
            Closing         += NoteWindow_Closing;
            SizeChanged     += NoteWindow_SizeChanged;
            LocationChanged += NoteWindow_LocationChanged;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            InicializarAudioService();
        }

        // ==================== EVENTOS VENTANA ====================

        private void NoteWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            FondoPostit.Background = _viewModel.BrushFondo;
            FondoPostit.BorderBrush = _viewModel.BrushBorde;
            SliderIntensidad.Value = _viewModel.IntensidadColor;
            ActualizarUIIntensidad();
            CargarTextoRtf();
            CargarInkData();
            WindowHelper.SetSiempreAlFrente(this, _viewModel.SiempreAlFrente);
            WindowHelper.AsegurarDentroDelEscritorio(this);
            PanelHerramientas.Visibility = Visibility.Collapsed;
            ActualizarEstadoPanelAudio();
            IniciarTimerAlarma();
        }

        private void NoteWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _timerAlarma?.Stop();
            _timerSonido?.Stop();
            _recBlinkTimer?.Stop();

            _audioService.StopPlayback();
            _audioService.StopRecording();
            _audioService.Dispose();

            _transcriptionService.StopListening();
            _transcriptionService.Dispose();

            GuardarTextoRtf();
            GuardarInkData();
            _viewModel.PosX  = Left;
            _viewModel.PosY  = Top;
            _viewModel.Ancho = Width;
            _viewModel.Alto  = Height;
            App.NoteService.GuardarNotas();
        }

        private void NoteWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _viewModel.Ancho = Width;
            _viewModel.Alto  = Height;
        }

        private void NoteWindow_LocationChanged(object? sender, EventArgs e)
        {
            _viewModel.PosX = Left;
            _viewModel.PosY = Top;
        }

        // ==================== ARRASTRE ====================

        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowHelper.ArrastrarVentana(this);
        }

        // ==================== MENÚ Y APARIENCIA ====================

        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = !MenuPopup.IsOpen;
        }

        private void Fondo0_Click(object sender, RoutedEventArgs e) => SetFondo(0);
        private void Fondo1_Click(object sender, RoutedEventArgs e) => SetFondo(1);
        private void Fondo2_Click(object sender, RoutedEventArgs e) => SetFondo(2);
        private void Fondo3_Click(object sender, RoutedEventArgs e) => SetFondo(3);
        private void Fondo4_Click(object sender, RoutedEventArgs e) => SetFondo(4);
        private void Fondo5_Click(object sender, RoutedEventArgs e) => SetFondo(5);
        private void Fondo6_Click(object sender, RoutedEventArgs e) => SetFondo(6);
        private void Fondo7_Click(object sender, RoutedEventArgs e) => SetFondo(7);

        private void SetFondo(int indice)
        {
            _viewModel.IndiceFondo = indice;
            ActualizarUIIntensidad();
        }

        private void SliderIntensidad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded || _viewModel == null) return;
            _viewModel.IntensidadColor = e.NewValue;
            ActualizarUIIntensidad();
        }

        private void IntensidadPastel_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.IntensidadColor = 0.30;
            SliderIntensidad.Value = 0.30;
            ActualizarUIIntensidad();
        }

        private void IntensidadMedia_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.IntensidadColor = 0.65;
            SliderIntensidad.Value = 0.65;
            ActualizarUIIntensidad();
        }

        private void IntensidadViva_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.IntensidadColor = 1.0;
            SliderIntensidad.Value = 1.0;
            ActualizarUIIntensidad();
        }

        private void ActualizarUIIntensidad()
        {
            if (TxtIntensidadPct != null)
            {
                TxtIntensidadPct.Text = $"{(int)(_viewModel.IntensidadColor * 100)}%";
            }
            FondoPostit.Background = _viewModel.BrushFondo;
            FondoPostit.BorderBrush = _viewModel.BrushBorde;
        }

        private void Opacidad100_Click(object sender, RoutedEventArgs e) => SetOpacidad(1.0);
        private void Opacidad80_Click(object sender, RoutedEventArgs e)  => SetOpacidad(0.8);
        private void Opacidad60_Click(object sender, RoutedEventArgs e)  => SetOpacidad(0.6);
        private void Opacidad40_Click(object sender, RoutedEventArgs e)  => SetOpacidad(0.4);
        private void Opacidad20_Click(object sender, RoutedEventArgs e)  => SetOpacidad(0.2);

        private void SetOpacidad(double valor)
        {
            Opacity = valor;
            _viewModel.Opacidad = valor;
            MenuPopup.IsOpen = false;
        }

        private void ComboFuente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || Editor == null) return;
            if (ComboFuente.SelectedItem is ComboBoxItem item)
            {
                var fuente = item.Content.ToString() ?? "Segoe UI";
                _viewModel.Fuente = fuente;
                Editor.FontFamily = new FontFamily(fuente);
            }
        }

        private void Tamano10_Click(object sender, RoutedEventArgs e) => SetTamanoFuente(10);
        private void Tamano12_Click(object sender, RoutedEventArgs e) => SetTamanoFuente(12);
        private void Tamano14_Click(object sender, RoutedEventArgs e) => SetTamanoFuente(14);
        private void Tamano18_Click(object sender, RoutedEventArgs e) => SetTamanoFuente(18);
        private void Tamano24_Click(object sender, RoutedEventArgs e) => SetTamanoFuente(24);

        private void SetTamanoFuente(double tamano)
        {
            if (!IsLoaded || Editor == null) return;
            _viewModel.TamanoFuente = tamano;
            Editor.FontSize = tamano;
            MenuPopup.IsOpen = false;
        }

        private void BtnPin_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SiempreAlFrente = !_viewModel.SiempreAlFrente;
            Topmost = _viewModel.SiempreAlFrente;
            WindowHelper.SetSiempreAlFrente(this, _viewModel.SiempreAlFrente);
        }

        private void ChkAlFrente_Click(object sender, RoutedEventArgs e)
        {
            bool alFrente = ChkAlFrente.IsChecked ?? false;
            _viewModel.SiempreAlFrente = alFrente;
            Topmost = alFrente;
            WindowHelper.SetSiempreAlFrente(this, alFrente);
        }

        private void BtnOcultar_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.EsVisible = false;
            Close();
        }

        private void OcultarNota_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            _viewModel.EsVisible = false;
            Close();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show(
                "¿Deseas eliminar definitivamente esta nota?\n\n(Pulsa NO si solo deseas ocultarla)",
                "TopApp Memo — Cerrar Nota",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                EliminarArchivoAudio();
                App.NoteService.EliminarNota(_viewModel.Id);
                Close();
            }
            else if (resultado == MessageBoxResult.No)
            {
                _viewModel.EsVisible = false;
                Close();
            }
        }

        private void EliminarNota_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            var resultado = MessageBox.Show(
                "¿Eliminar esta nota definitivamente?",
                "Eliminar Nota",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                EliminarArchivoAudio();
                App.NoteService.EliminarNota(_viewModel.Id);
                Close();
            }
        }

        // =========================================================
        //     AUDIO AUTÓNOMO — SERVICIO INTEGRADO DIRECTO
        // =========================================================

        private void InicializarAudioService()
        {
            _recBlinkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _recBlinkTimer.Tick += (s, e) =>
            {
                _dotVisible = !_dotVisible;
                DotRec.Opacity = _dotVisible ? 1.0 : 0.2;
            };

            _audioService.RecordingTick += (s, duracion) =>
            {
                Dispatcher.Invoke(() =>
                {
                    TxtTiempoGrabando.Text = duracion.ToString(@"mm\:ss");
                });
            };

            _audioService.PeakLevelChanged += (s, peak) =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    // Animación reactiva de las barras ecualizadoras
                    double baseH = Math.Max(3, Math.Min(16, peak * 36));
                    BarV1.Height = Math.Max(3, baseH * 0.4);
                    BarV2.Height = Math.Max(3, baseH * 0.8);
                    BarV3.Height = Math.Max(3, baseH * 1.0);
                    BarV4.Height = Math.Max(3, baseH * 0.7);
                    BarV5.Height = Math.Max(3, baseH * 0.5);
                });
            };

            _audioService.RecordingCompleted += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    _recBlinkTimer?.Stop();
                    if (File.Exists(_viewModel.RutaAudio))
                    {
                        _viewModel.TieneAudio = true;
                    }
                    ActualizarEstadoPanelAudio();
                });
            };

            _audioService.PlaybackTick += (s, pos) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (!_isUserDraggingSlider && _audioService.PlaybackTotalTime.TotalSeconds > 0)
                    {
                        double pct = (pos.TotalSeconds / _audioService.PlaybackTotalTime.TotalSeconds) * 100.0;
                        SliderAudio.Value = pct;
                        TxtTiempoActualAudio.Text = pos.ToString(@"mm\:ss");
                    }
                });
            };

            _audioService.PlaybackCompleted += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    IconPlayEstado.Data = (Geometry)FindResource("IconPlayGeo");
                    SliderAudio.Value = 0;
                    TxtTiempoActualAudio.Text = "00:00";
                });
            };

            _audioService.ErrorOccurred += (s, errorMsg) =>
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(errorMsg, "TopApp Memo — Audio", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            };

            _transcriptionService.TextRecognized += (s, texto) =>
            {
                Dispatcher.Invoke(() =>
                {
                    InsertarTextoTranscrito(texto);
                });
            };
        }

        private void BtnAudio_Click(object sender, RoutedEventArgs e)
        {
            if (PanelAudio.Visibility == Visibility.Visible)
            {
                PanelAudio.Visibility = Visibility.Collapsed;
            }
            else
            {
                PanelAudio.Visibility = Visibility.Visible;
                ActualizarEstadoPanelAudio();

                // Si ya tiene audio y no está reproduciendo, reproducir directamente
                if (_viewModel.TieneAudio && !_audioService.IsPlaying)
                {
                    BtnPlayPausa_Click(this, new RoutedEventArgs());
                }
            }
        }

        private void BtnCerrarAudio_Click(object sender, RoutedEventArgs e)
        {
            if (_audioService.IsRecording)
            {
                _audioService.StopRecording();
            }
            if (_transcriptionService.IsListening)
            {
                _transcriptionService.StopListening();
            }
            if (_audioService.IsPlaying)
            {
                _audioService.StopPlayback();
            }
            TxtIndicadorTranscribiendo.Visibility = Visibility.Collapsed;
            PanelAudio.Visibility = Visibility.Collapsed;
        }

        private void ActualizarEstadoPanelAudio()
        {
            if (_audioService.IsRecording)
            {
                SubPanelListoGrabar.Visibility = Visibility.Collapsed;
                SubPanelGrabando.Visibility = Visibility.Visible;
                SubPanelReproductor.Visibility = Visibility.Collapsed;
            }
            else if (_viewModel.TieneAudio && File.Exists(_viewModel.RutaAudio))
            {
                SubPanelListoGrabar.Visibility = Visibility.Collapsed;
                SubPanelGrabando.Visibility = Visibility.Collapsed;
                SubPanelReproductor.Visibility = Visibility.Visible;

                var dur = AudioRecordingService.GetDuration(_viewModel.RutaAudio);
                TxtTiempoTotalAudio.Text = dur.ToString(@"mm\:ss");
                TxtTiempoActualAudio.Text = "00:00";
                SliderAudio.Value = 0;
                IconPlayEstado.Data = (Geometry)FindResource("IconPlayGeo");
            }
            else
            {
                SubPanelListoGrabar.Visibility = Visibility.Visible;
                SubPanelGrabando.Visibility = Visibility.Collapsed;
                SubPanelReproductor.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnIniciarGrabacion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var carpetaAudio = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Audio");
                if (!Directory.Exists(carpetaAudio))
                    Directory.CreateDirectory(carpetaAudio);

                var rutaDestino = Path.Combine(carpetaAudio, $"{_viewModel.Id}.wav");

                if (_audioService.StartRecording(rutaDestino))
                {
                    _viewModel.RutaAudio = rutaDestino;
                    _viewModel.TieneAudio = false; // Se marca true al terminar
                    SubPanelListoGrabar.Visibility = Visibility.Collapsed;
                    SubPanelGrabando.Visibility = Visibility.Visible;
                    SubPanelReproductor.Visibility = Visibility.Collapsed;
                    _recBlinkTimer?.Start();

                    if (ChkTranscribir.IsChecked == true)
                    {
                        bool ok = _transcriptionService.StartListening();
                        TxtIndicadorTranscribiendo.Visibility = ok ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else
                    {
                        TxtIndicadorTranscribiendo.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error iniciando grabación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDetenerGrabacion_Click(object sender, RoutedEventArgs e)
        {
            _audioService.StopRecording();
            _transcriptionService.StopListening();
            TxtIndicadorTranscribiendo.Visibility = Visibility.Collapsed;
            _recBlinkTimer?.Stop();
        }

        private void BtnCancelarGrabacion_Click(object sender, RoutedEventArgs e)
        {
            _audioService.CancelRecording();
            _transcriptionService.StopListening();
            TxtIndicadorTranscribiendo.Visibility = Visibility.Collapsed;
            _recBlinkTimer?.Stop();
            _viewModel.TieneAudio = false;
            ActualizarEstadoPanelAudio();
        }

        private void InsertarTextoTranscrito(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return;

            var caret = Editor.CaretPosition ?? Editor.Document.ContentEnd;
            string textoAInsertar = texto.Trim();

            var textoPrevio = new TextRange(Editor.Document.ContentStart, caret).Text;
            if (!string.IsNullOrEmpty(textoPrevio) && !textoPrevio.EndsWith(" ") && !textoPrevio.EndsWith("\r\n") && !textoPrevio.EndsWith("\n"))
            {
                textoAInsertar = " " + textoAInsertar;
            }

            caret.InsertTextInRun(textoAInsertar + " ");
            Editor.CaretPosition = Editor.Document.ContentEnd;
            Editor.ScrollToEnd();

            var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            _viewModel.TextoPlano = range.Text.Trim();
            GuardarTextoRtf();
        }

        private void BtnPlayPausa_Click(object sender, RoutedEventArgs e)
        {
            if (_audioService.IsPlaying)
            {
                _audioService.Pause();
                IconPlayEstado.Data = (Geometry)FindResource("IconPlayGeo");
            }
            else
            {
                if (File.Exists(_viewModel.RutaAudio))
                {
                    if (_audioService.Play(_viewModel.RutaAudio))
                    {
                        IconPlayEstado.Data = (Geometry)FindResource("IconPauseGeo");
                        var dur = _audioService.PlaybackTotalTime;
                        TxtTiempoTotalAudio.Text = dur.ToString(@"mm\:ss");
                    }
                }
                else
                {
                    MessageBox.Show("El archivo de audio no existe en disco.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _viewModel.TieneAudio = false;
                    ActualizarEstadoPanelAudio();
                }
            }
        }

        private void BtnRegrabar_Click(object sender, RoutedEventArgs e)
        {
            _audioService.StopPlayback();
            SubPanelReproductor.Visibility = Visibility.Collapsed;
            SubPanelListoGrabar.Visibility = Visibility.Visible;
        }

        private void BtnEliminarAudio_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("¿Eliminar este mensaje de voz?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                EliminarArchivoAudio();
                ActualizarEstadoPanelAudio();
            }
        }

        private void SliderAudio_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isUserDraggingSlider = true;
        }

        private void SliderAudio_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isUserDraggingSlider = false;
            if (_audioService.PlaybackTotalTime.TotalSeconds > 0)
            {
                double targetSec = (_audioService.PlaybackTotalTime.TotalSeconds * SliderAudio.Value) / 100.0;
                _audioService.Seek(TimeSpan.FromSeconds(targetSec));
            }
        }

        private void EliminarArchivoAudio()
        {
            try
            {
                _audioService.StopPlayback();
                _audioService.StopRecording();

                if (!string.IsNullOrEmpty(_viewModel.RutaAudio) && File.Exists(_viewModel.RutaAudio))
                {
                    File.Delete(_viewModel.RutaAudio);
                }

                _viewModel.RutaAudio = string.Empty;
                _viewModel.TieneAudio = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error eliminando audio: {ex.Message}");
            }
        }

        // =========================================================
        //     ALARMA — CON VENTANA MODERNA Y PERSISTENCIA
        // =========================================================

        private void BtnAlarma_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.AlarmaActiva)
            {
                var resultado = MessageBox.Show(
                    $"Alarma actual programada para:\n{_viewModel.FechaAlarma:dd/MM/yyyy HH:mm}\n\n" +
                    "¿Deseas DESACTIVAR la alarma?\n(Pulsa NO para reprogramarla)",
                    "TopApp Memo — Alarma",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    _viewModel.AlarmaActiva = false;
                    _viewModel.FechaAlarma = null;
                    _viewModel.AlarmaSonada = false;
                    _timerAlarma?.Stop();
                    _timerSonido?.Stop();
                    return;
                }
                else if (resultado == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            // Abrir ventana moderna de configuración de alarma
            var winAlarma = new AlarmWindow(
                _viewModel.FechaAlarma,
                _viewModel.DuracionAlarmaSeg,
                _viewModel.RutaSonidoAlarma)
            {
                Owner = this
            };

            winAlarma.ShowDialog();

            if (winAlarma.Confirmado && winAlarma.FechaSeleccionada != null)
            {
                _viewModel.FechaAlarma = winAlarma.FechaSeleccionada;
                _viewModel.DuracionAlarmaSeg = winAlarma.DuracionSegundos;
                _viewModel.RutaSonidoAlarma = winAlarma.RutaSonido;
                _viewModel.AlarmaActiva = true;
                _viewModel.AlarmaSonada = false;

                IniciarTimerAlarma();

                MessageBox.Show(
                    $"Alarma activada con éxito:\n\n{_viewModel.FechaAlarma:dd/MM/yyyy HH:mm}\n" +
                    $"Duración: {_viewModel.DuracionAlarmaSeg} seg\n" +
                    $"Sonido: {(string.IsNullOrEmpty(_viewModel.RutaSonidoAlarma) ? "Sistema" : Path.GetFileName(_viewModel.RutaSonidoAlarma))}",
                    "TopApp Memo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void IniciarTimerAlarma()
        {
            _timerAlarma?.Stop();

            if (!_viewModel.AlarmaActiva || _viewModel.FechaAlarma == null || _viewModel.AlarmaSonada)
                return;

            _timerAlarma = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _timerAlarma.Tick += (s, e) => ComprobarAlarma();
            _timerAlarma.Start();
            ComprobarAlarma();
        }

        private void ComprobarAlarma()
        {
            if (!_viewModel.AlarmaActiva || _viewModel.FechaAlarma == null || _viewModel.AlarmaSonada)
            {
                _timerAlarma?.Stop();
                return;
            }

            if (DateTime.Now >= _viewModel.FechaAlarma.Value)
            {
                _timerAlarma?.Stop();
                _viewModel.AlarmaSonada = true;

                // Traer al frente
                Topmost = true;
                Activate();
                Focus();

                // Reproducir sonido
                IniciarSonidoAlarma();

                // Parpadeo visual
                var timerParpadeo = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
                int parpadeos = 0;
                int maxParpadeos = _viewModel.DuracionAlarmaSeg * 2;
                timerParpadeo.Tick += (s, ev) =>
                {
                    Opacity = Opacity < 1 ? 1 : 0.3;
                    parpadeos++;
                    if (parpadeos >= maxParpadeos)
                    {
                        timerParpadeo.Stop();
                        Opacity = _viewModel.Opacidad;
                        Topmost = _viewModel.SiempreAlFrente;
                    }
                };
                timerParpadeo.Start();

                var texto = _viewModel.TextoPlano.Length > 70
                    ? _viewModel.TextoPlano.Substring(0, 70) + "..."
                    : _viewModel.TextoPlano;

                MessageBox.Show(
                    $"🔔 ¡RECORDATORIO DE ALARMA!\n\n" +
                    $"{_viewModel.FechaAlarma.Value:dd/MM/yyyy HH:mm}\n\n" +
                    $"{texto}",
                    "TopApp Memo — Alarma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);

                _timerSonido?.Stop();
                _viewModel.AlarmaActiva = false;
            }
        }

        private void IniciarSonidoAlarma()
        {
            int repeticiones = 0;
            int maxRepeticiones = Math.Max(1, _viewModel.DuracionAlarmaSeg);

            _timerSonido = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timerSonido.Tick += (s, ev) =>
            {
                repeticiones++;
                if (repeticiones >= maxRepeticiones)
                {
                    _timerSonido.Stop();
                    return;
                }
                TocarSonido();
            };
            _timerSonido.Start();
            TocarSonido();
        }

        private void TocarSonido()
        {
            try
            {
                if (!string.IsNullOrEmpty(_viewModel.RutaSonidoAlarma) && File.Exists(_viewModel.RutaSonidoAlarma))
                {
                    using var player = new SoundPlayer(_viewModel.RutaSonidoAlarma);
                    player.Play();
                }
                else
                {
                    SystemSounds.Exclamation.Play();
                }
            }
            catch
            {
                SystemSounds.Exclamation.Play();
            }
        }

        // ==================== DIBUJO ====================

        private void BtnDibujar_Checked(object sender, RoutedEventArgs e)
        {
            Ink.EditingMode = InkCanvasEditingMode.Ink;
            Ink.IsHitTestVisible = true;
            Editor.IsEnabled = false;
            PanelHerramientas.Visibility = Visibility.Visible;
        }

        private void BtnDibujar_Unchecked(object sender, RoutedEventArgs e)
        {
            Ink.EditingMode = InkCanvasEditingMode.None;
            Ink.IsHitTestVisible = false;
            Editor.IsEnabled = true;
            PanelHerramientas.Visibility = Visibility.Collapsed;
            GuardarInkData();
        }

        private void SliderGrosor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Ink == null) return;
            Ink.DefaultDrawingAttributes.Width  = e.NewValue;
            Ink.DefaultDrawingAttributes.Height = e.NewValue;
        }

        private void InkNegro_Click(object sender, RoutedEventArgs e)    => SetInkColor(Colors.Black);
        private void InkRojo_Click(object sender, RoutedEventArgs e)     => SetInkColor(Color.FromRgb(239, 68, 68));
        private void InkAzul_Click(object sender, RoutedEventArgs e)     => SetInkColor(Color.FromRgb(59, 130, 246));
        private void InkVerde_Click(object sender, RoutedEventArgs e)    => SetInkColor(Color.FromRgb(16, 185, 129));
        private void InkAmarillo_Click(object sender, RoutedEventArgs e) => SetInkColor(Color.FromRgb(245, 158, 11));

        private void SetInkColor(Color color)
        {
            Ink.DefaultDrawingAttributes.Color = color;
        }

        private void BorrarInk_Click(object sender, RoutedEventArgs e)
        {
            Ink.Strokes.Clear();
            _viewModel.DatosInk = string.Empty;
            App.NoteService.GuardarNotas();
        }

        // ==================== EDITOR TEXTO ====================

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded || Editor == null) return;
            var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            _viewModel.TextoPlano = range.Text.Trim();
        }

        private void GuardarTextoRtf()
        {
            try
            {
                using var ms = new MemoryStream();
                var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
                range.Save(ms, DataFormats.Rtf);
                _viewModel.TextoRtf = Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando RTF: {ex.Message}");
            }
        }

        private void CargarTextoRtf()
        {
            try
            {
                if (string.IsNullOrEmpty(_viewModel.TextoRtf)) return;
                var bytes = Convert.FromBase64String(_viewModel.TextoRtf);
                using var ms = new MemoryStream(bytes);
                var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
                range.Load(ms, DataFormats.Rtf);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando RTF: {ex.Message}");
            }
        }

        // ==================== PERSISTENCIA INK ====================

        private void GuardarInkData()
        {
            try
            {
                if (Ink.Strokes.Count == 0)
                {
                    _viewModel.DatosInk = string.Empty;
                    return;
                }
                using var ms = new MemoryStream();
                Ink.Strokes.Save(ms);
                _viewModel.DatosInk = Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando Ink: {ex.Message}");
            }
        }

        private void CargarInkData()
        {
            try
            {
                if (string.IsNullOrEmpty(_viewModel.DatosInk)) return;
                var bytes = Convert.FromBase64String(_viewModel.DatosInk);
                using var ms = new MemoryStream(bytes);
                var coleccion = new StrokeCollection(ms);
                Ink.Strokes.Clear();
                Ink.Strokes.Add(coleccion);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando Ink: {ex.Message}");
            }
        }

        // ==================== VIEWMODEL CHANGES ====================

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(NoteViewModel.SiempreAlFrente):
                    Topmost = _viewModel.SiempreAlFrente;
                    WindowHelper.SetSiempreAlFrente(this, _viewModel.SiempreAlFrente);
                    break;
                case nameof(NoteViewModel.Opacidad):
                    Opacity = _viewModel.Opacidad;
                    break;
                case nameof(NoteViewModel.EsVisible):
                    if (!_viewModel.EsVisible) Close();
                    break;
                case nameof(NoteViewModel.IndiceFondo):
                case nameof(NoteViewModel.IntensidadColor):
                case nameof(NoteViewModel.BrushFondo):
                    FondoPostit.Background = _viewModel.BrushFondo;
                    FondoPostit.BorderBrush = _viewModel.BrushBorde;
                    ActualizarUIIntensidad();
                    break;
            }
        }
    }
}