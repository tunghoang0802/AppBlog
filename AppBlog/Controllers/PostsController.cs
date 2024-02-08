using AppBlog.Helpers;
using AppBlog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace AppBlog.Controllers
{
    public class PostsController : Controller
    {
        private readonly BlogContext? _context;
        public PostsController(BlogContext? context)
        {
            _context = context; 
        }

        //Get: posts
        //Get: Index
        [Route("{Alias}", Name = "ListTin")]

        public IActionResult Index(string Alias, int? page)
        {
            if (string.IsNullOrWhiteSpace(Alias))
                return RedirectToAction("Index","Home");
            if(Alias == "Admin")
            {
                return Redirect("~/Admin/Home");
            }

            var danhmuc = _context!.Categories!.FirstOrDefault(x => x.Alias == Alias);
            if (danhmuc == null)
                return RedirectToAction("Index", "Home");

            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;

            var lsPosts = _context.Posts.Include(x => x.Cat).Where(x => x.Cat!.Alias == Alias).AsNoTracking()
                .OrderByDescending(x => x.CreatedDate);
            PagedList<Post> models = new PagedList<Post>(lsPosts, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.DanhMuc = danhmuc;

            return View(models);
            
        }

        [Route("{Alias}.html", Name = "PostsDetails")]
        public async Task<IActionResult> GetDetails(string Alias)
        {
            if (string.IsNullOrWhiteSpace(Alias))
            {
                return NotFound();
            }

            var post = await _context!.Posts
                .Include(p => p.Account)
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.Alias == Alias);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
        private bool PostExists(int id)
        {
            return _context!.Posts.Any(e => e.PostId == id);
        }

    }
}
