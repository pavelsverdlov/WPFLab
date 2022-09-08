using System;
using System.ComponentModel;
using System.Windows;
using WPFLab.MVVM;

namespace WPFLab {
    public static class IoCExtensions {
        public static IDependencyRegisterService RegisterView<TView>(this IDependencyRegisterService service) 
            where TView : FrameworkElement {
          
            service.Register<TView>();

            return service;
        }
        public static IDependencyRegisterService RegisterTransientView<TView>(this IDependencyRegisterService service)
           where TView : FrameworkElement {

            service.RegisterTransient<TView>();

            return service;
        }
        public static TView ResolveView<TView, TViewModel>(this IDependencyResolverService container) 
            where TView : FrameworkElement
            where TViewModel : BaseNotify {
            
            var view = container.Resolve<TView>();
            var vm = container.Resolve<TViewModel>();

            view.DataContext = vm;
            view.Loaded += (x,y) => vm?.OnLoaded();
            view.Unloaded += (x,y) => vm?.OnUnloaded();

            return view;
        }
        public static TView ResolveView<TView>(this IDependencyResolverService container, Type tvm)
            where TView : FrameworkElement {

            var view = container.Resolve<TView>();
            var vm = (BaseNotify)container.Resolve(tvm);

            view.DataContext = vm;
            view.Loaded += (x, y) => vm.OnLoaded();
            view.Unloaded += (x, y) => vm.OnUnloaded();

            return view;
        }
    }
}
