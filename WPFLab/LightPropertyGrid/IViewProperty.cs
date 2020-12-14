namespace WPFLab.LightPropertyGrid {
    public interface IViewProperty {
        string PropertyName { get; }
        string Title { get; }
        bool IsReadOnly { get; set; }
        VisibleModes Visible { get; set; }
        void Validate();
    }
}
