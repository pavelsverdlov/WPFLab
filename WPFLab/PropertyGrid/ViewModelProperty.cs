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
using System.Windows;

namespace WPFLab.PropertyGrid {
    public enum VisibleModes {
        Visible,
        Hidden,
        Collapsed,
    }
    public class VisibleAttribute : Attribute {
        public bool IsVisible => Mode != VisibleModes.Collapsed;
        public VisibleModes Mode { get; }
        public VisibleAttribute(VisibleModes visibility = VisibleModes.Visible) {
            Mode = visibility;
        }
    }
    public interface IViewModelProxy {
        event Action<string, Action<IViewProperty>>? PropertyViewUpdate;
        ValidationResultProxy Validate();
        void OnAnalyzed();
    }

    public interface IViewPropertyValidator {
        public string? Validate(string propertyName);
    }

    public class ValidationResultProxy {
        readonly FluentValidation.Results.ValidationResult result;
        internal ValidationResultProxy(FluentValidation.Results.ValidationResult result) {
            this.result = result;
        }

        public bool IsValid => result.IsValid;

        public string? GetFirstErrorMessageBy(string propertyName) {
            return result.Errors.FirstOrDefault(x => x.PropertyName == propertyName)?.ErrorMessage;
        }
    }

    public interface IViewProperty {
        string PropertyName { get; }
        string Title { get; }
        bool IsReadOnly { get; set; }
        VisibleModes Visible { get; set; }
        void Validate();
    }

    public abstract class ViewModelProperty : INotifyPropertyChanged, IViewProperty {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string PropertyName { get; }
        public string Title { get; set; }
        public VisibleModes Visible {
            get {
                return visibility;
            }
            set {
                visibility = value;
                OnPropertyChanged(nameof(Visible));
            }
        }
        public bool IsReadOnly {
            get => isReadOnly;
            set {
                isReadOnly = value;
                OnPropertyChanged(nameof(IsReadOnly));
            }
        }
        public bool IsChanged { get; set; }
        public string? ErrorMessage { get; protected set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        readonly Action<string> change;
        readonly IViewPropertyValidator validator;
        VisibleModes visibility;
        bool isReadOnly;

        protected ViewModelProperty(string propertyName, Action<string> change, IViewPropertyValidator validator) {
            PropertyName = propertyName;
            this.validator = validator;
            Visible = VisibleModes.Visible;
            this.change = change;
        }
        protected ViewModelProperty(string propertyName, IViewPropertyValidator validator)
            : this(propertyName, x => { }, validator) {
        }

        public virtual void UpdateValue(string val) {
            IsChanged = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChanged)));

