using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CetCources.Startup))]
namespace CetCources
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
