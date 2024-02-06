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
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AppBlog.Areas.Admin.Models;
using AppBlog.Extension;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace AppBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountsController : Controller
    {
        private readonly BlogContext _context;

        public AccountsController(BlogContext context)
        {
            _context = context;
        }

        // GET: Admin/Accounts
       
        public IActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = Utilities.Page_Size;
            var lsAccounts = _context.Accounts.Include(a => a.Role).OrderByDescending(x => x.CreatedDate);
            PagedList<Account> models = new PagedList<Account>(lsAccounts, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        //Get: Admin/Login
        [HttpGet]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "Login")]

        public IActionResult Login(string? returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID != null)
                return RedirectToAction("Index", "Home", new { Area = "Admin" });
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "Login")]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Account? kh = _context.Accounts
                        .Include(a => a.Role)
                        .SingleOrDefault(a => a.Email!.ToLower() == model.Email!.ToLower().Trim());

                    if(kh == null)
                    {
                        ViewBag.Error = "Thông tin đăng nhập chưa chính xác";
                        return View(model);
                    }
                    string pass;
                    if (kh.Salt == null)
                    {
                        pass = model.Password!.Trim();
                    }
                    else
                    {
                        pass = (model.Password!.Trim() + kh.Salt!.Trim().ToMD5());
                    }
                    if(kh.Password!.Trim() != pass)
                    {
                        ViewBag.Error = "Thông tin đăng nhập chưa chính xác";
                        return View(model);
                    }
                    //Dang nhap thanh cong

                    //ghi nhan thoi gian dang nhap
                    kh.LastLogin = DateTime.Now;
                    _context.Update(kh);
                    await _context.SaveChangesAsync();

                    var taikhoanID = HttpContext.Session.GetString("AccountId");
                    //Identity
                    // luu session makh
                    HttpContext.Session.SetString("AccountId", kh.AccountId.ToString());
                    //Identity
                    var userClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, kh.FullName!),
                        new Claim(ClaimTypes.Email, kh.Email!),
                        new Claim("AccountId", kh.AccountId.ToString()),
                        new Claim("RoleId", kh.RoleId!.ToString()!),
                        new Claim(ClaimTypes.Role, kh.Role!.RoleName!)
                    };
                    var grandmaIdentity = new ClaimsIdentity(userClaims, "User Identity");
                    var userPrincipal = new ClaimsPrincipal(new[] {grandmaIdentity});
                    await HttpContext.SignInAsync(userPrincipal);

                    //if (Url.IsLocalUrl(returnUrl))
                    //{
                    //    return Redirect(returnUrl);
                    //}
                    return RedirectToAction("Index", "Home", new { Area = "Admin" });
                }
            }
            catch
            {
                return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            }
            return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
        }

        [Route("doi-mat-khau.html", Name = "ChangePassword")]
        [Authorize, HttpGet]
        public IActionResult ChangePassword()
        {
            // Kiểm tra xem người dùng đã được xác thực chưa
            if (!User.Identity!.IsAuthenticated)
            {
                // Nếu chưa, chuyển hướng họ đến trang đăng nhập
                Response.Redirect("/dang-nhap.html");
            }

            // Lấy ID tài khoản từ phiên
            var taikhoanID = HttpContext.Session.GetString("AccountId");

            // Nếu ID tài khoản không tồn tại trong phiên, chuyển hướng đến trang đăng nhập trong khu vực quản trị
            if (taikhoanID == null)
            {
                return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            }

            // Nếu người dùng đã được xác thực và có ID tài khoản trong phiên, hiển thị trang
            return View();
        }

        [Route("doi-mat-khau.html", Name = "ChangePassword")]
        [Authorize, HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!User.Identity!.IsAuthenticated) Response.Redirect("/dang-nhap.html");
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
            if (ModelState.IsValid)
            {
                

                var account = _context.Accounts.AsNoTracking().FirstOrDefault(x => x.AccountId == int.Parse(taikhoanID));
                if(account == null) return RedirectToAction("Login", "Accounts", new { Area = "Admin" });
                try
                {
                    string passnow;
                    // Check if the current password is correct
                    if (account!.Salt == null)
                    {
                        passnow = model.Password!.Trim();
                    }
                    else
                    {
                        passnow = (model.Password!.Trim() + account.Salt!.Trim().ToMD5());
                    }
                    
                    if (passnow == account.Password!.Trim()) // Correct password
                    {
                        if (account.Salt == null)
                        {
                            account.Password = model.Password;
                        }
                        else
                        {
                            account.Password = (model.Password + account.Salt.Trim()).ToMD5();
                        }
                        // Change to new password
                        
                        _context.Update(account);
                        _context.SaveChanges();
                        return RedirectToAction("Profile", "Accounts", new { Area = "Admin" });
                    }
                    else // Incorrect password; return to view
                    {
                        return View();
                    }
                }
                catch
                {
                    return View();
                }
            }
            return View();
        }


        [Route("dang-xuat.html", Name = "Logout")]
        public IActionResult Logout()
        {
            try
            {
                HttpContext.SignOutAsync();
                HttpContext.Session.Remove("AccountId");
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

    




        // GET: Admin/Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Admin/Accounts/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId");
            return View();
        }

        // POST: Admin/Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,FullName,Email,Phone,Password,Salt,Active,CreatedDate,RoleId,LastLogin")] Account account)
        {
            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // GET: Admin/Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // POST: Admin/Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,FullName,Email,Phone,Password,Salt,Active,CreatedDate,RoleId,LastLogin")] Account account)
        {
            if (id != account.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountId))
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
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // GET: Admin/Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Admin/Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountId == id);
        }
    }
}
