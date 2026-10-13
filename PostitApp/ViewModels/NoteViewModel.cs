using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PostitApp.Models;
using PostitApp.Helpers;

namespace PostitApp.ViewModels
{
    public class NoteViewModel : INotifyPropertyChanged
    {
        // ==================== MODELO ====================

        public NoteModel Modelo { get; private set; }

        // ==================== CONSTRUCTOR ====================

        public NoteViewModel(NoteModel modelo)
        {
            Modelo = modelo;
            InicializarComandos();
        }

        // ==================== PROPIEDADES BÁSICAS ====================

        public string Id => Modelo.Id;

        public string TextoRtf
        {
            get => Modelo.TextoRtf;
            set
            {
                if (Modelo.TextoRtf != value)
                {
                    Modelo.TextoRtf = value;
                    Modelo.FechaModificacion = DateTime.Now;
                    OnPropertyChanged();
                }
            }
        }

        public string TextoPlano
        {
            get => Modelo.TextoPlano;
            set
            {
                if (Modelo.TextoPlano != value)
                {
                    Modelo.TextoPlano = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DatosInk
        {
            get => Modelo.DatosInk;
            set
            {
                if (Modelo.DatosInk != value)
                {
                    Modelo.DatosInk = value;
                    Modelo.FechaModificacion = DateTime.Now;
                    OnPropertyChanged();
                }
            }
        }

        // ==================== AUDIO ====================

        public string RutaAudio
        {
            get => Modelo.RutaAudio;
            set
            {
                if (Modelo.RutaAudio != value)
                {
                    Modelo.RutaAudio = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TieneAudio));
                    App.NoteService.GuardarNotas();
                }
            }
        }

        public bool TieneAudio
        {
            get => Modelo.TieneAudio;
            set
            {
                if (Modelo.TieneAudio != value)
                {
                    Modelo.TieneAudio = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BrushMic));
                    App.NoteService.GuardarNotas();
                }
            }
        }

        // ==================== ALARMA ====================

