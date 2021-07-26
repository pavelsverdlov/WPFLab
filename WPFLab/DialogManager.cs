using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;

using WPFLab.MVVM;

namespace WPFLab {
    public interface IDialogViewModel {
        public Window Dialog { get; set; }
    }
    public interface IDialogRegisterService {
        IDialogRegisterService Register<TWin,TVM>() 
            where TWin : Window
            where TVM : BaseNotify;
    }
    public abstract class DialogManager : IDialogRegisterService {
        static Exception NotRegistered(Type type) => new Exception($"{type.Name} is not registered in IDialogRegisterService");


        readonly IDependencyResolverService resolverService;

        readonly Dictionary<Type, Type> winToVM;
        bool isRegistered;

        public DialogManager(IDependencyResolverService resolverService) {
            this.resolverService = resolverService;
            winToVM = new Dictionary<Type, Type>();
        }

        public IDialogRegisterService Register<TWin, TVM>()
            where TWin : Window
            where TVM : BaseNotify {
            winToVM.Add(typeof(TWin), typeof(TVM));
            return this;
        }
        public TVM Show<TWin, TVM, TOwner>() 
            where TWin : Window 
            where TOwner : Window {
            if (!isRegistered) {
                Register();
            }
            var win = resolverService.ResolveView<TWin>(GetViewModelType<TWin>());
            var dispatcher = resolverService.Resolve<Dispatcher>();
            win.Owner = resolverService.Resolve<TOwner>();
            dispatcher.InvokeAsync(() => {
                win.Show();
            });            
            return (TVM) win.DataContext;
        }
        public TVM Show<TWin, TVM>()
          where TWin : Window{
            if (!isRegistered) {
                Register();
            }
            var vm = GetViewModelType<TWin>();
            var win = resolverService.ResolveView<TWin>(vm);
            if(win.DataContext is IDialogViewModel dvm) {
                dvm.Dialog = win;
            }
            var dispatcher = resolverService.Resolve<Dispatcher>();
            dispatcher.InvokeAsync(() => {
                win.Show();
            });
            return (TVM)win.DataContext;
        }
        public void Show<TWin>() where TWin : Window {
            if (!isRegistered) {
                Register();
            }
            resolverService.ResolveView<TWin>(GetViewModelType<TWin>()).Show();
        }
        public void ShowDialog<TWin>() where TWin : Window {
            if (!isRegistered) {
                Register();
            }
            resolverService.ResolveView<TWin>(GetViewModelType<TWin>()).ShowDialog();
        }
        public TWin GetDialog<TWin>() where TWin : Window {
            if (!isRegistered) {
                Register();
            }
            return (TWin)resolverService.ResolveView<TWin>(GetViewModelType<TWin>());
        }
        


        protected abstract void OnRegister(IDialogRegisterService registrator);

        void Register() {
            OnRegister(this);
            isRegistered = true;
        }
        Type GetViewModelType<TWin>() {
            var type = typeof(TWin);
            if (!winToVM.TryGetValue(type, out var vm)) {
                throw NotRegistered(type);
            }
            return vm;
        }
    }
}
