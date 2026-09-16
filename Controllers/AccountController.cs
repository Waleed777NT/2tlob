using _2tlob.Data;
using _2tlob.Enum;
using _2tlob.Helpers;
using _2tlob.Models;
using _2tlob.Services.Interfaces;
using _2tlob.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace _2tlob.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISellerService _sellerService;
        private readonly IOrderService _orderService;
        private readonly IWishlistService _wishlistService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ISellerService sellerService,
            IOrderService orderService,
            IWishlistService wishlistService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _sellerService = sellerService;
            _orderService = orderService;
            _wishlistService = wishlistService;
        }


        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid) return View(model);

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                ModelState.AddModelError("Email", "An account with this email address already exists.");
                return View(model);
            }

            var newUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName.Trim(),
                Status = UserStatus.Active,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, "Customer");
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                TempData["Success"] = "Welcome, Your account has been created successfully!";

                if (!string.IsNullOrEmpty(returnUrl) || Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            if (user.Status == UserStatus.Suspended)
            {
                ModelState.AddModelError(string.Empty, "This account has been suspended. Please contact customer support.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName ?? model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Welcome back, {user.FullName}!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

                if (await _userManager.IsInRoleAsync(user, "Admin")) return RedirectToAction("Dashboard", "Admin");
                if (await _userManager.IsInRoleAsync(user, "Seller")) return RedirectToAction("Dashboard", "Seller");

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "This account has been locked out due to multiple failed login attempts. Please try again in 15 minutes.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Info"] = "You have been successfully logged out.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = RoleHelper.GetDisplayRole(roles);

            var pendingRequest = await _sellerService.GetUserPendingRequestAsync(user.Id);
            var customerOrders = await _orderService.GetCustomerOrdersAsync(user.Id);
            var wishlistCount = await _wishlistService.GetWishlistItemCountAsync(user.Id);

            var viewModel = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = primaryRole,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                PendingSellerRequest = pendingRequest,
                TotalOrders = customerOrders.Count(),
                WishlistCount = wishlistCount
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Role = RoleHelper.GetDisplayRole(roles);
                model.Email = user.Email ?? string.Empty;
                model.Status = user.Status;
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                TempData["Success"] = "Your profile has been updated successfully.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied(string? reason = null)
        {
            ViewBag.Reason = reason;
            return View();
        }
    }
}