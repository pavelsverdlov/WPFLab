using FluentValidation;
using FluentValidation.Validators;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows.Controls;

using WPFLab.PropertyGrid;

namespace WPFLab.Examples.PropertyGridExample {
    enum SomeEnum {
        [Display(Name = "Some enum 1")]
        SomeEnum1,
        [Display(Name = "Some enum 2")]
        SomeEnum2,
        [Display(Name = "Show hidden field below")]
        ShowHiddenFieldBelow,
    }
    class PropertyGridInnerClassDataProxy : ViewModelProxy<PropertyGridInnerClassDataProxy> {
        [Visible]
        public string SomeText1 { get; set; }
        [Visible]
        public string SomeText2 { get; set; }
        [Visible]
        public string SomeText3 { get; set; }
        protected override FluentValidation.Results.ValidationResult OnValidate() => Validate(this);

    }

    class PropertyGridTestDataProxy : ViewModelProxy<PropertyGridTestDataProxy> {
        SomeEnum _enum;
        bool simpleBoolean;
        private bool? nullableBool;

        [Visible]
        public string SomeText { get; set; }
        [Visible]
        public string Email { get; set; }
        [Visible]
        [DisplayFormat(DataFormatString = DisplayFormats.MobilePhoneFormatString)]
        public int Phone { get; set; }

        [Visible]
        public DateTime Birthday { get; set; }

        [Visible]
        [Display(Name = "Enum with display names")]
        public SomeEnum EnumWithDisplayNames {
            get => _enum;
            set {
                _enum = value;
                UpdateViewProperty(x => x.SelfDescribedGender,
                        view => view.Visible = _enum == SomeEnum.ShowHiddenFieldBelow ?
                           VisibleModes.Visible
                            : VisibleModes.Hidden
                    );
            }
        }

        [Visible]
        public string SelfDescribedGender { get; set; }

        [Visible]
        [Display(Name = "Collapse field below")]
        public bool CollapcePropertyBelow {
            get => simpleBoolean;
            set {
                simpleBoolean = value;
                UpdateViewProperty(x => x.CollapsedProperty,
                       view => view.Visible = simpleBoolean ?
                             VisibleModes.Collapsed
                           : VisibleModes.Visible
                   );
            }
        }
        [Visible]
        [Display(Name = "Field to collapsed")]
        public string CollapsedProperty { get; set; }
        [Visible]
        [Display(Name = "Nullable bool")]
        public bool? NullableBool {
            get => nullableBool;
            set { 
                nullableBool = value;
                UpdateViewProperty(x => x.ReadOnlyChangeableProperty,
                       view => view.IsReadOnly = nullableBool.HasValue && nullableBool.Value );
            }
        }

        [Visible]
        [Display(Name = "Changeable readonly")]
        [ReadOnly(true)]
        public string ReadOnlyChangeableProperty { get; set; }
        [Visible]
        public PropertyGridInnerClassDataProxy InnerClass { get; set; }

        protected override FluentValidation.Results.ValidationResult OnValidate() => Validate(this);

        public PropertyGridTestDataProxy() {
            RuleFor(x => x.SomeText)
                .NotEmpty();
            RuleFor(x => x.Email)
                .EmailAddress();
            //RuleFor(x => x.Phone)
            //    .PhoneNumber()
            //    .WithMessage("Please enter a valid phone number.");
            InnerClass = new PropertyGridInnerClassDataProxy();
            Email = "bad.com";
            //Phone = "1234";
        }




    }


}
