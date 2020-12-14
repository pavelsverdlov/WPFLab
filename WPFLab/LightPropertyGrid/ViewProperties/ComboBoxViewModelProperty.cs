using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;

namespace WPFLab.LightPropertyGrid.ViewProperties {
    class ComboBoxItem {
        public string Value { get; }
        public string Display { get; }
        public ComboBoxItem(string value, string disp) {
            Value = value;
            Display = disp;
        }
    }
    class ComboBoxViewModelProperty : ViewModelProperty {
        public ICollectionView Values { get; }

        string current;
        public ComboBoxViewModelProperty(string prName, Action<string?> change, string current,
            string[] values, IViewPropertyValidator validator)
            : base(prName, change, validator) {
            this.current = current;
            var obs = new ObservableCollection<ComboBoxItem>();
            var selectedIndex = 0;
            foreach (var v in values) {
                var cbi = new ComboBoxItem(v, v);
                if (v == current) {
                    selectedIndex = obs.Count;
                }
                obs.Add(cbi);
            }
            Values = CollectionViewSource.GetDefaultView(obs);
            Values.MoveCurrentToPosition(selectedIndex);
            Values.CurrentChanged += OnValuesCurrentChanged;
        }
        public ComboBoxViewModelProperty(string prName, Action<string?> change, string current,
            IEnumerable<FieldInfo> values, IViewPropertyValidator validator)
            : base(prName, change, validator) {
            this.current = current;
            var obs = new ObservableCollection<ComboBoxItem>();
            var selectedIndex = 0;
            foreach (var item in values) {
                var name = item.Name;
                var display = item.GetCustomAttribute<DisplayAttribute>();
                var disp = display?.GetName() ?? name;
                var cbi = new ComboBoxItem(name, disp);
                if (name == current) {
                    selectedIndex = obs.Count;
                }
                obs.Add(cbi);
            }
            Values = CollectionViewSource.GetDefaultView(obs);
            Values.MoveCurrentToPosition(selectedIndex);
            Values.CurrentChanged += OnValuesCurrentChanged;
        }

        void OnValuesCurrentChanged(object? sender, EventArgs e) {
            if (Values.CurrentItem is null) {
                return;
            }
            var value = string.Empty;
            if (Values.CurrentItem is ComboBoxItem item && TryChangeValue(ref value, item.Value)) {
                UpdateValue(value);
            } else {
                var val = Values.CurrentItem.ToString();
                if (val == current) {
                    return;
                }
                UpdateValue(val);
                current = val;
            }
        }

        public override void ChangeValue(object? newvalue) {
            //TODO: 
            throw new NotImplementedException();
        }
    }
}
