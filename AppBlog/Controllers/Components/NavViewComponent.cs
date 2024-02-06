using AppBlog.Enums;
using AppBlog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AppBlog.Controllers.Components
{
    public class NavViewComponent : ViewComponent
    {
        private readonly BlogContext _context;
        private IMemoryCache _memoryCache;

        public NavViewComponent(BlogContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public IViewComponentResult Invoke()
        {
            var _danhMucs = _memoryCache.GetOrCreate(CacheKeys.Categories, entry =>
            {
                entry.SlidingExpiration = TimeSpan.MaxValue;
                return GetCategories();
            });

            return View(_danhMucs);
        }

        public List<Category> GetCategories()
        {
            List<Category> listins = new List<Category>();
            listins = _context.Categories
                .AsNoTracking()
                //.Where(x => x.Published == false)
                .OrderBy(x => x.Ordering)
                .ToList();

            return listins;
        }
    }
}
