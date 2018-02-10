using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using Autofac;
using Autofac.Builder;
using Autofac.Core;

namespace PlayingWithDependencyInjectionInWebForm.AutofacImplementation
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Dependency>().As<IDependency>();
            builder.RegisterSource(new WebFormRegistrationSource());
            var container = builder.Build();
            var provider = new AutofacServiceProvider(container);
            typeof(HttpRuntime).GetProperty("WebObjectActivator")?.SetValue(null, provider);
        }
    }

    public class AutofacServiceProvider : IServiceProvider
    {
        private readonly ILifetimeScope _container;

        public AutofacServiceProvider(ILifetimeScope container)
        {
            _container = container;
        }

        public object GetService(Type serviceType)
        {
            if (_container.IsRegistered(serviceType))
            {
                Debug.WriteLine($"Resolving type {serviceType.FullName}");
                return _container.Resolve(serviceType);
            }

            Debug.WriteLine($"Activating type {serviceType.FullName}");
            return Activator.CreateInstance(serviceType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, null);
        }
    }

    public class WebFormRegistrationSource : IRegistrationSource
    {
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (service is IServiceWithType serviceWithType && serviceWithType.ServiceType.Namespace.StartsWith("ASP", true, CultureInfo.InvariantCulture))
            {
                return new[]
                {
                    RegistrationBuilder.ForType(serviceWithType.ServiceType).CreateRegistration()
                };
            }

            return Enumerable.Empty<IComponentRegistration>();
        }

        public bool IsAdapterForIndividualComponents => true;
    }

    public interface IDependency
    {
        string GetFormattedTime();
    }

    public class Dependency : IDependency
    {
        public string GetFormattedTime() => DateTimeOffset.UtcNow.ToString("f", CultureInfo.InvariantCulture);
    }
}