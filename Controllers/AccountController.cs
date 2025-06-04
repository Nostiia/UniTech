using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniTech.Models;
using UniTech.ViewsModels;
using Microsoft.AspNetCore.Authorization;

namespace UniTech.Controllers
{
    public class AccountController : Controller
    {
        private UserContext userContext;
        private PostContext postContext;
        public AccountController(UserContext userContext, PostContext postContext)
        {
            this.userContext = userContext;
            this.postContext = postContext; 
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = new List<string> { "teacher", "student" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await userContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    user = new User
                    {
                        Email = model.Email,
                        Username = model.Username,
                        Password = model.Password,
                        Role = model.Role
                    };
                    userContext.Users.Add(user);
                    await userContext.SaveChangesAsync();
                    await Authenticate(user);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect email or password");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await userContext.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);
                
                if (user != null)
                {
                    await Authenticate(user); // authentication

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Incorrect login and(or) password");
            }
            return View(model);
        }


        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // this one is critical
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Login");

            int userId = int.Parse(userIdClaim.Value);

            // Load user with related posts (only for teachers)
            var user = await userContext.Users
                .Include(u => u.Posts) // assuming navigation property exists
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user); // pass the whole user object to the view
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await userContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user); // This populates the model
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

                try
                {
                    var existingUser = await userContext.Users.FindAsync(id);
                    if (existingUser == null)
                        return NotFound();

                    // Update only fields you allow to change
                    existingUser.Username = user.Username;
                    existingUser.Email = user.Email;
                    // Don't update Role or other critical properties unless allowed

                    await userContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Profile), new { id = existingUser.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                        return NotFound();
                    throw;
                }
            
        }

        [HttpGet]
        public async Task<IActionResult> ProfileView(int id)
        {
            User user = await userContext.Users
                .Include(u => u.Posts)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private bool UserExists(int id)
        {
            return userContext.Users.Any(e => e.Id == id);
        }
    }
}
