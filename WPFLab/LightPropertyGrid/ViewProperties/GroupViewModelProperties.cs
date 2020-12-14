using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace WPFLab.LightPropertyGrid.ViewProperties {
    public class GroupViewModelProperties : ViewModelProperty, IViewPropertyValidator {
        class EmptyValidator : IViewPropertyValidator {
            public string? Validate(string propertyName) => null;
        }

        public ObservableCollection<ViewModelProperty> Properties { get; }

        readonly CultureInfo currentUICulture;
        object value;
        readonly IViewModelProxy proxy;

        internal GroupViewModelProperties([NotNull]string title, [NotNull] IViewModelProxy value, [NotNull] IViewPropertyValidator prValidator)
            : base(title, prValidator) {
            Title = title;
            this.value = value ?? throw new ArgumentNullException(nameof(value));
            Properties = new ObservableCollection<ViewModelProperty>();
            currentUICulture = Thread.CurrentThread.CurrentUICulture;
            proxy = value;
            proxy.PropertyViewUpdate += OnPropertyViewUpdate;
            proxy.PropertyChanged += OnPropertyChanged;
        }

        public GroupViewModelProperties([NotNull] string title, [NotNull] IViewModelProxy _class) :
            this(title, _class, new EmptyValidator()) {

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
        public override void ChangeValue(object? newvalue) {
            value = newvalue ?? throw new ArgumentNullException(nameof(newvalue));
            //OnPropertyChanged(nameof(IsChecked));
        }

        void Analyze(object com, PropertyInfo pr) {
            var propertyName = pr.Name;

            var visible = pr.GetCustomAttribute<VisibleAttribute>();
            if (visible == null) { //visible by default
                visible = new VisibleAttribute();
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

                Action<string?> change = x => {
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
                Action<string?> update = _value => {
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

                void change(string? x) => pr.SetValue(com, Enum.Parse(type, x));
                var values = fields.Select(x => x.GetValue(com)?.ToString()).ToArray();

                AddSequence(propertyName, title,
                    val?.ToString() ?? string.Empty,
                    change, fields);
            } else if (type.IsClass && val is IViewModelProxy proxy) {
                //inner classes must implement IViewModelProxy
                AddGroup(propertyName, title, proxy);
            }
        }
        void AddPrimitive(string propertyName, string title, string val, Action<string?> change, bool isReadOnly) {
            var pr = new TextBoxViewModelProperty(propertyName, val, change, this) {
                Title = title,
                IsReadOnly = isReadOnly,
            };
            Properties.Add(pr);
        }
        void AddSequence(string propertyName, string title, string currentVal, Action<string?> change, string[] values) {
            var pr = new ComboBoxViewModelProperty(propertyName, change, currentVal, values, this) {
                Title = title,
            };
            Properties.Add(pr);
        }
        void AddSequence(string propertyName, string title, string currentVal, Action<string?> change, IEnumerable<FieldInfo> values) {
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
        void OnPropertyChanged(string propertyName, object? value) {
            foreach (var pr in Properties) {
                if (pr.PropertyName == propertyName) {
                    pr.ChangeValue(value?.ToString());
                }
            }
        }

        
    }
}
