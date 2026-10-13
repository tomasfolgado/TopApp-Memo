using System;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;

namespace PostitApp.Services
{
    public class LocalizationService
    {
        public static LocalizationService Instance { get; } = new();

        public event EventHandler? LanguageChanged;

        public string CurrentLanguage { get; private set; } = "es";

        private readonly string _configFilePath;

        public LocalizationService()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDir))
            {
                try { Directory.CreateDirectory(dataDir); } catch { }
            }
            _configFilePath = Path.Combine(dataDir, "settings.json");
        }

        public void Initialize()
        {
            string savedLang = "es";
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    var dict = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(json);
                    if (dict != null && dict.TryGetValue("Language", out var val) && (val == "es" || val == "en"))
                    {
                        savedLang = val;
                    }
                }
            }
            catch { }

            SetLanguage(savedLang, saveConfig: false);
        }

        public void SetLanguage(string langCode, bool saveConfig = true)
        {
            if (langCode != "es" && langCode != "en")
                langCode = "es";

            CurrentLanguage = langCode;

            try
            {
                var uri = new Uri($"pack://application:,,,/Resources/Strings.{langCode}.xaml", UriKind.Absolute);
                var newDict = new ResourceDictionary { Source = uri };

                var appResources = Application.Current?.Resources;
                if (appResources != null)
                {
                    // Buscar si ya hay un diccionario de strings
                    ResourceDictionary? existing = null;
                    foreach (var dict in appResources.MergedDictionaries)
                    {
                        if (dict.Source != null && dict.Source.OriginalString.Contains("Strings."))
                        {
                            existing = dict;
                            break;
                        }
                    }

                    if (existing != null)
                    {
                        int index = appResources.MergedDictionaries.IndexOf(existing);
                        appResources.MergedDictionaries[index] = newDict;
                    }
                    else
                    {
                        appResources.MergedDictionaries.Add(newDict);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading strings: {ex.Message}");
            }

            if (saveConfig)
            {
                try
                {
                    var dict = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "Language", CurrentLanguage }
                    };
                    File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(dict, Formatting.Indented));
                }
                catch { }
            }

            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public string GetString(string key, string fallback = "")
        {
            try
            {
                if (Application.Current?.Resources[key] is string str)
                    return str;
            }
            catch { }
            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }
    }
}
