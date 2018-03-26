using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace PlayingWithDependencyInjectionInWebForm.MicrosoftDependencyInjection
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var collection = new ServiceCollection();
            collection.AddScoped<IDependency, Dependency>(sp => new Dependency("foo"));
            var provider = new MicrosoftDependencyInjectionServiceProvider(collection.BuildServiceProvider());
            HttpRuntime.WebObjectActivator = provider;
        }
    }

    public class MicrosoftDependencyInjectionServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public MicrosoftDependencyInjectionServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                IServiceScope lifetimeScope;
                if (HttpContext.Current != null)
                {
                    lifetimeScope = (IServiceScope)HttpContext.Current.Items[typeof(IServiceScope)];
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

                        lifetimeScope = _serviceProvider.CreateScope();
                        HttpContext.Current.Items.Add(typeof(IServiceScope), lifetimeScope);
                        HttpContext.Current.ApplicationInstance.RequestCompleted += CleanScope;
                    }
                }
                else
                {
                    lifetimeScope = _serviceProvider.CreateScope();
                }

                return ActivatorUtilities.GetServiceOrCreateInstance(lifetimeScope.ServiceProvider, serviceType);
            }
            catch (InvalidOperationException)
            {
                //No public ctor available, revert to a private/internal one
                return Activator.CreateInstance(serviceType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, null);
            }
        }
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

        public Dependency(string parameter)
        {
            Id = Interlocked.Increment(ref _id);
        }

        public string GetFormattedTime() => DateTimeOffset.UtcNow.ToString("f", CultureInfo.InvariantCulture);
    }
}