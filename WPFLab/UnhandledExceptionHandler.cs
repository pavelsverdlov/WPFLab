using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace WPFLab {
    class UnhandledExceptionHandler {
        readonly IAppLogger logger;
        readonly LabApplication application;
        readonly Dispatcher dispatcher;

        Dictionary<Type, Func<Exception, Exception>> handlers = new Dictionary<Type, Func<Exception, Exception>>();
        Action<string, string>? messageBoxHandler;
        bool isEnabled;

        public UnhandledExceptionHandler(LabApplication application, Dispatcher dispatcher, IAppLogger logger) {
            this.logger = logger;
            this.application = application;
            this.dispatcher = dispatcher;
        }

        public void StartHandling() {
            if (isEnabled) { return; }

            isEnabled = true;

#if DEBUG
            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
#endif

            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            application.DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        public void StopHandling() {
            if (!isEnabled) { return; }

            AppDomain.CurrentDomain.FirstChanceException -= OnFirstChanceException;
            AppDomain.CurrentDomain.UnhandledException -= OnDomainUnhandledException;
            application.DispatcherUnhandledException -= OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

            isEnabled = false;
        }

        public void AddErrorHandler<T>(Func<Exception, Exception> handler) where T : Exception {
            handlers[typeof(T)] = handler;
        }

        public void RemoveErrorHandler<T>() where T : Exception {
            var type = typeof(T);
            if (!handlers.ContainsKey(type)) { return; }

            handlers.Remove(type);
        }

        public void SetMessageBoxHandler(Action<string, string> handler) {
            messageBoxHandler = handler;
        }

        void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e) {
            logger.Debug(GetFormattedMessage(e.Exception, out var message, out var stackTrace));
        }

        void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) {
            e.SetObserved();
            Handle(e.Exception);
        }

        void OnUnhandledExceptionFilter(object? sender, DispatcherUnhandledExceptionFilterEventArgs e) {
            e.RequestCatch = true;
            Handle(e.Exception);
        }

        void OnDispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e) {
            e.Handled = true;
            Handle(e.Exception);
        }

        void OnDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e) {
            Handle(e.ExceptionObject as Exception, e.IsTerminating);
        }

        void OnThreadException(object? sender, ThreadExceptionEventArgs e) {
            Handle(e.Exception);
        }

        public void Handle(Exception? e) {
            Handle(e, false);
        }

        void Handle(Exception e, bool isCritical) {
            if (e == null) { return; }

            var formattedMessage = GetFormattedMessage(e, out var message, out var stackTrace);
            logger.Error($"{message}{Environment.NewLine}{stackTrace}");

            if (handlers.TryGetValue(e.GetType(), out var handler)) {
                handler?.Invoke(e);
                return;
            }

            ShowMessage(isCritical ? "Critical error" : "Error", message);
        }

        static string GetFormattedMessage(Exception ex, out string? message, out string? stackTrace) {
            stackTrace = ex?.StackTrace;
            while (ex?.InnerException != null) {
                ex = ex.InnerException;
            }
            message = ex?.Message;

            return $"{message}{Environment.NewLine}{stackTrace}";
        }

        void ShowMessage(string title, string message) {
            dispatcher.Invoke(new Action(() => {
                if (messageBoxHandler != null) {
                    messageBoxHandler(title, message);
                } else {
                    MessageBox.Show(title, message);
                }
            }), DispatcherPriority.Send);
        }
    }
}
