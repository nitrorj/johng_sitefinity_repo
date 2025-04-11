using System;
using System.Web;
using System.Web.Mvc;
using Telerik.Sitefinity.Mvc;
using Telerik.Sitefinity.Personalization;
using MercolaSiteFinity.MVC.ViewModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Localization;
using System.Text.RegularExpressions;
using System.Text;
using Telerik.Sitefinity.Modules.Newsletters;
using Telerik.Sitefinity.Newsletters.Model;
using Telerik.Sitefinity.Web;
using ServiceStack;
using System.Net.Http;

namespace MercolaSiteFinity.MVC.Controllers
{
    [ControllerToolboxItem(Name = "CustomWidget_MVC", Title = "Custom Widget", SectionName = "CustomWidgets")]
    public class CustomWidgetController : Controller, IPersonalizable
    {
        public ActionResult Index()
        {
            string action = Request.Form["sfaction"];
            var siteMap = SiteMapBase.GetCurrentNode();

            if (!string.IsNullOrEmpty(action))
            {
                if (action.Equals("Subscribe", StringComparison.OrdinalIgnoreCase))
                    return Subscribe(
                        Request.Form["email"],
                        Request.Form["source"],
                        Request.Form["sourceLocation"],
                        Request.Form["returnUrl"]
                    );

                if (action.Equals("Unsubscribe", StringComparison.OrdinalIgnoreCase))
                    return Unsubscribe(Request.Form["email"], Request.Form["returnUrl"]);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Subscribe(string email, string source, string sourceLocation, string returnUrl, string nlfrequency = "enHealthDailyNL")
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(source) || string.IsNullOrEmpty(sourceLocation))
            {
                TempData["SubscriptionMessage"] = "All fields are required.";
                return Redirect(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "/");
            }

            string emailDomain = email.Split('@').LastOrDefault()?.ToLower();

            if (emailDomain != null)
            {
                SetSubscriberCookie(email);
                string redirectUrl = "https://articles-test.mercola.com/sites/Subscribe/SubscribeEmail.aspx?" +
                                     "emailaddress=" + HttpUtility.UrlEncode(email) +
                                     "&Source=" + HttpUtility.UrlEncode(source) +
                                     "&SourceLocation=" + HttpUtility.UrlEncode(sourceLocation) +
                                     "&ReturnURL=" + HttpUtility.UrlEncode(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "/") +
                                     "&NLFrequency=" + HttpUtility.UrlEncode(nlfrequency);

                return Redirect(redirectUrl);
            }

            return Redirect(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "/");
        }

        private void SetSubscriberCookie(string email)
        {
            HttpCookie subscriberCookie = new HttpCookie("Subscriber")
            {
                ["IsSubscribed"] = "true",
                ["Email"] = email,
                Expires = DateTime.Now.AddYears(1)
            };

            Response.Cookies.Add(subscriberCookie);
        }

        [HttpPost]
        public ActionResult Unsubscribe(string email, string returnUrl)
        {
            if (string.IsNullOrEmpty(email) && Request.Cookies["Subscriber"] != null)
            {
                email = Request.Cookies["Subscriber"]["Email"];
            }

            // Expire the single cookie
            if (Request.Cookies["Subscriber"] != null)
            {
                HttpCookie expiredCookie = new HttpCookie("Subscriber")
                {
                    Expires = DateTime.Now.AddDays(-1) // Expire the cookie
                };
                Response.Cookies.Add(expiredCookie);
            }

            return Redirect(!string.IsNullOrEmpty(returnUrl) ? returnUrl : Request.UrlReferrer?.ToString() ?? "/");
        }
    }
}
