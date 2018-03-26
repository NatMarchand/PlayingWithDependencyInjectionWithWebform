using System;
using System.Diagnostics;
using System.Web.UI;

namespace PlayingWithDependencyInjectionInWebForm.AutofacImplementation
{
    public partial class Main : MasterPage
    {
        protected IDependency Dependency { get; }

        public Main(IDependency dependency)
        {
            Dependency = dependency;
            Debug.WriteLine($"Dependency {dependency.Id}");
        }
    }
}