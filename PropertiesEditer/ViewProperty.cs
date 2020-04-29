using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using WPFLab;
using System.Threading;

namespace WPFLab.PropertiesEditer {
    public interface IViewValidator {
        FluentValidation.Results.ValidationResult Validate();
    }

    public interface IViewPropertyValidator {
        public string? Validate(string propertyName);
    }
    public abstract class ViewProperty : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged = (x, y) => { };
        public string Key { get; set; }
        public string Title { get; set; }

        public bool IsChanged { get; set; }

        #region error handling

        public string? ErrorMessage { get; protected set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        #endregion

        readonly IViewPropertyValidator validator;

        protected ViewProperty(IViewPropertyValidator validator) {
            this.validator = validator;
        }

        public virtual void UpdateValue(string val) {
            IsChanged = true;
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsChanged)));

            ErrorMessage = validator.Validate(Key);
            OnPropertyChanged(nameof(HasError));
        }

        protected void OnPropertyChanged(string name) {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public abstract class ViewStringProperty : ViewProperty {
        class ChangedCommand : BaseWPFCommand<string> {
            readonly ViewStringProperty pr;

            public ChangedCommand(ViewStringProperty viewProperty) {
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
                try {
                    //try to convert string to target type
                    change(value);
                } catch {
                    //if can't use prev valid value
                    value = _value;
                }
                _value = value;
                OnPropertyChanged(nameof(Value));
                UpdateValue(_value);
            }
        }

        readonly Action<string> change;
        string _value;

        public ViewStringProperty(string val, Action<string> change, IViewPropertyValidator validator)
            : base(validator) {
            Changed = new ChangedCommand(this);
            this.change = change;
            _value = val;
        }

        public override void UpdateValue(string val) {
            change(val);
            base.UpdateValue(val);
        }
    }

    public class TextBoxViewProperty : ViewStringProperty {
        public TextBoxViewProperty(string val, Action<string> change, IViewPropertyValidator validator)
            : base(val, change, validator) { }
    }

    public class ComboBoxViewProperty : ViewStringProperty {
        public ICollectionView Values { get; }

        string current;
        public ComboBoxViewProperty(Action<string> change, string current, string[] values, IViewPropertyValidator validator)
            : base(current, change, validator) {
            this.current = current;
            var obs = new ObservableCollection<string>(values);
            Values = CollectionViewSource.GetDefaultView(obs);
            Values.MoveCurrentToPosition(obs.IndexOf(current));
            Values.CurrentChanged += OnValuesCurrentChanged;
        }

        void OnValuesCurrentChanged(object? sender, EventArgs e) {
            if (Values.CurrentItem is null) {
                return;
            }
            var val = Values.CurrentItem.ToString();
            if (val == current) {
                return;
            }
            UpdateValue(val);
            current = val;
        }
    }

    public class GroupViewProperty<TClass> : IViewPropertyValidator
        where TClass : IViewValidator {
        public string Title { get; set; }
        public TClass Value { get; }
        public ObservableCollection<ViewProperty> Properties { get; }

        readonly CultureInfo currentUICulture;

        public GroupViewProperty(TClass _class, string title) {
            if (_class is null) {
                throw new ArgumentNullException(nameof(_class));
            }
            Properties = new ObservableCollection<ViewProperty>();
            Value = _class;
            Title = title;
            currentUICulture = Thread.CurrentThread.CurrentUICulture;
        }

        public void Analyze() {
            foreach (var pr in Value.GetType().GetProperties()) {
                var editable = pr.GetCustomAttribute<EditableAttribute>();
                if (editable != null && editable.AllowEdit && pr.CanRead && pr.CanWrite) {
                    Analyze(Value, pr);
                }
            }
        }

        void AddPrimitive(string key, string? val, Action<string> change) {
            var pr = new TextBoxViewProperty(val, change, this) {
                Title = key,
                Key = key,
            };
            Properties.Add(pr);
        }
        void AddSequence<T>(string key, T val, Action<string> change, string[] values) {
            var pr = new ComboBoxViewProperty(change, val?.ToString(), values, this) {
                Title = key,
                Key = key,
            };
            Properties.Add(pr);
        }

        void Analyze(object com, PropertyInfo pr) {
            if (!pr.CanWrite) { return; }

            var name = pr.Name;

            var val = pr.GetValue(com);

            var type = pr.PropertyType;

            if (type.IsPrimitive || Type.GetTypeCode(type) == TypeCode.String) {
                var converter = GetConverter(type, currentUICulture);
                var toString = GetToStringConverter(type, currentUICulture);

                Action<string> change = x => {
                    if (string.IsNullOrWhiteSpace(x)) {
                        pr.SetValue(com, default);
                    } else {
                        pr.SetValue(com, converter(x));
                    }
                };

                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Boolean:
                        AddSequence(name, val, change, new[] { bool.TrueString, bool.FalseString });
                        break;
                    default:
                        AddPrimitive(name, toString(val), change);
                        break;
                }
                return;
            } else if (type.IsEnum) {
                var fields = type.GetFields().ToList();
                fields.RemoveAt(0);
                void change(string x) => pr.SetValue(com, Enum.Parse(type, x));
                var values = fields.Select(x => x.GetValue(com).ToString()).ToArray();
                AddSequence(name, val, change, values);
                return;
            }

            //throw new NotImplementedException();
        }

        protected Func<string, object?> GetConverter(Type t, CultureInfo culture) {
            switch (Type.GetTypeCode(t)) {
                case TypeCode.Boolean:
                    return _in => bool.Parse(_in);
                case TypeCode.String:
                    return _in => _in;
                case TypeCode.Single:
                    return _in => float.Parse(_in, NumberStyles.Float, culture);
                case TypeCode.Double:
                    return _in => double.Parse(_in, NumberStyles.Float, culture);
                case TypeCode.Int32:
                    return _in => int.Parse(_in, NumberStyles.Integer, culture);
                case TypeCode.UInt32:
                    return _in => uint.Parse(_in, NumberStyles.Integer, culture);
                default:
                    return x => null;
            }
        }
        protected Func<object?, string?> GetToStringConverter(Type t, CultureInfo culture) {
            switch (Type.GetTypeCode(t)) {
                case TypeCode.Boolean:
                    return _in => _in?.ToString();
                case TypeCode.String:
                    return _in => _in?.ToString();
                case TypeCode.Single:
                    return _in => ((float)_in).ToString(culture);
                case TypeCode.Double:
                    return _in => ((double)_in).ToString(culture);
                case TypeCode.Int32:
                    return _in => ((int)_in).ToString(culture);
                case TypeCode.UInt32:
                    return _in => ((uint)_in).ToString(culture);
                default:
                    return x => null;
            }

        }

        public string? Validate(string propertyName) {
            var res = Value.Validate();
            if (!res.IsValid) {
                var er = res.Errors.FirstOrDefault(x => x.PropertyName == propertyName)?.ErrorMessage;
                return er;
            }
            return null;
        }

    }
}
