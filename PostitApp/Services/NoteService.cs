using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PostitApp.Models;
using PostitApp.Helpers;

namespace PostitApp.Services
{
    /// <summary>
    /// Servicio principal de gestión de notas
    /// Maneja la persistencia, creación y eliminación de notas
    /// </summary>
    public class NoteService
    {
        // ==================== CAMPOS ====================

        /// <summary>Ruta del archivo JSON donde se guardan las notas</summary>
        private readonly string _rutaArchivo;

        /// <summary>Configuración de serialización JSON</summary>
        private readonly JsonSerializerSettings _jsonSettings;

        // ==================== EVENTOS ====================

        /// <summary>Se dispara cuando la colección de notas cambia</summary>
        public event EventHandler? NotasActualizadas;

        // ==================== PROPIEDADES ====================

        /// <summary>Lista principal de todas las notas</summary>
        public List<NoteModel> Notas { get; private set; } = new();

        // ==================== CONSTRUCTOR ====================

        public NoteService()
        {
            // Definir ruta del archivo JSON en la carpeta de la aplicación
            var carpetaApp = AppDomain.CurrentDomain.BaseDirectory;
            _rutaArchivo = Path.Combine(carpetaApp, "Data", "notes.json");

            // Configurar serialización JSON con formato legible
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-dd HH:mm:ss"
            };

            // Asegurar que existe el directorio Data
            var carpetaData = Path.GetDirectoryName(_rutaArchivo);
            if (!Directory.Exists(carpetaData))
                Directory.CreateDirectory(carpetaData!);
        }

        // ==================== MÉTODOS CRUD ====================

        /// <summary>
        /// Crea una nueva nota con valores por defecto
        /// y la añade a la colección
        /// </summary>
        public NoteModel CrearNota()
        {
            // Generar posición aleatoria en el escritorio
            var posicion = WindowHelper.GenerarPosicionAleatoria(300, 300);

            var nuevaNota = new NoteModel
            {
                Id = Guid.NewGuid().ToString(),
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now,
                PosX = posicion.X,
                PosY = posicion.Y,
                Ancho = 300,
                Alto = 300,
                EsVisible = true,
                IndiceFondo = 0,
                Opacidad = 1.0,
                Fuente = "Segoe UI",
                TamanoFuente = 14
            };

            Notas.Add(nuevaNota);
            NotasActualizadas?.Invoke(this, EventArgs.Empty);
            return nuevaNota;
        }

        /// <summary>
        /// Elimina una nota por su ID
        /// </summary>
        public void EliminarNota(string id)
        {
            var nota = Notas.FirstOrDefault(n => n.Id == id);
            if (nota != null)
            {
                Notas.Remove(nota);
                GuardarNotas();
                NotasActualizadas?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Busca una nota por su ID
        /// </summary>
        public NoteModel? ObtenerNota(string id)
        {
            return Notas.FirstOrDefault(n => n.Id == id);
        }

        /// <summary>
        /// Actualiza los datos de una nota existente
        /// </summary>
        public void ActualizarNota(NoteModel nota)
        {
            var indice = Notas.FindIndex(n => n.Id == nota.Id);
            if (indice >= 0)
            {
                nota.FechaModificacion = DateTime.Now;
                Notas[indice] = nota;
                NotasActualizadas?.Invoke(this, EventArgs.Empty);
            }
        }

        // ==================== PERSISTENCIA ====================

        /// <summary>
        /// Guarda todas las notas en el archivo JSON
        /// </summary>
        public void GuardarNotas()
        {
            try
            {
                var json = JsonConvert.SerializeObject(Notas, _jsonSettings);
                File.WriteAllText(_rutaArchivo, json, System.Text.Encoding.UTF8);
                System.Diagnostics.Debug.WriteLine($"Notas guardadas: {Notas.Count} notas en {_rutaArchivo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar notas: {ex.Message}");
            }
        }

        /// <summary>
        /// Carga las notas desde el archivo JSON
        /// Si el archivo no existe, inicializa la colección vacía
        /// </summary>
        public void CargarNotas()
        {
            try
            {
                if (File.Exists(_rutaArchivo))
                {
                    var json = File.ReadAllText(_rutaArchivo, System.Text.Encoding.UTF8);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var notas = JsonConvert.DeserializeObject<List<NoteModel>>(json, _jsonSettings);
                        Notas = notas ?? new List<NoteModel>();
                        System.Diagnostics.Debug.WriteLine($"Notas cargadas: {Notas.Count} notas");
                    }
                    else
                    {
                        Notas = new List<NoteModel>();
                    }
                }
                else
                {
                    // Crear archivo vacío si no existe
                    Notas = new List<NoteModel>();
                    GuardarNotas();
                }

                NotasActualizadas?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar notas: {ex.Message}");
                Notas = new List<NoteModel>();
            }
        }

        /// <summary>
        /// Exporta todas las notas a un archivo JSON en la ruta especificada
        /// </summary>
        public void ExportarNotas(string rutaDestino)
        {
            try
            {
                var json = JsonConvert.SerializeObject(Notas, _jsonSettings);
                File.WriteAllText(rutaDestino, json, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al exportar notas: {ex.Message}");
            }
        }

        /// <summary>
        /// Importa notas desde un archivo JSON externo
        /// </summary>
        public void ImportarNotas(string rutaOrigen)
        {
            try
            {
                if (File.Exists(rutaOrigen))
                {
                    var json = File.ReadAllText(rutaOrigen, System.Text.Encoding.UTF8);
                    var notasImportadas = JsonConvert.DeserializeObject<List<NoteModel>>(json, _jsonSettings);

                    if (notasImportadas != null)
                    {
                        // Añadir solo las notas que no existen ya
                        foreach (var nota in notasImportadas)
                        {
                            if (!Notas.Any(n => n.Id == nota.Id))
                            {
                                // Generar nuevo ID para evitar conflictos
                                nota.Id = Guid.NewGuid().ToString();
                                Notas.Add(nota);
                            }
                        }

                        GuardarNotas();
                        NotasActualizadas?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al importar notas: {ex.Message}");
            }
        }
    }
}