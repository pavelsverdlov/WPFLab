using System;
using System.Collections.Generic;
using System.Text;

namespace WPFLab {
    public static class WpfLabDependencyExtenstions {
        public static IDependencyRegisterService RegisterApplication<T>(this IDependencyRegisterService service, T app) where T : LabApplication {
            service.Register<T>(x => app);
            service.Register<LabApplication>(x => app);
            service.Register(_ => app.Dispatcher);
            return service;
        }
        public static IDependencyRegisterService RegisterUnhandledExceptionHandler(this IDependencyRegisterService service) {
            service.Register<UnhandledExceptionHandler>();
            return service;
        }
        //public static IDependencyRegisterService RegisterAppLoger<IImp>(this IDependencyRegisterService service)
        //    where IImp : class, IAppLogger  {
        //    service.Register<IAppLogger, IImp>();
        //    return service;
        //}
        public static IDependencyRegisterService RegisterMvvm(this IDependencyRegisterService service) {
            service.Register<IDialogManager>(x => new DialogManager());
            return service;
        }




        public static IDependencyResolverService UseUnhandledExceptionHandler(this IDependencyResolverService service) {
            service.Resolve<UnhandledExceptionHandler>().StartHandling();
            return service;
        }
        public static IDependencyResolverService RemoveUnhandledExceptionHandler(this IDependencyResolverService service) {
            service.Resolve<UnhandledExceptionHandler>().StopHandling();
            return service;
        }

    }
}
