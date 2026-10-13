using System;
using System.ComponentModel;
using System.Windows;
using PostitApp.ViewModels;

namespace PostitApp.Views
{
    /// <summary>
    /// Lógica de código detrás de la ventana de gestión central
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public ManagerWindow()
        {
            InitializeComponent();
            _viewModel = (MainViewModel)DataContext;

            Loaded  += ManagerWindow_Loaded;
            Closing += ManagerWindow_Closing;

            App.NoteService.NotasActualizadas += OnNotasActualizadas;
        }

        private void ManagerWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            _viewModel.ActualizarColeccion();
        }

        private void ManagerWindow_Closing(object? sender, CancelEventArgs e)
        {
            App.NoteService.GuardarNotas();
            Application.Current.Shutdown();
        }

        private void OnNotasActualizadas(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _viewModel.ActualizarColeccion();
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            App.NoteService.NotasActualizadas -= OnNotasActualizadas;
            base.OnClosed(e);
        }
    }
}