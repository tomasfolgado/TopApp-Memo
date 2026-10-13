using System;
using System.Windows.Input;

namespace PostitApp.Helpers
{
    /// <summary>
    /// Implementación genérica de ICommand para usar en ViewModels
    /// Versión sin parámetro
    /// </summary>
    public class RelayCommand : ICommand
    {
        // ==================== CAMPOS ====================

        private readonly Action _ejecutar;
        private readonly Func<bool>? _puedeEjecutar;

        // ==================== CONSTRUCTOR ====================

        /// <summary>
        /// Constructor del comando sin parámetro
        /// </summary>
        /// <param name="ejecutar">Acción a ejecutar</param>
        /// <param name="puedeEjecutar">Función que determina si el comando puede ejecutarse</param>
        public RelayCommand(Action ejecutar, Func<bool>? puedeEjecutar = null)
        {
            _ejecutar = ejecutar ?? throw new ArgumentNullException(nameof(ejecutar));
            _puedeEjecutar = puedeEjecutar;
        }

        // ==================== ICOMMAND ====================

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return _puedeEjecutar == null || _puedeEjecutar();
        }

        public void Execute(object? parameter)
        {
            _ejecutar();
        }
    }

    /// <summary>
    /// Implementación genérica de ICommand para usar en ViewModels
    /// Versión con parámetro tipado
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        // ==================== CAMPOS ====================

        private readonly Action<T> _ejecutar;
        private readonly Func<T, bool>? _puedeEjecutar;

        // ==================== CONSTRUCTOR ====================

        /// <summary>
        /// Constructor del comando con parámetro tipado
        /// </summary>
        /// <param name="ejecutar">Acción a ejecutar con parámetro</param>
        /// <param name="puedeEjecutar">Función que determina si el comando puede ejecutarse</param>
        public RelayCommand(Action<T> ejecutar, Func<T, bool>? puedeEjecutar = null)
        {
            _ejecutar = ejecutar ?? throw new ArgumentNullException(nameof(ejecutar));
            _puedeEjecutar = puedeEjecutar;
        }

        // ==================== ICOMMAND ====================

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter is T valor)
                return _puedeEjecutar == null || _puedeEjecutar(valor);

            return _puedeEjecutar == null;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T valor)
                _ejecutar(valor);
        }
    }
}