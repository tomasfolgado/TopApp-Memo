using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using PostitApp.Models;
using PostitApp.Helpers;
using PostitApp.Services;

namespace PostitApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ==================== SERVICIO ====================

        private readonly NoteService _noteService;
        private readonly AudioRecordingService _previewAudioService = new();
        private string? _audioPreviewPlayingId;

        // ==================== CONSTRUCTOR ====================

        public MainViewModel()
        {
            _noteService = App.NoteService;
            InicializarComandos();
            ActualizarColeccion();
            _noteService.NotasActualizadas += OnNotasActualizadas;
            LocalizationService.Instance.LanguageChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(CurrentLanguage));
                OnPropertyChanged(nameof(IsSpanish));
                OnPropertyChanged(nameof(IsEnglish));
            };
        }

        // ==================== COLECCIÓN ====================

        private ObservableCollection<NoteViewModel> _notas = new();
        public ObservableCollection<NoteViewModel> Notas
        {
            get => _notas;
            set
            {
                _notas = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalNotas));
                OnPropertyChanged(nameof(NotasVisibles));
                OnPropertyChanged(nameof(NotasOcultas));
                OnPropertyChanged(nameof(NotasConAudio));
                OnPropertyChanged(nameof(NotasConAlarma));
                OnPropertyChanged(nameof(HayNotas));
            }
        }

        public bool HayNotas => Notas.Count > 0;

        // ==================== ESTADÍSTICAS ====================

        public int TotalNotas => _noteService.Notas.Count;
        public int NotasVisibles => _noteService.Notas.Count(n => n.EsVisible);
        public int NotasOcultas => _noteService.Notas.Count(n => !n.EsVisible);
        public int NotasConAudio => _noteService.Notas.Count(n => n.TieneAudio);
        public int NotasConAlarma => _noteService.Notas.Count(n => n.AlarmaActiva);

        // ==================== BÚSQUEDA Y FILTRADO ====================

        private string _textoBusqueda = string.Empty;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                ActualizarColeccion();
            }
        }

        private string _filtroActivo = "todas";
        public string FiltroActivo
        {
            get => _filtroActivo;
            set
            {
                if (_filtroActivo != value)
                {
                    _filtroActivo = value;
                    OnPropertyChanged();
                    ActualizarColeccion();
                }
            }
        }

        // ==================== COMANDOS ====================

        public ICommand? NuevaNotaCmd { get; private set; }
        public ICommand? AbrirNotaCmd { get; private set; }
        public ICommand? EliminarNotaCmd { get; private set; }
        public ICommand? MostrarNotaCmd { get; private set; }
        public ICommand? OcultarNotaCmd { get; private set; }
        public ICommand? ToggleVisibilidadCmd { get; private set; }
        public ICommand? MostrarTodasCmd { get; private set; }
        public ICommand? OcultarTodasCmd { get; private set; }
        public ICommand? GuardarTodasCmd { get; private set; }
        public ICommand? FiltrarCmd { get; private set; }
        public ICommand? ReproducirAudioPreviewCmd { get; private set; }
        public ICommand? LimpiarBusquedaCmd { get; private set; }
        public string CurrentLanguage => LocalizationService.Instance.CurrentLanguage;
        public bool IsSpanish => CurrentLanguage == "es";
        public bool IsEnglish => CurrentLanguage == "en";
        public ICommand? CambiarIdiomaCmd { get; private set; }
        public ICommand? HelpCmd { get; private set; }

        private void InicializarComandos()
        {
            NuevaNotaCmd = new RelayCommand(() =>
            {
                var modelo = _noteService.CrearNota();
                var ventana = new Views.NoteWindow(modelo);
                ventana.Show();
                ActualizarColeccion();
                _noteService.GuardarNotas();
            });

            AbrirNotaCmd = new RelayCommand<NoteViewModel>(vm =>
            {
                if (vm == null) return;
                var ventanaExistente = BuscarVentanaNota(vm.Id);
                if (ventanaExistente != null)
                {
                    ventanaExistente.Activate();
                    ventanaExistente.Focus();
                    return;
                }

                vm.EsVisible = true;
                var nuevaVentana = new Views.NoteWindow(vm.Modelo);
                nuevaVentana.Show();
                ActualizarColeccion();
                _noteService.GuardarNotas();
            });

            MostrarNotaCmd = new RelayCommand<NoteViewModel>(vm =>
            {
                if (vm == null) return;
                var ventanaExistente = BuscarVentanaNota(vm.Id);
                if (ventanaExistente != null)
                {
                    ventanaExistente.Activate();
                    ventanaExistente.Focus();
                    return;
                }

                vm.EsVisible = true;
                var ventana = new Views.NoteWindow(vm.Modelo);
                ventana.Show();
                ActualizarColeccion();
                _noteService.GuardarNotas();
            });

            OcultarNotaCmd = new RelayCommand<NoteViewModel>(vm =>
            {
                if (vm == null) return;
                var ventanaExistente = BuscarVentanaNota(vm.Id);
                ventanaExistente?.Close();
                vm.EsVisible = false;
                ActualizarColeccion();
                _noteService.GuardarNotas();
            });

            ToggleVisibilidadCmd = new RelayCommand<NoteViewModel>(vm =>
            {
                if (vm == null) return;
                if (vm.EsVisible)
                {
                    var ventanaExistente = BuscarVentanaNota(vm.Id);
                    ventanaExistente?.Close();
                    vm.EsVisible = false;
                }
                else
                {
                    vm.EsVisible = true;
                    var ventana = new Views.NoteWindow(vm.Modelo);
                    ventana.Show();
                }
                ActualizarColeccion();
                _noteService.GuardarNotas();
            });

            MostrarTodasCmd = new RelayCommand(() =>
            {
                foreach (var modelo in _noteService.Notas)
                {
                    modelo.EsVisible = true;
                    if (BuscarVentanaNota(modelo.Id) == null)
                    {
                        var win = new Views.NoteWindow(modelo);
                        win.Show();
                    }
                }
                ActualizarColeccion();
                _noteService.GuardarNotas();
            });

            OcultarTodasCmd = new RelayCommand(() =>
            {
                foreach (Window win in Application.Current.Windows.Cast<Window>().ToList())
                {
                    if (win is Views.NoteWindow nw)
                    {
                        nw.Close();
                    }
                }
                foreach (var modelo in _noteService.Notas)
                {
                    modelo.EsVisible = false;
                }
                ActualizarColeccion();
                _noteService.GuardarNotas();
            });

            EliminarNotaCmd = new RelayCommand<NoteViewModel>(vm =>
            {
                if (vm == null) return;
                var preg = LocalizationService.Instance.GetString("Str_MsgEliminarPregunta", "¿Estás seguro de que quieres eliminar esta nota definitivamente?");
                var tit = LocalizationService.Instance.GetString("Str_MsgEliminarTitulo", "TopApp Memo — Eliminar Nota");
                var resultado = MessageBox.Show(preg, tit, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    BuscarVentanaNota(vm.Id)?.Close();
                    _previewAudioService.StopPlayback();

                    if (!string.IsNullOrEmpty(vm.RutaAudio) && File.Exists(vm.RutaAudio))
                    {
                        try { File.Delete(vm.RutaAudio); } catch { }
                    }

                    _noteService.EliminarNota(vm.Id);
                    ActualizarColeccion();
                }
            });

            GuardarTodasCmd = new RelayCommand(() =>
            {
                _noteService.GuardarNotas();
                var msg = LocalizationService.Instance.GetString("Str_MsgGuardadas", "Todas las notas han sido guardadas correctamente.");
                MessageBox.Show(
                    msg,
                    "TopApp Memo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });

            FiltrarCmd = new RelayCommand<string>(filtro =>
            {
                if (!string.IsNullOrEmpty(filtro))
                {
                    FiltroActivo = filtro;
                }
            });

            LimpiarBusquedaCmd = new RelayCommand(() =>
            {
                TextoBusqueda = string.Empty;
            });

            CambiarIdiomaCmd = new RelayCommand<string>(lang =>
            {
                if (!string.IsNullOrEmpty(lang))
                {
                    LocalizationService.Instance.SetLanguage(lang);
                }
                else
                {
                    var nuevo = LocalizationService.Instance.CurrentLanguage == "es" ? "en" : "es";
                    LocalizationService.Instance.SetLanguage(nuevo);
                }
            });
            HelpCmd = new RelayCommand(() =>
            {
                var winAyuda = new Views.HelpWindow();
                Window? owner = null;
                foreach (Window w in Application.Current.Windows)
                {
                    if (w is Views.ManagerWindow)
                    {
                        owner = w;
                        break;
                    }
                }

                if (owner != null)
                {
                    winAyuda.Owner = owner;
                }
                else
                {
                    winAyuda.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                winAyuda.ShowDialog();
            });

            ReproducirAudioPreviewCmd = new RelayCommand<NoteViewModel>(vm =>
            {
                if (vm == null || !vm.TieneAudio || !File.Exists(vm.RutaAudio)) return;

                if (_previewAudioService.IsPlaying && _audioPreviewPlayingId == vm.Id)
                {
                    _previewAudioService.StopPlayback();
                    _audioPreviewPlayingId = null;
                }
                else
                {
                    _audioPreviewPlayingId = vm.Id;
                    _previewAudioService.Play(vm.RutaAudio);
                }
            });
        }

        private Views.NoteWindow? BuscarVentanaNota(string noteId)
        {
            foreach (Window ventana in Application.Current.Windows)
            {
                if (ventana is Views.NoteWindow nw && nw.NoteId == noteId)
                {
                    return nw;
                }
            }
            return null;
        }

        public void ActualizarColeccion()
        {
            var lista = new ObservableCollection<NoteViewModel>();
            foreach (var modelo in _noteService.Notas)
            {
                // Filtro de texto
                bool coincideTexto = string.IsNullOrEmpty(_textoBusqueda) ||
                                     modelo.TextoPlano.Contains(_textoBusqueda, StringComparison.OrdinalIgnoreCase);

                if (!coincideTexto) continue;

                // Filtro de categoría
                bool coincideCategoria = _filtroActivo switch
                {
                    "visibles" => modelo.EsVisible,
                    "ocultas"  => !modelo.EsVisible,
                    "audio"    => modelo.TieneAudio,
                    "alarma"   => modelo.AlarmaActiva,
                    _          => true
                };

                if (coincideCategoria)
                {
                    lista.Add(new NoteViewModel(modelo));
                }
            }
            Notas = lista;
        }

        private void OnNotasActualizadas(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ActualizarColeccion();
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
        }
    }
}