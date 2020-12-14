using System;
using System.Windows.Input;

namespace WPFLab.LightPropertyGrid.ViewProperties {
    abstract class ViewModelStringProperty : ViewModelProperty {
        class ChangedCommand : BaseWPFCommand<string> {
            readonly ViewModelStringProperty pr;

            public ChangedCommand(ViewModelStringProperty viewProperty) {
                this.pr = viewProperty;
            }

            public override void Execute(string parameter) {
                pr.UpdateValue(parameter);
            }
        }
        public ICommand Changed { get; }

        public string Value {
            get => _value;
            set {
                if (TryChangeValue(ref _value, value)) {
                    //OnPropertyChanged(nameof(Value));
                    UpdateValue(_value);
                }
            }
        }

        //  readonly Action<string> change;
        string _value;

        public ViewModelStringProperty(string prName, string val, Action<string> change, IViewPropertyValidator validator)
            : base(prName, change, validator) {
            Changed = new ChangedCommand(this);
            // this.change = change;
            _value = val;
        }

        public override void UpdateValue(string val) {
            base.UpdateValue(val);
        }
        public override void ChangeValue(object? newvalue) {
            TryChangeValue(ref _value, newvalue?.ToString());
            UpdateValue(_value);
            OnPropertyChanged(nameof(Value));
        }
    }
}
