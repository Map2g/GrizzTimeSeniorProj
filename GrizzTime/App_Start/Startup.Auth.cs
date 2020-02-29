using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using System;
using GrizzTime.Models;

    [assembly: OwinStartup("ProdConfig", typeof(GrizzTime.Startup))]

namespace GrizzTime
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864   
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user   
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider   
            // Configure the sign in cookie   
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Home/LandingPage"),
                LogoutPath = new PathString("/Home/Logout"),
                ExpireTimeSpan = TimeSpan.FromMinutes(5.0)
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
        }
    }
}