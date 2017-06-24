using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FinancialApplication.Startup))]
namespace FinancialApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
