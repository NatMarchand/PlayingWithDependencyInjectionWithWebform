﻿using System.Web.UI;

namespace PlayingWithDependencyInjectionInWebForm.MicrosoftDependencyInjection
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