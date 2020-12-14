using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

using WPFLab.Examples.PropertyGridExample;
using WPFLab.MVVM;
using WPFLab.LightPropertyGrid;


using FluentValidation;

namespace WPFLab.Examples {
    class MainViewModel : BaseNotify {
        readonly MapperService service;

        public GroupViewModelProperties<PropertyGridTestDataProxy> PropertyGrid { get; }
        public ICommand ChangeTextCommand { get; }

        public MainViewModel(MapperService service) {
            this.service = service;
            PropertyGrid = new GroupViewModelProperties<PropertyGridTestDataProxy>(new PropertyGridTestDataProxy(), "Test Property Grid");

            //PropertyGrid.Value.RuleFor(x => x.SomeText).EmailAddress().WithMessage(" ...");

            ChangeTextCommand = new WpfActionCommand(OnChangeText);
        }

        private void OnChangeText() {
            PropertyGrid.Value.SomeText = "new text";
        }

        public override void OnLoaded() {
            base.OnLoaded();
            PropertyGrid.Analyze();
            PropertyGrid.Validate();
        }


    }
}
