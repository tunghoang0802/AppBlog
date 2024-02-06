using AppBlog.Enums;
using AppBlog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AppBlog.Controllers.Components
{
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly BlogContext _context;
        private IMemoryCache _memoryCache;

        public CategoriesViewComponent(BlogContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public IViewComponentResult Invoke()
        {
            var _lsDanhMuc = _memoryCache.GetOrCreate(CacheKeys.Categories, entry =>
            {
                entry.SlidingExpiration = TimeSpan.MaxValue;
                return GetIsCategories();
            });
            return View(_lsDanhMuc);
        }

        public List<Category> GetIsCategories()
        {
            List<Category> listins = new List<Category>();
            listins = _context.Categories
                              .AsNoTracking()
                              //.Where(x => x.Published == true)
                              .OrderBy(x => x.Ordering)
                              .ToList();
            return listins;
        }
    }
}
