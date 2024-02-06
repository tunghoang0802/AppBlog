using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppBlog.Models;
using AppBlog.Helpers;
using PagedList.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace AppBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize()]
    public class PostsController : Controller
    {
        private readonly BlogContext _context;
        private readonly IWebHostEnvironment _environment;

        public PostsController(BlogContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Posts
        public IActionResult Index(int? page)
        {
            if (!User.Identity!.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var accountId = HttpContext.Session.GetString("AccountId");
            if (accountId == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });

            var userAccount = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(accountId));
            if (userAccount == null) return NotFound();

            List<Post> lsPosts = new List<Post>();

            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = Utilities.Page_Size;

            // Kiểm tra nếu tài khoản là Admin
            if (userAccount.RoleId == 1)
            {
                // Lấy danh sách bài viết, bao gồm thông tin tài khoản và danh mục, sắp xếp theo ID danh mục giảm dần
                lsPosts = _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Cat)
                .OrderByDescending(x => x.CatId)
                .ToList();
            }
            else // Trường hợp tài khoản không phải Admin
            {
                // Lấy danh sách bài viết của tài khoản hiện tại, bao gồm thông tin tài khoản và danh mục, sắp xếp theo ID danh mục giảm dần
                lsPosts = _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Cat)
                .Where(x => x.AccountId == userAccount.AccountId)
                .OrderByDescending(x => x.CatId)
                .ToList();
            }

            // Chuyển danh sách bài viết thành danh sách phân trang
            PagedList<Post> models = new PagedList<Post>(lsPosts.AsQueryable(), pageNumber, pageSize);

            // Trả về view với models là danh sách phân trang của bài viết
            return View(models);

        }

        // GET: Admin/Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Admin/Posts/Create
        public IActionResult Create()
        {
            // Kiểm tra nếu người dùng có quyền truy cập không
            if (!User.Identity!.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var accountId = HttpContext.Session.GetString("AccountId");
            if (accountId == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName");
            return View();

        }

        // POST: Admin/Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Title,Scontents,Contents,Thumb,Published,Alias,CreatedDate,Author,AccountId,Tags,CatId,IsHot,IsNewfeed")] Post post, IFormFile? fThumb)
        {
            if (!User.Identity!.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var accountId = HttpContext.Session.GetString("AccountId");
            if (accountId == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });

            var userAccount = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(accountId));
            if (userAccount == null) return NotFound();

            if (ModelState.IsValid)
            {
                post.AccountId = userAccount.AccountId;
                post.Author = userAccount.FullName;
                if (post.CatId == null) post.CatId = 1;
                post.CreatedDate = DateTime.Now;
                post.Alias = Utilities.SEOUrl(post.Title);
                
               
                if (fThumb != null)
                {
                    string fileExtension = Path.GetExtension(fThumb.FileName);
                    string newName = Utilities.SEOUrl(post.Title) + "preview_" + fileExtension;
                    post.Thumb = await Utilities.UploadFile(fThumb, @"posts\", newName.ToLower());
                }
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }           
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", post.CatId);
            return View(post);
        }

        // GET: Admin/Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId", post.AccountId);
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", post.CatId);
            return View(post);
        }

        // POST: Admin/Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Scontents,Contents,Thumb,Published,Alias,CreatedDate,Author,AccountId,Tags,CatId,IsHot,IsNewfeed")] Post post, IFormFile? fThumb)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }

            if (!User.Identity!.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var accountId = HttpContext.Session.GetString("AccountId");
            if (accountId == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });

            var userAccount = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(accountId));
            if (userAccount == null) return NotFound();

            if(userAccount.AccountId != 1)
            {
                if (post.AccountId != userAccount.AccountId) return RedirectToAction(nameof(Index));
            }

           

            if (ModelState.IsValid)
            {
                try
                {
                    post.Alias = Utilities.SEOUrl(post.Title);
                    post.AccountId = userAccount.AccountId;
                    post.Author = userAccount.FullName;
                    if (fThumb != null)
                    {
                        string fileExtension = Path.GetExtension(fThumb.FileName);
                        string newName = Utilities.SEOUrl(post.Title) + "preview_" + fileExtension;
                        post.Thumb = await Utilities.UploadFile(fThumb, @"posts\", newName.ToLower());
                    }
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId", post.AccountId);
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", post.CatId);
            return View(post);
        }

        // GET: Admin/Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Account)
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Admin/Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                string Path = _environment.WebRootPath + "/images/posts/" + post.Thumb;
                System.IO.File.Delete(Path);

               
                
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
