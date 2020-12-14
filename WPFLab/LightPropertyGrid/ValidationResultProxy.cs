using System.Linq;

namespace WPFLab.LightPropertyGrid {
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
}
