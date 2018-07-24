using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace CEBApi
{
    public class WebApiApplication : HttpApplication
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["CEBConnectionString"].ConnectionString;    // connection string

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            SqlDependency.Start(cs); // start sql dependency
        }

        protected void Application_End()
        {
            SqlDependency.Stop(cs); // stop sql dependency
        }
    }
}