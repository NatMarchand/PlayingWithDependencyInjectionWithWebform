using System.Web.UI;

namespace PlayingWithDependencyInjectionInWebForm.MicrosoftDependencyInjection
{
    public partial class Main : MasterPage
    {
        protected IDependency Dependency { get; }

        public Main(IDependency dependency)
        {
            Dependency = dependency;
        }
    }
}