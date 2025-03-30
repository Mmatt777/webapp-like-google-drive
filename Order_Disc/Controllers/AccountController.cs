using Microsoft.AspNetCore.Mvc;
using Order_Disc.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Order_Disc.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Order_Disc.Controllers
{
    public class AccountController : Controller
    {
        private readonly AffDbContext _context;

        public AccountController(AffDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationVM model)
        {
            if (ModelState.IsValid)
            {
                var emailExists = await _context.UserAccounts.AnyAsync(u => u.Email == model.Email);
                var usernameExists = await _context.UserAccounts.AnyAsync(u => u.UserName == model.UserName);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                }

                if (usernameExists)
                {
                    ModelState.AddModelError("UserName", "This username is already taken.");
                }

                if (!emailExists && !usernameExists)
                {
                    var newUser = new UserAccounts
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserName = model.UserName,
                        Email = model.Email,
                        Password = model.Password
                    };

                    _context.UserAccounts.Add(newUser);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Welcome {newUser.FirstName} {newUser.LastName}! Your account has been successfully registered.";

                    return RedirectToAction("Login", "Account");
                }
            }

            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.UserAccounts
                    .FirstOrDefaultAsync(u =>
                        (u.UserName == model.UserNameOrEmail || u.Email == model.UserNameOrEmail)
                        && u.Password == model.Password);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim("Email", user.Email),
                        new Claim(ClaimTypes.Role, "User")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    TempData["SuccessMessage"] = $"Hi, {user.FirstName} {user.LastName}!";
                    return RedirectToAction("MyDisc", "Folder");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username, email address, or password.");
                }
            }
            return View(model);
        }
    }
}
