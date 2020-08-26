using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace WPFLab {
    public abstract class LabApplication : Application {
        readonly IoCService service;
        protected LabApplication() {
            service = new IoCService();
        }

        protected sealed override void OnStartup(StartupEventArgs e) {
            ConfigureServices(service);
            service.Build();
            base.OnStartup(e);
            AppStartup(e, service);
        }
        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
            AppExit(e, service);
        }

        protected abstract void ConfigureServices(IDependencyRegisterService registrator);
        protected abstract void AppStartup(StartupEventArgs e, IDependencyResolverService resolver);
        protected abstract void AppExit(ExitEventArgs e, IDependencyResolverService resolver);
    }
}
