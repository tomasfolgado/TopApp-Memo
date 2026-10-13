using System;
using Newtonsoft.Json;

namespace PostitApp.Models
{
    public class NoteModel
    {
        // ==================== IDENTIFICACIÓN ====================

        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("fechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [JsonProperty("fechaModificacion")]
        public DateTime FechaModificacion { get; set; } = DateTime.Now;

        // ==================== CONTENIDO ====================

        [JsonProperty("textoRtf")]
        public string TextoRtf { get; set; } = string.Empty;

        [JsonProperty("textoPlano")]
        public string TextoPlano { get; set; } = string.Empty;

        [JsonProperty("datosInk")]
        public string DatosInk { get; set; } = string.Empty;

        // ==================== AUDIO ====================

        /// <summary>Ruta del archivo de audio grabado</summary>
        [JsonProperty("rutaAudio")]
        public string RutaAudio { get; set; } = string.Empty;

        /// <summary>Indica si tiene audio grabado</summary>
        [JsonProperty("tieneAudio")]
        public bool TieneAudio { get; set; } = false;

        // ==================== ALARMA ====================

        /// <summary>Fecha y hora de la alarma</summary>
        [JsonProperty("fechaAlarma")]
        public DateTime? FechaAlarma { get; set; } = null;

        /// <summary>Indica si la alarma está activa</summary>
        [JsonProperty("alarmaActiva")]
        public bool AlarmaActiva { get; set; } = false;

        /// <summary>Indica si la alarma ya sonó</summary>
        [JsonProperty("alarmaSonada")]
        public bool AlarmaSonada { get; set; } = false;

        /// <summary>Ruta del sonido personalizado para la alarma</summary>
        [JsonProperty("rutaSonidoAlarma")]
        public string RutaSonidoAlarma { get; set; } = string.Empty;

        /// <summary>Duración del sonido de la alarma en segundos</summary>
        [JsonProperty("duracionAlarmaSeg")]
        public int DuracionAlarmaSeg { get; set; } = 10;

        // ==================== APARIENCIA ====================

        [JsonProperty("indiceFondo")]
        public int IndiceFondo { get; set; } = 0;

        /// <summary>Intensidad / saturación del color (0.15 muy suave a 1.0 máxima viveza)</summary>
        [JsonProperty("intensidadColor")]
        public double IntensidadColor { get; set; } = 0.85;

        [JsonProperty("opacidad")]
        public double Opacidad { get; set; } = 1.0;

        [JsonProperty("fuente")]
        public string Fuente { get; set; } = "Segoe UI";

        [JsonProperty("tamanoFuente")]
        public double TamanoFuente { get; set; } = 14;

        // ==================== POSICIÓN Y TAMAÑO ====================

        [JsonProperty("posX")]
        public double PosX { get; set; } = 100;

        [JsonProperty("posY")]
        public double PosY { get; set; } = 100;

        [JsonProperty("ancho")]
        public double Ancho { get; set; } = 300;

        [JsonProperty("alto")]
        public double Alto { get; set; } = 300;

        // ==================== ESTADO ====================

        [JsonProperty("esVisible")]
        public bool EsVisible { get; set; } = true;

        [JsonProperty("siempreAlFrente")]
        public bool SiempreAlFrente { get; set; } = false;

        [JsonProperty("eliminada")]
        public bool Eliminada { get; set; } = false;

        // ==================== PALETA DE COLORES VIVOS ====================

        public static readonly (byte R, byte G, byte B)[] ColoresBaseRGB = new (byte, byte, byte)[]
        {
            (255, 235, 59),  // 0: Amarillo Canario Vivo
            (100, 181, 246), // 1: Azul Cielo Vivo
            (140, 233, 154), // 2: Verde Menta / Lima Vivo
            (255, 128, 171), // 3: Rosa / Fucsia Vivo
            (206, 147, 216), // 4: Morado / Lavanda Vivo
            (255, 167, 38),  // 5: Naranja Mandarina Vivo
            (77,  208, 225), // 6: Cyan Turquesa Vivo
            (255, 255, 255)  // 7: Blanco Puro Nieve
        };

        public static readonly string[] ColoresFondoHex = new string[]
        {
            "#FFEB3B", "#64B5F6", "#8CE99A", "#FF80AB", "#CE93D8", "#FFA726", "#4DD0E1", "#FFFFFF"
        };

        public static readonly string[] ColoresBordeHex = new string[]
        {
            "#FBC02D", "#1E88E5", "#40C057", "#F50057", "#8E24AA", "#F57C00", "#00ACC1", "#CBD5E1"
        };

        public static readonly string[] ColoresCabeceraHex = new string[]
        {
            "#FDD835", "#42A5F5", "#51CF66", "#FF4081", "#AB47BC", "#FB8C00", "#26C6DA", "#F1F5F9"
        };

        public static string CalcularColorFondoHex(int indice, double intensidad)
        {
            int idx = Math.Clamp(indice, 0, ColoresBaseRGB.Length - 1);
            if (idx == 7) return "#FFFFFF";
            double i = Math.Clamp(intensidad, 0.15, 1.0);
            var baseC = ColoresBaseRGB[idx];

            byte r = (byte)Math.Clamp((int)(255 - (255 - baseC.R) * i), 0, 255);
            byte g = (byte)Math.Clamp((int)(255 - (255 - baseC.G) * i), 0, 255);
            byte b = (byte)Math.Clamp((int)(255 - (255 - baseC.B) * i), 0, 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public static string CalcularColorBordeHex(int indice, double intensidad)
        {
            int idx = Math.Clamp(indice, 0, ColoresBaseRGB.Length - 1);
            if (idx == 7) return "#CBD5E1";
            double i = Math.Min(1.0, Math.Clamp(intensidad, 0.15, 1.0) * 1.25);
            var baseC = ColoresBaseRGB[idx];

            byte r = (byte)Math.Clamp((int)(baseC.R * 0.85 * i + (255 * (1 - i) * 0.6)), 0, 255);
            byte g = (byte)Math.Clamp((int)(baseC.G * 0.85 * i + (255 * (1 - i) * 0.6)), 0, 255);
            byte b = (byte)Math.Clamp((int)(baseC.B * 0.85 * i + (255 * (1 - i) * 0.6)), 0, 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public static string CalcularColorCabeceraHex(int indice, double intensidad)
        {
            int idx = Math.Clamp(indice, 0, ColoresBaseRGB.Length - 1);
            if (idx == 7) return "#F1F5F9";
            double i = Math.Min(1.0, Math.Clamp(intensidad, 0.15, 1.0) * 1.15);
            var baseC = ColoresBaseRGB[idx];

            byte r = (byte)Math.Clamp((int)(255 - (255 - baseC.R) * i), 0, 255);
            byte g = (byte)Math.Clamp((int)(255 - (255 - baseC.G) * i), 0, 255);
            byte b = (byte)Math.Clamp((int)(255 - (255 - baseC.B) * i), 0, 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public static string[] RutasFondos { get; } = new string[]
        {
            "pack://application:,,,/Assets/Backgrounds/posit_yellow.png",
            "pack://application:,,,/Assets/Backgrounds/posit_blue.png",
            "pack://application:,,,/Assets/Backgrounds/posit_green.png",
            "pack://application:,,,/Assets/Backgrounds/posit_red.png",
            "pack://application:,,,/Assets/Backgrounds/posit_purple.png",
            "pack://application:,,,/Assets/Backgrounds/posit_orange.png",
            "pack://application:,,,/Assets/Backgrounds/posit_cyan.png",
            "pack://application:,,,/Assets/Backgrounds/posit_white.png"
        };

        [JsonIgnore]
        public string RutaFondoActual => RutasFondos[Math.Clamp(IndiceFondo, 0, RutasFondos.Length - 1)];

        [JsonIgnore]
        public string ColorFondoActual => CalcularColorFondoHex(IndiceFondo, IntensidadColor);

        [JsonIgnore]
        public string ColorBordeActual => CalcularColorBordeHex(IndiceFondo, IntensidadColor);

        [JsonIgnore]
        public string ColorCabeceraActual => CalcularColorCabeceraHex(IndiceFondo, IntensidadColor);
    }
}