using System;

namespace WPFLab.LightPropertyGrid.ViewProperties {
    class CheckBoxViewModelProperty : ViewModelProperty {
        private bool? isChecked;

        public bool? IsChecked {
            get => isChecked;
            set {
                isChecked = value;

                var strvalue = ConvertValue(value);

                var _value = string.Empty;
                if (TryChangeValue(ref _value, strvalue)) {
                    UpdateValue(_value);
                }
            }
        }
        public CheckBoxViewModelProperty(string prName, bool? val, Action<string?> change, IViewPropertyValidator validator)
            : base(prName, change, validator) {
            isChecked = val;
        }

        public override void ChangeValue(object? newvalue) {
            var str = newvalue?.ToString();
            if (str == null || !bool.TryParse(str, out var val)) {
                return;
            }
            var value = ConvertValue(isChecked);
            TryChangeValue(ref value, ConvertValue(val));
            UpdateValue(value);
            OnPropertyChanged(nameof(IsChecked));
        }


        static string? ConvertValue(bool? value) {
            string? strvalue;
            if (!value.HasValue) {
                strvalue = null;
            } else {
                strvalue = value.Value ? bool.TrueString : bool.FalseString;
            }
            return strvalue;
        }
    }
}
