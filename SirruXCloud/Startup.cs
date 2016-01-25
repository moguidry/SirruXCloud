using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SirruXCloud.Startup))]
namespace SirruXCloud
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            
        }
    }
}
