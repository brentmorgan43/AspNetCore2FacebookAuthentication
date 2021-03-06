﻿ASPNET Core 2.0 External Authentication Minimal Requirements

This project is just an example of how to get external authentication working with ASPNET Core 2.0
with a minimal amount of work involved and using the Empty AspNetCore template. I had a lot of trouble 
finding an example of how to get this to work using the Empty web application template in Visual Studio. 
I have elected to use Facebook authentication for this example, but the other authentication providers 
would follow a similar process.

The following steps were done to get this to work if you wanted to repeat them (note all steps
that mention adding a using statement should place those statements at the top of the class
file, or just let Visual Studio handle it):

1. In Visual Studio create a new ASP.NET Core Web Application project

2. Select Empty on the template selection screen - the Change Authentication button should be
greyed out, but double check and make sure it says No Authentication is selected. I also left
docker support unchecked.

3. With the project created, right click on the project and select Manage Nuget Packages - if
you are command line junky, you can do that instead, but I prefer the GUI. Make sure the 
following projects are installed:
	a. Microsoft.AspNetCore.All
	b. Microsoft.AspNetCore.Authentication.Facebook
	c. Microsoft.NETCore.App
	d. Microsoft.VisualStudio.Web.CodeGeneration.Design

4. Open the project properties and select the Debug tab
	a. Set Profile to IIS Express
	b. Set Launch to IIS Express
	c. Check the Enable SSL and copy the generated URL
	d. Paste the copied URL to Launch Browser textbox and the App URL textbox
	e. Make a note of the URL too because you will need it to setup the Facebook Authentication later

5. Open this link, https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins?tabs=aspnetcore2x, 
and follow the instructions for "Create the app in Facebook" - bear in mind that the instructions
in the doc do not "perfectly" align with the interface, at least it didn't for me.

6. Right click on the project in Solution Explorer and select Manage User Secrets. Copy the 
following text into the file, replacing the AppId and AppSecret with the appropriate
values from facebook:
{
  "Authentication": {
    "Facebook": {
      "AppId": "AppIdValue",
      "AppSecret": "AppSecretValue"
    }
  }
}

7. Open the Startup.cs file and make the following coding adjustments:
	a. Add the Configuration property to the class:
        public IConfiguration Configuration { get; }
	b. The configuration property will require a using statement:
		using Microsoft.Extensions.Configuration;
	c. Add a constructor for the Startup class - this will setup the configuration items
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

8. Stub out ApplicationUser class
	a. Add a new folder to the project named Models
	b. Add a new class to this folder named ApplicationUser.cs
	c. Make sure the new class inherits IdentityUser
		public class ApplicationUser : IdentityUser
		{
		}
	d. Add a using statement for AspNetCore Identity
		using Microsoft.AspNetCore.Identity;

9. Open the Startup.cs file and make the following coding adjustments to the ConfigureServices method
	a. The first call should be made to AddIdentity
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddDefaultTokenProviders();
	b. Add a using statment for your Models folder - in my example it is:
		using FacebookAuthentication.Models;
	c. Add a using statement for AspNetCore Identity
		using Microsoft.AspNetCore.Identity;
	d. The next call is to AddAuthentication
        services.AddAuthentication().AddFacebook(facebookOptions =>
        {
            facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
            facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
        });
	e. To make this as a place to build off a larger MVC application we will need to add MVC components.
	To do so, add the following code as the final call of the method:
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(new RequireHttpsAttribute());
        });

        services.AddMvc();
	f. Add a using statement for AspNetCore MVC
		using Microsoft.AspNetCore.Mvc;

10. Open the Startup.cs file and make the following coding adjustment to the Configure method
	a. Remove the app.Run Hello World code - you can either comment it out or delete it - I deleted it
        //app.Run(async (context) =>
        //{
        //    await context.Response.WriteAsync("Hello World!");
        //});
	b. Add code to enforce HTTPS
		var rewriteoptions = new RewriteOptions().AddRedirectToHttps();
        app.UseRewriter(rewriteoptions);
	c. Add a using statement for AspNetCore Rewrite
		using Microsoft.AspNetCore.Rewrite;
	d. Add code to use static files
		app.UseStaticFiles();
	e. Add code to use authentication
		app.UseAuthentication();
	f. Add code to use MVC and set the default route
        app.UseMvc(routes =>
        {
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
        });

11. Set up Views and Controllers so that something will display when the application is run
	a. Add a new folder to the project named Views
	b. Add a new folder named Shared to the Views folder
	c. Add a new View to the Views folder named _ViewStart.cshtml
	d. Replace the code in the _ViewStart.cshtml file with the following:
		@{
			Layout = "ViewLayout";
		}
	e. Add a new View to the Shared folder named ViewLayout.cshtml
	f. Replace the code in the ViewLayout.cshtml file with the following:
		<!DOCTYPE html>
		<html>
		<head>
			<meta name="viewport" content="width=device-width" />
			<title>ViewLayout</title>
		</head>
		<body>
			<div class="container-fluid body-content">
				<div>
					@RenderBody()
				</div>
			</div>
		</body>
		</html>
	g. Add a new folder to the project named Controllers
	h. Add a new controller to the Controllers folder named HomeController - be sure to select
	"MVC Controller - Empty" for the controller template
	i. Open the HomeController, right click on the Index() method, and select "Add View...". 
	Leave the defaults selected. When Visual Studio is done with the add, a new folder named Home and Index.cshtml
	file should have been added to the Views folder
	j. Replace the code in the Index.cshtml file with the following:
		@{
			ViewData["Title"] = "Index";
		}
		<h2>Home Controller - Index Page</h2>
		<form method="post" action="/Home/FacebookLogin">
			<button type="submit" class="btn btn-default" name="provider" value="Facebook" title="Log in using your Facebook account">Facebook</button>
		</form>
	k. Add a new method to the HomeController class to manage the Facebook authentication
        public IActionResult FacebookLogin()
        {
            // Request a redirect to the external login provider.
            var redirectUrl = "./Home";
            var properties = new AuthenticationProperties();
            properties.RedirectUri = redirectUrl;
            properties.Items.Add("LoginProvider", "Facebook");
            var cr = new ChallengeResult("Facebook", properties);
            return cr;
        }
	l. Add a using statement for AspNetCore Authentication to the Home controller
		using Microsoft.AspNetCore.Authentication;

12. Build the project

13. Run the project - assuming your facebook credentials are saved in your browser, you may
want to test this using the browser's In Private or In Cognito mode 
	a. Open a browser In Private mode
	b. Paste your application's url in the address bar and load the page
	c. Click the Facebook button which should bring up the Facebook login screen
	d. After successfully logging in to Facebook you should be returned to the Home view as an authenticated user






