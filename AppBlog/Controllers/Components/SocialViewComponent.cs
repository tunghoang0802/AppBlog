using AppBlog.Enums;
using AppBlog.ModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AppBlog.Controllers.Components
{
    public class SocialViewComponent : ViewComponent
    {
        private readonly IConfiguration _config;
        private IMemoryCache memoryCache;

        public SocialViewComponent(IMemoryCache cache, IConfiguration conf)
        {
            memoryCache = cache;
            _config = conf;
        }

        public IViewComponentResult Invoke()
        {
            var socialData = memoryCache.GetOrCreate(CacheKeys.Social, entry =>
            {
                entry.SlidingExpiration = TimeSpan.MaxValue;
                return FetchSocialLinks();
            });
            return View(socialData);
        }

        public SocialVM FetchSocialLinks()
        {
            SocialVM socialLinks = new SocialVM();
            socialLinks.Facebook = _config.GetValue<string>("SocialLinks:facebook")!;
            socialLinks.Twitter = _config.GetValue<string>("SocialLinks:twitter")!;
            socialLinks.Instagram = _config.GetValue<string>("SocialLinks:instagram")!;
            socialLinks.Printerest = _config.GetValue<string>("SocialLinks:pinterest")!;
            socialLinks.Youtube = _config.GetValue<string>("SocialLinks:youtube")!;

            return socialLinks;
        }
    }
}
