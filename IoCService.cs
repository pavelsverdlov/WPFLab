﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NLog;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace WPFLab {
    public interface IDependencyResolverService {
        T Resolve<T>() where T : class;
    }
    public interface IDependencyRegisterService {
        IDependencyRegisterService Register<T, TIm>() where T : class where TIm : class, T;
        IDependencyRegisterService Register<T>() where T : class;
        IDependencyRegisterService Register<T>(Func<IServiceProvider, T> implementationFactory) where T : class;
        IDependencyRegisterService RegisterAsSingleton<T, TIm>() where T : class where TIm : class, T;
        IDependencyRegisterService RegisterTransient<T>() where T : class;
    }

    class IoCService : IDependencyRegisterService, IDependencyResolverService {
        readonly ServiceCollection service;
        ServiceProvider? container;

        public bool IsBuilt { get; internal set; }

        public IoCService() {
            service = new ServiceCollection();
            //service.AddLogging();
           

            IsBuilt = false;
        }

        public void Build() {
            Register<IDependencyResolverService>(x => this);
            container = service.BuildServiceProvider();

            //var factory = container.GetService<ILoggerFactory>();
            //factory.AddNLog();
            //factory.ConfigureNLog("nlog.config");

            IsBuilt = true;
        }

        public T Resolve<T>() where T : class {
            if (!IsBuilt) throw new Exception("Service provider is not built.");
            return container.GetService<T>();
        }
        public IDependencyRegisterService Register<T>() where T : class {
            if (IsBuilt) throw new Exception("Can't register new instance, service provider is already built.");
            service.AddScoped<T>();
            IsBuilt = false;
            return this;
        }
        public IDependencyRegisterService RegisterTransient<T>() where T : class {
            if (IsBuilt) throw new Exception("Can't register new instance, service provider is already built.");
            service.AddTransient<T>();
            IsBuilt = false;
            return this;
        }
        public IDependencyRegisterService Register<T>(Func<IServiceProvider, T> implementationFactory) where T : class {
            if (IsBuilt) throw new Exception("Can't register new instance, service provider is already built.");
            service.AddScoped<T>(implementationFactory);
            IsBuilt = false;
            return this;
        }
        public IDependencyRegisterService Register<T, TIm>()
            where T : class 
            where TIm : class, T {
            if (IsBuilt) throw new Exception("Can't register new instance, service provider is already built.");
            service.AddScoped<T, TIm>();
            IsBuilt = false;
            return this;
        }
        public IDependencyRegisterService RegisterAsSingleton<T,TIm>() where T : class where TIm : class, T {
            if (IsBuilt) throw new Exception("Can't register new instance, service provider is already built.");
            service.AddSingleton<T, TIm>();
            IsBuilt = false;
            return this;
        }
        public IDependencyRegisterService RegisterAsSingleton<T>() where T : class {
            if (IsBuilt) throw new Exception("Can't register new instance, service provider is already built.");
            service.AddSingleton<T>();
            IsBuilt = false;
            return this;
        }
        public IDependencyRegisterService RegisterAsSingleton<T>(Func<IServiceProvider, T> implementationFactory) where T : class {
            if (IsBuilt) throw new Exception("Can't register new instance, service provider is already built.");
            service.AddSingleton<T>(implementationFactory);
            IsBuilt = false;
            return this;
        }
    }
}
