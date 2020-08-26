using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace WPFLab {
    public static class FluentValidationEx {
        public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder) {
            return ruleBuilder.Matches(@"^[2-9]\d{2}-\d{3}-\d{4}$");
        }
    }
}
