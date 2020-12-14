using System;

using WPFLab.LightPropertyGrid.ViewProperties;

namespace WPFLab.LightPropertyGrid {
    public class GroupViewModelProperties<TClass> : GroupViewModelProperties
        where TClass : class, IViewModelProxy {
        public TClass Value { get; }
        public GroupViewModelProperties(TClass _class, string title) : base(title, _class) {
            if (_class is null) {
                throw new ArgumentNullException(nameof(_class));
            }
            Value = _class;
        }
    }
}
