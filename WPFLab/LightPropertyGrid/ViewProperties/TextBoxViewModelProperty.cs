using System;

namespace WPFLab.LightPropertyGrid.ViewProperties {
    class TextBoxViewModelProperty : ViewModelStringProperty {
        public TextBoxViewModelProperty(string prName, string val, Action<string> change, IViewPropertyValidator validator)
            : base(prName, val, change, validator) { }
    }
}
