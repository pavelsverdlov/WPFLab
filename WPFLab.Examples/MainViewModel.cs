using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

using WPFLab.Examples.PropertyGridExample;
using WPFLab.MVVM;
using WPFLab.PropertyGrid;

namespace WPFLab.Examples {
    class MainViewModel : BaseNotify {
        readonly MapperService service;

        public GroupViewModelProperties<PropertyGridTestDataProxy> PropertyGrid { get; }
        public ICommand ValidateCommand;
        public string SourceCode { get; set; }

        public MainViewModel(MapperService service) {
            this.service = service;
            PropertyGrid = new GroupViewModelProperties<PropertyGridTestDataProxy>(new PropertyGridTestDataProxy(), "Test Property Grid");

            SourceCode = @"";
        }

        public override void OnLoaded() {
            base.OnLoaded();
            PropertyGrid.Analyze();
            PropertyGrid.Validate();
        }


    }
}
