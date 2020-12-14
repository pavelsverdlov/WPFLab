namespace WPFLab.LightPropertyGrid {
    public interface IViewPropertyValidator {
        /// <summary>
        /// Verify property by internal rules in case some rule failed return error message
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string? Validate(string propertyName);
    }
}
