using FluentValidation;
using FluentValidation.Results;

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace WPFLab.PropertyGrid {
    public abstract class ViewModelProxy<TProxy> : AbstractValidator<TProxy>, IViewModelProxy {
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

        protected abstract ValidationResult OnValidate();
    }
}