            Validate();
        }
        public bool TryChangeValue(ref string field, string? value) {
            try {
                //try to convert string to target type
                change(value);
                return true;
            } catch {
                //if can't use prev valid value
                value = field;
                change(value);
            } finally {
                field = value;
            }

            return false;
        }
        public virtual void Validate() {
            ErrorMessage = validator.Validate(PropertyName);
            OnPropertyChanged(nameof(HasError));
        }

        protected void OnPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

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
            //change(val);
            base.UpdateValue(val);
        }
    }

    class TextBoxViewModelProperty : ViewModelStringProperty {
        public TextBoxViewModelProperty(string prName, string val, Action<string> change, IViewPropertyValidator validator)
            : base(prName, val, change, validator) { }
    }

    class ComboBoxItem {
        public string Value { get; }
        public string Display { get; }
        public ComboBoxItem(string value, string disp) {
            Value = value;
            Display = disp;
        }
    }
    class CheckBoxViewModelProperty : ViewModelProperty {
        private bool? isChecked;

        public bool? IsChecked {
            get => isChecked;
            set {
                isChecked = value;

                string? strvalue;
                if (!value.HasValue) {
                    strvalue = null;
                } else {
                    strvalue = value.Value ? bool.TrueString : bool.FalseString;
                }

                var _value = string.Empty;
                if (TryChangeValue(ref _value, strvalue)) {
                    UpdateValue(_value);
                }
            }
        }
        public CheckBoxViewModelProperty(string prName, bool? val, Action<string> change, IViewPropertyValidator validator)
            : base(prName, change, validator) {
            isChecked = val;
        }
    }
    class ComboBoxViewModelProperty : ViewModelProperty {
        public ICollectionView Values { get; }

        string current;
        public ComboBoxViewModelProperty(string prName, Action<string> change, string current,
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
        public ComboBoxViewModelProperty(string prName, Action<string> change, string current,
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
    }

    public class GroupViewModelProperties : ViewModelProperty, IViewPropertyValidator {
        public ObservableCollection<ViewModelProperty> Properties { get; }

        readonly CultureInfo currentUICulture;
        readonly object value;
        readonly IViewModelProxy proxy;

        internal GroupViewModelProperties(string title, IViewModelProxy value, IViewPropertyValidator prValidator)
            : base(title, prValidator) {
            Title = title;
            this.value = value ?? throw new ArgumentNullException(nameof(value));
            Properties = new ObservableCollection<ViewModelProperty>();
            currentUICulture = Thread.CurrentThread.CurrentUICulture;
            proxy = value;
            proxy.PropertyViewUpdate += OnPropertyViewUpdate;
        }

        public GroupViewModelProperties(string title, IViewModelProxy _class) : this(title, _class, null) {

        }

        public void Analyze() {
            foreach (var pr in value.GetType().GetProperties()) {
                if (pr.CanRead) {
                    Analyze(value, pr);
                }
            }
            proxy.OnAnalyzed();
        }

        public override void Validate() {
            foreach (var pr in Properties) {
                pr.Validate();
            }
        }
        public string? Validate(string propertyName) {
            var res = proxy.Validate();

            if (!res.IsValid) {
                var er = res.GetFirstErrorMessageBy(propertyName);
                return er;
            }
            return null;
        }


        void Analyze(object com, PropertyInfo pr) {
            var propertyName = pr.Name;

            var visible = pr.GetCustomAttribute<VisibleAttribute>();
            if (visible == null) { //ignore not visible properties
                return;
            }
            var visibility = visible.Mode;

            var display = pr.GetCustomAttribute<DisplayAttribute>();
            var title = display?.GetName() ?? propertyName;

            var readOnly = pr.GetCustomAttribute<ReadOnlyAttribute>();
            var isReadOnly = readOnly?.IsReadOnly ?? !pr.CanWrite;

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
                        AddSequence(propertyName, title,
                            val?.ToString() ?? string.Empty,
                            change, new[] { bool.TrueString, bool.FalseString });
                        break;
                    default:
                        AddPrimitive(propertyName, title, toString(val), change, isReadOnly);
                        break;
                }
            } else if (type.IsValueType && type.IsGenericType
                  && Type.GetTypeCode(type.GenericTypeArguments[0]) == TypeCode.Boolean) {
                Action<string> update = _value => {
                    if (_value == null) {
                        pr.SetValue(com, null);
                    } else {
                        pr.SetValue(com, (bool?)bool.Parse(_value));
                    }
                };
                Properties.Add(new CheckBoxViewModelProperty(propertyName, (bool?)val, update, this) {
                    Title = title,
                });
            } else if (type.IsEnum) {
                var fields = type.GetFields().ToList();
                fields.RemoveAt(0);//remove first because it default field of enum type

                void change(string x) => pr.SetValue(com, Enum.Parse(type, x));
                var values = fields.Select(x => x.GetValue(com)?.ToString()).ToArray();

                AddSequence(propertyName, title,
                    val?.ToString() ?? string.Empty,
                    change, fields);
            } else if (type.IsClass && val is IViewModelProxy proxy) {
                //inner classes must implement IViewModelProxy
                AddGroup(propertyName, title, proxy);
            }
        }
        void AddPrimitive(string propertyName, string title, string val, Action<string> change, bool isReadOnly) {
            var pr = new TextBoxViewModelProperty(propertyName, val, change, this) {
                Title = title,
                IsReadOnly = isReadOnly,
            };
            Properties.Add(pr);
        }
        void AddSequence(string propertyName, string title, string currentVal, Action<string> change, string[] values) {
            var pr = new ComboBoxViewModelProperty(propertyName, change, currentVal, values, this) {
                Title = title,
            };
            Properties.Add(pr);
        }
        void AddSequence(string propertyName, string title, string currentVal, Action<string> change, IEnumerable<FieldInfo> values) {
            var pr = new ComboBoxViewModelProperty(propertyName, change, currentVal, values, this) {
                Title = title,
            };
            Properties.Add(pr);
        }
        void AddGroup(string propertyName, string title, IViewModelProxy val) {
            var pr = new GroupViewModelProperties(propertyName, val, this) {
                Title = title
            };
            pr.Analyze();
            Properties.Add(pr);
        }

        static Func<string, object?> GetConverter(Type t, CultureInfo culture) {
            var sin = NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
            switch (Type.GetTypeCode(t)) {
                case TypeCode.Boolean:
                    return _in => bool.Parse(_in);
                case TypeCode.String:
                    return _in => _in;
                case TypeCode.Single:
                    return _in => float.Parse(_in, sin | NumberStyles.Float, culture);
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
        static Func<object?, string> GetToStringConverter(Type t, CultureInfo culture) {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            switch (Type.GetTypeCode(t)) {
                case TypeCode.Boolean:
                    return _in => _in.ToString();
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
                    return x => string.Empty;
            }
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }

        void OnPropertyViewUpdate(string propertyName, Action<IViewProperty> update) {
            foreach (var pr in Properties) {
                if (pr.PropertyName == propertyName) {
                    update(pr);
                }
            }
        }
    }

    public class GroupViewModelProperties<TClass> : GroupViewModelProperties
        where TClass : class, IViewModelProxy {
        public TClass Value { get; }
        public GroupViewModelProperties(TClass _class, string title) : base(title, _class) {
            if (_class is null) {
                throw new ArgumentNullException(nameof(_class));
            }
            Value = _class;
        }
    }
}
