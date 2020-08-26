using System;
using System.Collections.Generic;
using System.Text;
using WPFLab.Examples.PropertyGridExample;
using WPFLab.MVVM;
using WPFLab.PropertyGrid;

namespace WPFLab.Examples {
    class MainViewModel : BaseNotify {
        readonly MapperService service;

        public GroupViewModelProperties<PropertyGridTestDataProxy> Profile { get; }
        public string SourceCode { get; set; }

        public MainViewModel(MapperService service) {
            this.service = service;
            Profile = new GroupViewModelProperties<PropertyGridTestDataProxy>(new PropertyGridTestDataProxy(), "Test Property Grid");

            SourceCode = @"";
        }

        public override void OnLoaded() {
            base.OnLoaded();
            Profile.Analyze();
            Profile.Validate();
        }


    }
}
