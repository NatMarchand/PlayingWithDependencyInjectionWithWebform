using System;
using System.Web.UI;

namespace PlayingWithDependencyInjectionInWebForm.AutofacImplementation
{
    public partial class Index : Page
    {
        protected IDependency Dependency { get; }

        public Index(IDependency dependency)
        {
            Dependency = dependency;
        }
    }
}