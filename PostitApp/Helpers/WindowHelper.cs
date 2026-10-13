using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PostitApp.Helpers
{
    /// <summary>
    /// Clase de ayuda para operaciones con ventanas de Windows
    /// Contiene métodos para mover, redimensionar y capturar ventanas
    /// </summary>
    public static class WindowHelper
    {
        // ==================== WIN32 API ====================

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        // Constantes Win32
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;

        // ==================== MÉTODOS DE VENTANA ====================

        /// <summary>
        /// Permite arrastrar una ventana sin barra de título
        /// Llamar desde el evento MouseLeftButtonDown de cualquier elemento
        /// </summary>
        public static void ArrastrarVentana(Window ventana)
        {
            try
            {
                var handle = new WindowInteropHelper(ventana).Handle;
                ReleaseCapture();
                SendMessage(handle, WM_NCLBUTTONDOWN, new IntPtr(HT_CAPTION), IntPtr.Zero);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al arrastrar ventana: {ex.Message}");
            }
        }

        /// <summary>
        /// Establece si una ventana siempre aparece al frente de las demás
        /// </summary>
        public static void SetSiempreAlFrente(Window ventana, bool alFrente)
        {
            try
            {
                var handle = new WindowInteropHelper(ventana).Handle;
                var insertAfter = alFrente ? HWND_TOPMOST : HWND_NOTOPMOST;
                SetWindowPos(handle, insertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al establecer siempre al frente: {ex.Message}");
            }
        }

        /// <summary>
        /// Captura una miniatura de una ventana WPF como BitmapSource
        /// </summary>
        public static BitmapSource? CapturarMiniatura(Window ventana, int ancho, int alto)
        {
            try
            {
                // Renderizar el contenido visual de la ventana
                var renderTarget = new RenderTargetBitmap(
                    (int)ventana.ActualWidth,
                    (int)ventana.ActualHeight,
                    96, 96,
                    PixelFormats.Pbgra32);

                renderTarget.Render(ventana);

                // Escalar al tamaño de miniatura deseado
                var escala = new TransformedBitmap(renderTarget,
                    new ScaleTransform(
                        ancho / ventana.ActualWidth,
                        alto / ventana.ActualHeight));

                return escala;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al capturar miniatura: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Centra una ventana en la pantalla principal
        /// </summary>
        public static void CentrarEnPantalla(Window ventana)
        {
            var pantalla = SystemParameters.WorkArea;
            ventana.Left = (pantalla.Width - ventana.Width) / 2;
            ventana.Top = (pantalla.Height - ventana.Height) / 2;
        }

        /// <summary>
        /// Asegura que una ventana no quede fuera de los límites del escritorio
        /// </summary>
        public static void AsegurarDentroDelEscritorio(Window ventana)
        {
            var pantalla = SystemParameters.WorkArea;

            // Ajustar posición X
            if (ventana.Left < 0)
                ventana.Left = 0;
            if (ventana.Left + ventana.Width > pantalla.Width)
                ventana.Left = pantalla.Width - ventana.Width;

            // Ajustar posición Y
            if (ventana.Top < 0)
                ventana.Top = 0;
            if (ventana.Top + ventana.Height > pantalla.Height)
                ventana.Top = pantalla.Height - ventana.Height;
        }

        /// <summary>
        /// Genera una posición aleatoria dentro del escritorio para una nueva nota
        /// </summary>
        public static Point GenerarPosicionAleatoria(double anchoVentana, double altoVentana)
        {
            var pantalla = SystemParameters.WorkArea;
            var rnd = new Random();

            double x = rnd.NextDouble() * (pantalla.Width - anchoVentana);
            double y = rnd.NextDouble() * (pantalla.Height - altoVentana);

            return new Point(Math.Max(0, x), Math.Max(0, y));
        }
    }
}