        public DateTime? FechaAlarma
        {
            get => Modelo.FechaAlarma;
            set
            {
                if (Modelo.FechaAlarma != value)
                {
                    Modelo.FechaAlarma = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AlarmaActiva));
                    OnPropertyChanged(nameof(TieneAlarma));
                    OnPropertyChanged(nameof(BrushBell));
                    OnPropertyChanged(nameof(TextoAlarma));
                    App.NoteService.GuardarNotas();
                }
            }
        }

        public bool AlarmaActiva
        {
            get => Modelo.AlarmaActiva;
            set
            {
                if (Modelo.AlarmaActiva != value)
                {
                    Modelo.AlarmaActiva = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TieneAlarma));
                    OnPropertyChanged(nameof(BrushBell));
                    OnPropertyChanged(nameof(TextoAlarma));
                    App.NoteService.GuardarNotas();
                }
            }
        }

        public bool AlarmaSonada
        {
            get => Modelo.AlarmaSonada;
            set
            {
                if (Modelo.AlarmaSonada != value)
                {
                    Modelo.AlarmaSonada = value;
                    OnPropertyChanged();
                    App.NoteService.GuardarNotas();
                }
            }
        }

        /// <summary>Texto descriptivo de la alarma para tooltips</summary>
        public string TextoAlarma
        {
            get
            {
                if (!AlarmaActiva || FechaAlarma == null)
                    return "Sin alarma";
                return $"Alarma: {FechaAlarma.Value:dd/MM/yyyy HH:mm}";
            }
        }

        public string RutaSonidoAlarma
        {
            get => Modelo.RutaSonidoAlarma;
            set
            {
                if (Modelo.RutaSonidoAlarma != value)
                {
                    Modelo.RutaSonidoAlarma = value;
                    OnPropertyChanged();
                    App.NoteService.GuardarNotas();
                }
            }
        }

        public int DuracionAlarmaSeg
        {
            get => Modelo.DuracionAlarmaSeg;
            set
            {
                if (Modelo.DuracionAlarmaSeg != value)
                {
                    Modelo.DuracionAlarmaSeg = value;
                    OnPropertyChanged();
                    App.NoteService.GuardarNotas();
                }
            }
        }

        public bool TieneAlarma => AlarmaActiva && FechaAlarma != null;

        // ==================== APARIENCIA ====================

        public int IndiceFondo
        {
            get => Modelo.IndiceFondo;
            set
            {
                if (Modelo.IndiceFondo != value)
                {
                    Modelo.IndiceFondo = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RutaFondo));
                    OnPropertyChanged(nameof(ColorFondo));
                    OnPropertyChanged(nameof(ColorBorde));
                    OnPropertyChanged(nameof(ColorCabecera));
                    OnPropertyChanged(nameof(BrushFondo));
                    OnPropertyChanged(nameof(BrushBorde));
                    OnPropertyChanged(nameof(BrushCabecera));
                    App.NoteService.GuardarNotas();
                }
            }
        }

        public double IntensidadColor
        {
            get => Modelo.IntensidadColor;
            set
            {
                if (Math.Abs(Modelo.IntensidadColor - value) > 0.005)
                {
                    Modelo.IntensidadColor = Math.Clamp(value, 0.15, 1.0);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ColorFondo));
                    OnPropertyChanged(nameof(ColorBorde));
                    OnPropertyChanged(nameof(ColorCabecera));
                    OnPropertyChanged(nameof(BrushFondo));
                    OnPropertyChanged(nameof(BrushBorde));
                    OnPropertyChanged(nameof(BrushCabecera));
                    App.NoteService.GuardarNotas();
                }
            }
        }

        public string RutaFondo => Modelo.RutaFondoActual;
        public string ColorFondo => Modelo.ColorFondoActual;
        public string ColorBorde => Modelo.ColorBordeActual;
        public string ColorCabecera => Modelo.ColorCabeceraActual;

        public System.Windows.Media.SolidColorBrush BrushFondo =>
            (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString(ColorFondo)!;

        public System.Windows.Media.SolidColorBrush BrushBorde =>
            (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString(ColorBorde)!;

        public System.Windows.Media.SolidColorBrush BrushCabecera =>
            (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString(ColorCabecera)!;

        public System.Windows.Media.SolidColorBrush BrushPin =>
            SiempreAlFrente ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(217, 119, 6))
                            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 65, 85));

        public System.Windows.Media.SolidColorBrush BrushBell =>
            TieneAlarma ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(217, 119, 6))
                        : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 65, 85));

        public System.Windows.Media.SolidColorBrush BrushMic =>
            TieneAudio ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(5, 150, 105))
                       : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 65, 85));

        public double Opacidad
        {
            get => Modelo.Opacidad;
            set
            {
                double valor = Math.Max(0.2, Math.Min(1.0, value));
                if (Modelo.Opacidad != valor)
                {
                    Modelo.Opacidad = valor;
                    OnPropertyChanged();
                    App.NoteService.GuardarNotas();
                }
            }
        }

        public string Fuente
        {
            get => Modelo.Fuente;
            set
            {
                if (Modelo.Fuente != value)
                {
                    Modelo.Fuente = value;
                    OnPropertyChanged();
                    App.NoteService.GuardarNotas();
                }
            }
        }

        public double TamanoFuente
        {
            get => Modelo.TamanoFuente;
            set
            {
                if (Modelo.TamanoFuente != value)
                {
                    Modelo.TamanoFuente = value;
                    OnPropertyChanged();
                    App.NoteService.GuardarNotas();
                }
            }
        }

        // ==================== POSICIÓN ====================

        public double PosX
        {
            get => Modelo.PosX;
            set { if (Modelo.PosX != value) { Modelo.PosX = value; OnPropertyChanged(); } }
        }

        public double PosY
        {
            get => Modelo.PosY;
            set { if (Modelo.PosY != value) { Modelo.PosY = value; OnPropertyChanged(); } }
        }

        public double Ancho
        {
            get => Modelo.Ancho;
            set { if (Modelo.Ancho != value) { Modelo.Ancho = value; OnPropertyChanged(); } }
        }

        public double Alto
        {
            get => Modelo.Alto;
            set { if (Modelo.Alto != value) { Modelo.Alto = value; OnPropertyChanged(); } }
        }

        // ==================== ESTADO ====================

        public bool EsVisible
        {
            get => Modelo.EsVisible;
            set
            {
                if (Modelo.EsVisible != value)
                {
                    Modelo.EsVisible = value;
                    OnPropertyChanged();
                    App.NoteService.GuardarNotas();
                }
            }
        }

        public bool SiempreAlFrente
        {
            get => Modelo.SiempreAlFrente;
            set
            {
                if (Modelo.SiempreAlFrente != value)
                {
                    Modelo.SiempreAlFrente = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BrushPin));
                    App.NoteService.GuardarNotas();
                }
            }
        }

        public DateTime FechaCreacion => Modelo.FechaCreacion;
        public DateTime FechaModificacion => Modelo.FechaModificacion;

        // ==================== COMANDOS ====================

        public ICommand? CambiarFondoCmd { get; private set; }
        public ICommand? NuevaNotaCmd { get; private set; }
        public ICommand? OcultarNotaCmd { get; private set; }
        public ICommand? EliminarNotaCmd { get; private set; }
        public ICommand? ToggleSiempreAlFrenteCmd { get; private set; }

        private void InicializarComandos()
        {
            CambiarFondoCmd = new RelayCommand<int>(indice =>
            {
                IndiceFondo = indice;
            });

            NuevaNotaCmd = new RelayCommand(() =>
            {
                var nuevaNota = App.NoteService.CrearNota();
                var ventana = new Views.NoteWindow(nuevaNota);
                ventana.Show();
                App.NoteService.GuardarNotas();
            });

            OcultarNotaCmd = new RelayCommand(() =>
            {
                EsVisible = false;
            });

            EliminarNotaCmd = new RelayCommand(() =>
            {
                App.NoteService.EliminarNota(Modelo.Id);
            });

            ToggleSiempreAlFrenteCmd = new RelayCommand(() =>
            {
                SiempreAlFrente = !SiempreAlFrente;
            });
        }

        // ==================== INOTIFYPROPERTYCHANGED ====================

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
        }
    }
}