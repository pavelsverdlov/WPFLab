using System;
using System.ComponentModel;
using System.Text;
using WPFLab;
using System.Windows;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace WPFLab.LightPropertyGrid.ViewProperties {
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

        readonly Action<string?> change;
        readonly IViewPropertyValidator validator;
        VisibleModes visibility;
        bool isReadOnly;

        protected ViewModelProperty([NotNull]string propertyName, [NotNull] Action<string?> change, [NotNull] IViewPropertyValidator validator) {
            PropertyName = propertyName;
            Title = propertyName;
            this.validator = validator;
            Visible = VisibleModes.Visible;
            this.change = change;
        }
        protected ViewModelProperty([NotNull] string propertyName, [NotNull] IViewPropertyValidator validator)
            : this(propertyName, x => { }, validator) {
        }

        public abstract void ChangeValue(object? newvalue);
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
        
        protected void OnPropertyChanged([NotNull] string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
