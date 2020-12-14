using System;

namespace WPFLab.LightPropertyGrid {
    public enum VisibleModes {
        Visible,
        Hidden,
        Collapsed,
    }
    public class VisibleAttribute : Attribute {
        public bool IsVisible => Mode != VisibleModes.Collapsed;
        public VisibleModes Mode { get; }
        public VisibleAttribute(VisibleModes visibility = VisibleModes.Visible) {
            Mode = visibility;
        }
    }
}
