using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;

namespace PlayingWithDependencyInjectionInWebForm.AutofacImplementation
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Dependency>().As<IDependency>().InstancePerRequest();
            builder.RegisterSource(new WebFormRegistrationSource());
            var container = builder.Build();
            var provider = new AutofacServiceProvider(container);
            HttpRuntime.WebObjectActivator = provider;
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
            ILifetimeScope lifetimeScope;
            if (HttpContext.Current != null)
            {

                lifetimeScope = (ILifetimeScope)HttpContext.Current.Items[typeof(ILifetimeScope)];
                if (lifetimeScope == null)
                {
                    void CleanScope(object sender, EventArgs args)
                    {
                        if (sender is HttpApplication application)
                        {
                            application.RequestCompleted -= CleanScope;
                            lifetimeScope.Dispose();
                        }
                    }

                    lifetimeScope = _container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
                    HttpContext.Current.Items.Add(typeof(ILifetimeScope), lifetimeScope);
                    HttpContext.Current.ApplicationInstance.RequestCompleted += CleanScope;
                }
            }
            else
            {
                lifetimeScope = _container;
            }

            if (lifetimeScope.IsRegistered(serviceType))
            {
                Debug.WriteLine($"Resolving type {serviceType.FullName}");
                return lifetimeScope.Resolve(serviceType);
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
        int Id { get; }
        string GetFormattedTime();
    }

    [DebuggerDisplay("Dependency #{" + nameof(Id) + "}")]
    public class Dependency : IDependency
    {
        private static int _id;

        public int Id { get; }

        public Dependency()
        {
            Id = Interlocked.Increment(ref _id);
        }

        public string GetFormattedTime() => DateTimeOffset.UtcNow.ToString("f", CultureInfo.InvariantCulture);
    }
}