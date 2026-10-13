using System;
using System.IO;
using System.Windows;
using PostitApp.Services;
using PostitApp.Views;

namespace PostitApp
{
    public partial class App : Application
    {
        public static NoteService NoteService { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Capturar cualquier error no controlado
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                File.WriteAllText("C:\\topapp-posit\\error.txt",
                    ex.ExceptionObject.ToString());
            };

            DispatcherUnhandledException += (s, ex) =>
            {
                File.WriteAllText("C:\\topapp-posit\\error.txt",
                    ex.Exception.ToString());
                ex.Handled = true;
            };

            try
            {
                base.OnStartup(e);

                LocalizationService.Instance.Initialize();

                NoteService = new NoteService();
                NoteService.CargarNotas();

                var manager = new ManagerWindow();
                manager.Show();

                foreach (var nota in NoteService.Notas)
                {
                    if (nota.EsVisible)
                    {
                        var ventana = new NoteWindow(nota);
                        ventana.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log"), ex.ToString());
                MessageBox.Show(ex.ToString(), "Error de inicio");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NoteService?.GuardarNotas();
            base.OnExit(e);
        }
    }
}