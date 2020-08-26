using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace WPFLab {
    public abstract class BaseWPFCommand : ICommand {
        public event EventHandler CanExecuteChanged = (s, r) => { };
        public virtual bool CanExecute(object parameter) {
            return true;
        }

        public abstract void Execute(object parameter);
    }
    public abstract class BaseWPFCommand<TParam> : ICommand {
        public event EventHandler CanExecuteChanged = (s, r) => { };
        public virtual bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (parameter is TParam param) {
                Execute(param);
            }
        }
        public abstract void Execute(TParam parameter);
    }

    public class WpfActionCommand<T> : BaseWPFCommand<T> {
        readonly Action<T> action;
        public WpfActionCommand(Action<T> action) {
            this.action = action;
        }

        public override void Execute(T parameter) {
            action?.Invoke(parameter);
        }
    }
    public class WpfActionCommand : BaseWPFCommand {
        readonly Action action;
        public WpfActionCommand(Action action) {
            this.action = action;
        }

        public override void Execute(object parameter) {
            action?.Invoke();
        }
    }
}
