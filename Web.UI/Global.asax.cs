using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Castle.Windsor;
using Microsoft.AspNet.SignalR;
using Web.UI.Injection.Installers;
using Web.UI.Injection.WebAPI;

namespace Web.UI
{
    // For Castle.Windsor WebApi Injection resolver
    // thanks to: http://blog.ploeh.dk/2012/10/03/DependencyInjectioninASP.NETWebAPIwithCastleWindsor/

    public class WebApiApplication : System.Web.HttpApplication
    {
        private readonly IWindsorContainer container;

        public WebApiApplication()
        {
            this.container = new WindsorContainer();                
        }

        public override void Dispose()
        {
            this.container.Dispose();
            base.Dispose();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // Configure all AutoMapper Profiles
            AutoMapperConfig.Configure();
            // Install DI mapper
            this.container.Install(
                        new MappersInstaller(),
                        new CommandStackInstaller(),
                        new QueryStackInstaller(),
                        new EventStoreInstaller(),
                        new ControllersInstaller(),
                        new LegacyMigrationInstaller()
                        );

            GlobalConfiguration.Configuration.Services.Replace(
                typeof(IHttpControllerActivator),
                new WindsorCompositionRoot(this.container));

            // Use of static reference to Di container to pass to SignalR.
            // I don't like this approach because it forces to expose the DI container as public throughout all 
            // the application, and it's not what I want to do
            //HostingEnvironment.Name = "AspNET";
            //HostingEnvironment.DIContainer = container;
        }

    }

    public class WindsorDependencyResolver : DefaultDependencyResolver
    {
        private readonly IWindsorContainer _container;

        public WindsorDependencyResolver(IWindsorContainer container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            _container = container;
        }

        public override object GetService(Type serviceType)
        {
            return _container.Kernel.HasComponent(serviceType) ? _container.Resolve(serviceType) : base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.Kernel.HasComponent(serviceType) ? _container.ResolveAll(serviceType).Cast<object>() : base.GetServices(serviceType);
        }
    }

    internal static class HostingEnvironment
    {
        public static string Name { get; set; }
        public static IWindsorContainer DIContainer { get; set; }
    }
}