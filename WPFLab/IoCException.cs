using System;

namespace WPFLab {
    public class DependencyInjectionException : Exception {
        public static DependencyInjectionException ServiceNotBuilt => new DependencyInjectionException("Service provider is not built.");
        public static DependencyInjectionException ServiceIsBuilt => new DependencyInjectionException("Can't register new instance, service provider is already built.");
        
        public DependencyInjectionException(string? message) : base(message) {
        }
    }
}
