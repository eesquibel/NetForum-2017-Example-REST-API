using System;
using System.Threading.Tasks;
using System.Web.Http;
using Avectra.netForum.Common;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(NetForumRestExample.Startup))]

namespace NetForumRestExample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Config.CurrentUserName = "NetForumRestExample";
            Config.SuperUser = true;
            Config.Context.Application["xweb"] = true;

            ConfigureAuth(app);

            app.UseWebApi(GlobalConfiguration.Configuration);
        }
    }
}
