using System;
using System.Globalization;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace PlayingWithDependencyInjectionInWebForm.MicrosoftDependencyInjection
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var collection = new ServiceCollection();
            collection.AddTransient<IDependency, Dependency>(sp => new Dependency("foo"));
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
                return ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, serviceType);
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
        string GetFormattedTime();
    }

    public class Dependency : IDependency
    {
        public Dependency(string parameter)
        {

        }

        public string GetFormattedTime() => DateTimeOffset.UtcNow.ToString("f", CultureInfo.InvariantCulture);
    }
}