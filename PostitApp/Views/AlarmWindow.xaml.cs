using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PostitApp.Helpers;
using PostitApp.Services;

namespace PostitApp.Views
{
    public partial class AlarmWindow : Window
    {
        public DateTime? FechaSeleccionada { get; private set; }
        public int DuracionSegundos { get; private set; } = 10;
        public string RutaSonido { get; private set; } = string.Empty;
        public bool Confirmado { get; private set; } = false;

        public AlarmWindow(DateTime? fechaActual = null, int duracionActual = 10, string sonidoActual = "")
        {
            InitializeComponent();

            var fechaBase = fechaActual ?? DateTime.Now.AddHours(1);
            DpFecha.SelectedDate = fechaBase.Date;
            TxtHora.Text = fechaBase.ToString("HH");
            TxtMinuto.Text = fechaBase.ToString("mm");

            DuracionSegundos = duracionActual > 0 ? duracionActual : 10;
            SliderDuracion.Value = DuracionSegundos;
            LblDuracion.Text = $"{DuracionSegundos} {LocalizationService.Instance.GetString("Str_AlarmSegundos", "segundos")}";

            RutaSonido = sonidoActual;
            if (!string.IsNullOrEmpty(RutaSonido) && File.Exists(RutaSonido))
            {
                LblSonido.Text = Path.GetFileName(RutaSonido);
            }
            else
            {
                LblSonido.Text = LocalizationService.Instance.GetString("Str_AlarmSonidoSistema", "Sonido del sistema");
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowHelper.ArrastrarVentana(this);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = false;
            Close();
        }

        private void SliderDuracion_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LblDuracion == null) return;
            DuracionSegundos = (int)e.NewValue;
            LblDuracion.Text = $"{DuracionSegundos} {LocalizationService.Instance.GetString("Str_AlarmSegundos", "segundos")}";
        }

        private void Preset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                DateTime objetivo;
                if (tag == "tomorrow")
                {
                    objetivo = DateTime.Today.AddDays(1).AddHours(9);
                }
                else if (int.TryParse(tag, out int minutos))
                {
                    objetivo = DateTime.Now.AddMinutes(minutos);
                }
                else
                {
                    return;
                }

                DpFecha.SelectedDate = objetivo.Date;
                TxtHora.Text = objetivo.ToString("HH");
                TxtMinuto.Text = objetivo.ToString("mm");
            }
        }

        private void BtnElegirSonido_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = LocalizationService.Instance.GetString("Str_AlarmDialogTitle", "Seleccionar sonido de alarma"),
                Filter = LocalizationService.Instance.GetString("Str_AlarmDialogFiltro", "Archivos WAV (*.wav)|*.wav|Todos los archivos (*.*)|*.*"),
                InitialDirectory = @"C:\Windows\Media"
            };

            if (dialog.ShowDialog() == true)
            {
                RutaSonido = dialog.FileName;
                LblSonido.Text = Path.GetFileName(RutaSonido);
                ProbarSonido();
            }
        }

        private void BtnProbarSonido_Click(object sender, RoutedEventArgs e)
        {
            ProbarSonido();
        }

        private void ProbarSonido()
        {
            try
            {
                if (!string.IsNullOrEmpty(RutaSonido) && File.Exists(RutaSonido))
                {
                    using var player = new SoundPlayer(RutaSonido);
                    player.Play();
                }
                else
                {
                    SystemSounds.Exclamation.Play();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{LocalizationService.Instance.GetString("Str_AlarmErrorReproducir", "No se pudo reproducir")}: {ex.Message}", LocalizationService.Instance.GetString("Str_MsgAudioAviso", "Aviso"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnActivar_Click(object sender, RoutedEventArgs e)
        {
            if (DpFecha.SelectedDate == null)
            {
                MessageBox.Show(LocalizationService.Instance.GetString("Str_AlarmValidaFecha", "Por favor selecciona una fecha."), LocalizationService.Instance.GetString("Str_AlarmTitle", "Alarma"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtHora.Text.Trim(), out int hora) || hora < 0 || hora > 23 ||
                !int.TryParse(TxtMinuto.Text.Trim(), out int minuto) || minuto < 0 || minuto > 59)
            {
                MessageBox.Show(LocalizationService.Instance.GetString("Str_AlarmValidaHora", "Introduce una hora y minutos válidos (00-23 : 00-59)."), LocalizationService.Instance.GetString("Str_AlarmTitle", "Alarma"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var fecha = DpFecha.SelectedDate.Value.Date.AddHours(hora).AddMinutes(minuto);
            if (fecha <= DateTime.Now)
            {
                MessageBox.Show(LocalizationService.Instance.GetString("Str_AlarmValidaFutura", "La fecha y hora de la alarma debe ser futura."), LocalizationService.Instance.GetString("Str_AlarmTitle", "Alarma"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FechaSeleccionada = fecha;
            DuracionSegundos = (int)SliderDuracion.Value;
            Confirmado = true;
            Close();
        }
    }
}
