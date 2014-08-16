using Castle.Windsor;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
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
            this.container = new WindsorContainer()
                .Install(
                        new CommandStackInstaller(),
                        new QueryStackInstaller(),
                        new EventStoreInstaller(),
                        new ControllersInstaller()
                        ); 
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

            GlobalConfiguration.Configuration.Services.Replace(
                typeof(IHttpControllerActivator),
                new WindsorCompositionRoot(this.container));
        }
    }
}