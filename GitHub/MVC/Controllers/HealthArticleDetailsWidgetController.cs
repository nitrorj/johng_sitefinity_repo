using MercolaSiteFinity.MVC.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Sitefinity.ContentLocations;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Mvc;
using Telerik.Sitefinity.Personalization;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Web;
using Telerik.Sitefinity.Web.UI;
using HtmlAgilityPack;

namespace MercolaSiteFinity.MVC.Controllers
{
    [ControllerToolboxItem(Name = "HealthArticleDetailsWidget", Title = "Health Article Details Widget", SectionName = "CustomWidgets")]
    public class HealthArticleDetailsWidgetController : Controller, IPersonalizable, IContentLocatableView
    {

        private readonly HealthArticlesService _service = new HealthArticlesService();

        public ActionResult Index()
        {
            string urlPath = Request.Url.AbsolutePath;
            string[] segments = urlPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string urlName = segments.LastOrDefault();

            if (string.IsNullOrEmpty(urlName))
            {
                return Content("No article specified.");
            }

            var article = _service.GetHealthArticleByUrl(urlName);
            if (article == null)
            {
                return HttpNotFound();
            }

            var articles = _service.GetLatestArticles(100);
            var currentIndex = articles.FindIndex(a => a.Id == article.Id);
            var previousArticle = currentIndex > 0 ? articles[currentIndex - 1] : null;
            var nextArticle = currentIndex < articles.Count - 1 ? articles[currentIndex + 1] : null;

            bool isSubscriber = Request.Cookies["Subscriber"] != null
                    && Request.Cookies["Subscriber"]["IsSubscribed"] == "true";

            var currentNode = SiteMapBase.GetCurrentNode();
            string pagePath = currentNode != null ? currentNode.Url.TrimStart('~') : string.Empty;
            string baseUrl = string.Format("{0}://{1}/{2}/",
                Request.Url.Scheme,
                Request.Url.Authority,
                pagePath.Trim('/'));

            string articleContent = isSubscriber
                ? SanitizeHtml(article.Body)
                : SanitizeHtml(article.Body.Length > 300 ? article.Body.Substring(0, 300) + "..." : article.Body);

            var viewModel = new HealthArticleDetailViewModel
            {
                Article = article,
                PreviousArticle = previousArticle,
                NextArticle = nextArticle,
                IsSubscriber = isSubscriber,
                ArticleContent = articleContent,
                PreviousArticleURL = previousArticle != null ? baseUrl + FormatUrl(previousArticle.Title) : null,
                NextArticleURL = nextArticle != null ? baseUrl + FormatUrl(nextArticle.Title) : null,
                ListPageURL = baseUrl
            };

            return View("HealthArticleDetails", viewModel);
        }

        private string FormatUrl(string title)
        {
            return title.ToLower().Replace(" ", "-").Replace(",", "-").Replace(".", "").Replace("'", "").Replace("\"", "").Replace(":", "-");
        }

        private string SanitizeHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.OptionFixNestedTags = true;
            doc.LoadHtml(html ?? string.Empty);
            return doc.DocumentNode.OuterHtml;
        }

        protected override void HandleUnknownAction(string actionName)
        {
            this.ActionInvoker.InvokeAction(this.ControllerContext, "Index");
        }

        public bool? DisableCanonicalUrlMetaTag { get; set; } = false;

        string BlogPostModuleType = "Telerik.Sitefinity.DynamicTypes.Model.HealthArticlesBlogPosts.Healtharticlesblogposts";

        public System.Collections.Generic.IEnumerable<IContentLocationInfo> GetLocations()
        {
            var location = new ContentLocationInfo();
            var contentType = TypeResolutionService.ResolveType(BlogPostModuleType);

            location.ContentType = contentType;

            string providerName = string.Empty;
            var manager = DynamicModuleManager.GetManager(providerName);
            var fullProviderName = manager.Provider.Name;

            location.ProviderName = fullProviderName;

            yield return location;
        }
        IEnumerable<IContentLocationInfo> IContentLocatableView.GetLocations()
        {
            return GetLocations();
        }
    }
}
