using Hangfire;
using Microsoft.Owin;
using Owin;
using Todo.Infrastructure.Events.Polling;
using Web.UI.Injection.Hangfire;

[assembly: OwinStartup(typeof(Web.UI.Startup))]

namespace Web.UI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.MapSignalR();

            // Use of static reference to Di container to pass to SignalR.
            // I don't like this approach because it forces to expose the DI container as public throughout all 
            // the application, and it's not what I want to do
            //app.MapSignalR(new HubConfiguration
            //{
            //    //see: http://www.asp.net/signalr/overview/advanced/dependency-injection
            //    Resolver = new WindsorDependencyResolver(HostingEnvironment.DIContainer)
            //});

            //Hangfire configuration
            GlobalConfiguration.Configuration.UseSqlServerStorage("ToDoContext");
            app.UseHangfireDashboard();
            app.UseHangfireServer();

            //Fire & forget job
            JobActivator.Current = new WindsorJobActivator(HostingEnvironment.DIContainer.Kernel);
            BackgroundJob.Enqueue(() => HostingEnvironment.DIContainer.Resolve<CommitObserverStarter>().Start());
        }
    }

    

}
