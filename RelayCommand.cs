using System.Windows.Input;

namespace ChristmasClockController
{
    public class RelayCommand : ICommand {
        private readonly System.Action<object?> _execute;
        private readonly System.Func<bool> _canExecute;

        public RelayCommand(System.Action execute, System.Func<bool> canExecute) {
            _execute = _=>execute();
            _canExecute = canExecute;
        }
        public RelayCommand(System.Action<object?> execute, System.Func<bool> canExecute) {
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(System.Action execute) : this(execute, () => true) { }
        public RelayCommand(System.Action<object?> execute) : this(execute, () => true) { }

        public bool CanExecute(object? parameter) => _canExecute();

        public void Execute(object? parameter) => _execute(parameter);

        public event System.EventHandler? CanExecuteChanged;
    }
}