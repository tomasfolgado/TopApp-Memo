using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PostitApp.Models;

namespace PostitApp.Services
{
    /// <summary>
    /// Servicio para generar y gestionar miniaturas de las notas Post-it
    /// Las miniaturas se usan en la ventana de gestión central
    /// </summary>
    public class ThumbnailService
    {
        // ==================== CONSTANTES ====================

        /// <summary>Ancho de las miniaturas en píxeles</summary>
        private const int ANCHO_MINIATURA = 150;

        /// <summary>Alto de las miniaturas en píxeles</summary>
        private const int ALTO_MINIATURA = 150;

        /// <summary>Calidad de la imagen PNG generada (0-100)</summary>
        private const int CALIDAD_PNG = 85;

        // ==================== CAMPOS ====================

        /// <summary>Carpeta donde se guardan las miniaturas</summary>
        private readonly string _carpetaMiniaturas;

        // ==================== CONSTRUCTOR ====================

        public ThumbnailService()
        {
            // Definir carpeta de miniaturas dentro de la app
            _carpetaMiniaturas = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", "Thumbnails");

            // Crear carpeta si no existe
            if (!Directory.Exists(_carpetaMiniaturas))
                Directory.CreateDirectory(_carpetaMiniaturas);
        }

        // ==================== MÉTODOS PRINCIPALES ====================

        /// <summary>
        /// Genera una miniatura a partir de un elemento visual WPF
        /// y la guarda en disco
        /// </summary>
        public BitmapSource? GenerarMiniatura(UIElement elemento, string notaId)
        {
            try
            {
                // Obtener tamaño real del elemento
                double ancho = elemento.RenderSize.Width;
                double alto = elemento.RenderSize.Height;

                if (ancho <= 0 || alto <= 0)
                    return null;

                // Renderizar el elemento visual
                var renderTarget = new RenderTargetBitmap(
                    (int)ancho,
                    (int)alto,
                    96, 96,
                    PixelFormats.Pbgra32);

                renderTarget.Render(elemento);

                // Escalar al tamaño de miniatura
                var escalaX = ANCHO_MINIATURA / ancho;
                var escalaY = ALTO_MINIATURA / alto;

                var miniatura = new TransformedBitmap(
                    renderTarget,
                    new ScaleTransform(escalaX, escalaY));

                // Guardar en disco
                GuardarMiniatura(miniatura, notaId);

                return miniatura;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al generar miniatura: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Genera una miniatura de texto simple cuando no hay ventana disponible
        /// Se usa para notas archivadas o cerradas
        /// </summary>
        public BitmapSource GenerarMiniaturaTexto(NoteModel nota)
        {
            try
            {
                // Crear un DrawingVisual para renderizar texto
                var drawing = new DrawingVisual();

                using (var context = drawing.RenderOpen())
                {
                    // Fondo con el color de la nota
                    var colorFondo = ObtenerColorFondo(nota.IndiceFondo);
                    context.DrawRectangle(
                        new SolidColorBrush(colorFondo),
                        null,
                        new Rect(0, 0, ANCHO_MINIATURA, ALTO_MINIATURA));

                    // Texto de la nota (primeras líneas)
                    var textoMostrar = nota.TextoPlano.Length > 100
                        ? nota.TextoPlano.Substring(0, 100) + "..."
                        : nota.TextoPlano;

                    if (!string.IsNullOrEmpty(textoMostrar))
                    {
                        var formattedText = new FormattedText(
                            textoMostrar,
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(nota.Fuente),
                            10,
                            Brushes.Black,
                            96);

                        formattedText.MaxTextWidth = ANCHO_MINIATURA - 10;
                        formattedText.MaxTextHeight = ALTO_MINIATURA - 10;

                        context.DrawText(formattedText, new Point(5, 5));
                    }

                    // Borde decorativo
                    context.DrawRectangle(
                        null,
                        new Pen(new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)), 1),
                        new Rect(0, 0, ANCHO_MINIATURA, ALTO_MINIATURA));
                }

                // Renderizar el DrawingVisual
                var renderTarget = new RenderTargetBitmap(
                    ANCHO_MINIATURA, ALTO_MINIATURA,
                    96, 96,
                    PixelFormats.Pbgra32);

                renderTarget.Render(drawing);

                // Guardar en disco
                GuardarMiniatura(renderTarget, nota.Id);

                return renderTarget;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al generar miniatura de texto: {ex.Message}");
                return GenerarMiniaturaVacia();
            }
        }

        /// <summary>
        /// Carga una miniatura guardada desde disco
        /// </summary>
        public BitmapImage? CargarMiniatura(string notaId)
        {
            try
            {
                var ruta = ObtenerRutaMiniatura(notaId);
                if (File.Exists(ruta))
                {
                    var imagen = new BitmapImage();
                    imagen.BeginInit();
                    imagen.CacheOption = BitmapCacheOption.OnLoad;
                    imagen.UriSource = new Uri(ruta);
                    imagen.EndInit();
                    imagen.Freeze();
                    return imagen;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar miniatura: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Elimina la miniatura de una nota del disco
        /// </summary>
        public void EliminarMiniatura(string notaId)
        {
            try
            {
                var ruta = ObtenerRutaMiniatura(notaId);
                if (File.Exists(ruta))
                    File.Delete(ruta);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al eliminar miniatura: {ex.Message}");
            }
        }

        // ==================== MÉTODOS PRIVADOS ====================

        /// <summary>Guarda un BitmapSource como PNG en disco</summary>
        private void GuardarMiniatura(BitmapSource bitmap, string notaId)
        {
            try
            {
                var ruta = ObtenerRutaMiniatura(notaId);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using var stream = File.OpenWrite(ruta);
                encoder.Save(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar miniatura en disco: {ex.Message}");
            }
        }

        /// <summary>Obtiene la ruta completa de la miniatura de una nota</summary>
        private string ObtenerRutaMiniatura(string notaId)
        {
            return Path.Combine(_carpetaMiniaturas, $"{notaId}.png");
        }

        /// <summary>Genera una miniatura vacía de color gris</summary>
        private BitmapSource GenerarMiniaturaVacia()
        {
            var drawing = new DrawingVisual();
            using (var context = drawing.RenderOpen())
            {
                context.DrawRectangle(
                    new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                    null,
                    new Rect(0, 0, ANCHO_MINIATURA, ALTO_MINIATURA));
            }

            var renderTarget = new RenderTargetBitmap(
                ANCHO_MINIATURA, ALTO_MINIATURA,
                96, 96, PixelFormats.Pbgra32);

            renderTarget.Render(drawing);
            return renderTarget;
        }

        /// <summary>
        /// Devuelve el color de fondo según el índice del Post-it
        /// Debe coincidir con los colores de los PNG de Assets/Backgrounds
        /// </summary>
        private Color ObtenerColorFondo(int indiceFondo)
        {
            return indiceFondo switch
            {
                0 => Color.FromRgb(255, 235, 59),  // Amarillo Canario Vivo
                1 => Color.FromRgb(100, 181, 246), // Azul Cielo Vivo
                2 => Color.FromRgb(140, 233, 154), // Verde Menta Vivo
                3 => Color.FromRgb(255, 128, 171), // Rosa Neón Vivo
                4 => Color.FromRgb(206, 147, 216), // Morado Lavanda Vivo
                5 => Color.FromRgb(255, 167, 38),  // Naranja Mandarina Vivo
                6 => Color.FromRgb(77, 208, 225),  // Cyan Turquesa Vivo
                7 => Color.FromRgb(255, 255, 255), // Blanco Puro
                _ => Color.FromRgb(255, 235, 59)
            };
        }
    }
}