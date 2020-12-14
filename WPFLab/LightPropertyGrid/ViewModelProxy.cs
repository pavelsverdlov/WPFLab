using FluentValidation;
using FluentValidation.Results;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WPFLab.LightPropertyGrid {
    public interface IViewModelProxy {
        event Action<string, object?>? PropertyChanged;
        event Action<string, Action<IViewProperty>>? PropertyViewUpdate;
        ValidationResultProxy Validate();
        void OnAnalyzed();
    }

    public abstract class ViewModelProxy<TProxy> : AbstractValidator<TProxy>, IViewModelProxy {
        public event Action<string, object?>? PropertyChanged;
        public event Action<bool>? ValidationStatusChanged;
        public event Action<string, Action<IViewProperty>>? PropertyViewUpdate;

        public ValidationResultProxy Validate() {
            var result = OnValidate();
            ValidationStatusChanged?.Invoke(result.IsValid);
            return new ValidationResultProxy(result);
        }
        public virtual void OnAnalyzed() { }

        protected void UpdateViewProperty<TProperty>(Expression<Func<TProxy, TProperty>> expression, Action<IViewProperty> change) {
            if(change == null) {
                throw new ArgumentNullException(nameof(change));
            }

            var member = expression.Body as MemberExpression;
            if (member?.Member is PropertyInfo property) {
                PropertyViewUpdate?.Invoke(property.Name, change);
            } else {
                throw new NotSupportedException("Support only property in expression.");
            }
        }
        protected bool Update<T>(ref T currentValue, T newValue, [CallerMemberName] string propertyName = "") {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) {
                return false;
            }
            currentValue = newValue;
            PropertyChanged?.Invoke(propertyName, currentValue);
            return true;
        }

        protected abstract ValidationResult OnValidate();
    }
}
