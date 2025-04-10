using System;
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
using MercolaSiteFinity.MVC.Controllers;
using MercolaSiteFinity.MVC.Models;
using Telerik.Sitefinity.Modules.GenericContent;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Multisite;

namespace MercolaSiteFinity.MVC.ViewModel
{
    public class HealthArticleViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Body { get; set; }
        public bool iKnownHealthUser { get; set; }
    }

    public class HealthArticleDetailViewModel
    {
        public HealthArticleViewModel Article { get; set; }
        public HealthArticleViewModel PreviousArticle { get; set; }
        public HealthArticleViewModel NextArticle { get; set; }
        public bool IsSubscriber { get; set; }  // Determines if the user is a subscriber
        public string ArticleContent { get; set; }
        public string PreviousArticleURL { get; set; }
        public string NextArticleURL { get; set; }
        public string ListPageURL { get; set; }
    }
    public class HealthArticlesService
    {
        public List<HealthArticleViewModel> GetLatestArticles(int count = 10)
        {
            var currentContext = SystemManager.CurrentContext;
            var multisiteContext = currentContext.MultisiteContext;
            var currentSite = multisiteContext.CurrentSite;

            var providerName = currentSite.GetProviders("HealthArticlesBlogPosts").Select(p => p.ProviderName).First();

            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(providerName);
            Type healthArticleType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.HealthArticlesBlogPosts.Healtharticlesblogposts");

            using (new SiteRegion(currentSite))
            {
                var articles = dynamicModuleManager.GetDataItems(healthArticleType)
                    .Where(a => a.Status == ContentLifecycleStatus.Live)
                    .ToList()
                    .OrderByDescending(a => (a.GetValue<DateTime?>("PostDate") ?? DateTime.MinValue))
                    .Take(count)
                    .Select(a => new HealthArticleViewModel
                    {
                        Id = a.Id,
                        Title = a.GetString("Title"),
                        Author = a.GetString("PostAuthor"),
                        Date = a.GetValue<DateTime?>("PostDate") ?? DateTime.MinValue,
                        Body = a.GetString("Body")
                    })
                    .ToList();

                return articles;
            }
        }
        public HealthArticleViewModel GetHealthArticleByUrl(string urlName)
        {
            var currentContext = SystemManager.CurrentContext;
            var multisiteContext = currentContext.MultisiteContext;
            var currentSite = multisiteContext.CurrentSite;

            var providerName = currentSite.GetProviders("HealthArticlesBlogPosts").Select(p => p.ProviderName).First();

            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(providerName);
            Type healthArticleType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.HealthArticlesBlogPosts.Healtharticlesblogposts");

            using (new SiteRegion(currentSite))
            {
                var article = dynamicModuleManager.GetDataItems(healthArticleType)
                    .Where(a => a.Status == ContentLifecycleStatus.Live && a.Visible)
                    .FirstOrDefault(a => a.GetValue<string>("UrlName") == urlName);

                if (article == null)
                    return null;

                return new HealthArticleViewModel
                {
                    Id = article.Id,
                    Title = article.GetString("Title"),
                    Author = article.GetString("PostAuthor"),
                    Date = article.GetValue<DateTime?>("PostDate") ?? DateTime.MinValue,
                    Body = article.GetString("Body")
                };
            }
        }
    }
}
