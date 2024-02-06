using AppBlog.Enums;
using AppBlog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AppBlog.Controllers.Components
{
    public class PopularViewComponent : ViewComponent
    {
        private readonly BlogContext _dbContext;
        private IMemoryCache _cache;

        public PopularViewComponent(BlogContext dbContext, IMemoryCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public IViewComponentResult Invoke()
        {
            var popularPosts = _cache.GetOrCreate(CacheKeys.Popular, entry =>
            {
                entry.SlidingExpiration = TimeSpan.MaxValue;
                return RetrievePopularPosts();
            });

            return View(popularPosts);
        }

        public List<Post> RetrievePopularPosts()
        {
            List<Post> posts = new List<Post>();
            posts = _dbContext.Posts
                .Where(post => post.Published == true)
                .Take(6)
                .ToList();

            return posts;
        }
    }
}
