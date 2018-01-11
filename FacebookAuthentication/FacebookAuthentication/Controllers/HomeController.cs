using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace FacebookAuthentication.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

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

    }
}