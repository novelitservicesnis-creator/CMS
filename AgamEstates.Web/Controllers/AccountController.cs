using AgamEstates.Core;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.Service;
using AgamEstates.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgamEstates.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IUserRepository _userRepository;

        public AccountController(
            AgamEntities agamEntity,
            IConfiguration configuration,
            ILogger<AccountController> logger,
            IUserRepository userRepository,
            IMemoryCache? cache = null)
            : base(agamEntity, configuration, logger, cache)
        {
            _userRepository = userRepository;
        }

        [HttpGet("/Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Agent";
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.Equals("/Login", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(returnUrl);
                }
                return Redirect("/AdminPanel");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost("/Login")]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                          (Request.Headers.Accept.ToString().Contains("application/json") && !Request.Headers.Accept.ToString().Contains("text/html")) ||
                          (Request.ContentType?.Contains("application/json") == true);

            // Handle possible JSON body binding fallback if model wasn't populated
            if (model == null || (string.IsNullOrWhiteSpace(model.Email) && string.IsNullOrWhiteSpace(model.Password)))
            {
                if (Request.ContentType?.Contains("application/json") == true)
                {
                    try
                    {
                        Request.EnableBuffering();
                        using var reader = new System.IO.StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                        var body = await reader.ReadToEndAsync();
                        Request.Body.Position = 0;
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            var parsed = System.Text.Json.JsonSerializer.Deserialize<LoginViewModel>(body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (parsed != null) model = parsed;
                        }
                    }
                    catch
                    {
                        // ignore deserialization failure
                    }
                }
            }

            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                const string msg = "Please enter both email and password.";
                if (isAjax) return Json(new { success = false, message = msg });
                ViewBag.ErrorMessage = msg;
                ViewBag.ReturnUrl = returnUrl;
                return View(model ?? new LoginViewModel());
            }

            try
            {
                var user = await _userRepository.GetUserByEmailAsync(model.Email.Trim());
                if (user == null)
                {
                    const string msg = "Invalid email address or password.";
                    if (isAjax) return Json(new { success = false, message = msg });
                    ViewBag.ErrorMessage = msg;
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                if (!user.IsActive)
                {
                    const string msg = "Your account is inactive. Please contact the administrator.";
                    if (isAjax) return Json(new { success = false, message = msg });
                    ViewBag.ErrorMessage = msg;
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                var isValid = await _userRepository.ValidatePasswordAsync(user.UserId, model.Password);
                if (!isValid)
                {
                    const string msg = "Invalid email address or password.";
                    if (isAjax) return Json(new { success = false, message = msg });
                    ViewBag.ErrorMessage = msg;
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName ?? "Agam User"),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.Role, user.Role ?? "Agent"),
                    new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? "")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                string targetUrl;
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) &&
                    !returnUrl.Equals("/Login", StringComparison.OrdinalIgnoreCase) &&
                    !returnUrl.Equals("/Logout", StringComparison.OrdinalIgnoreCase))
                {
                    targetUrl = returnUrl;
                }
                else
                {
                    targetUrl = "/AdminPanel";
                }

                if (isAjax)
                {
                    return Json(new { success = true, redirectUrl = targetUrl });
                }

                return Redirect(targetUrl);
            }
            catch (Exception ex)
            {
                await LogException(ex);
                const string msg = "Something went wrong. Please try again.";
                if (isAjax) return Json(new { success = false, message = msg });
                ViewBag.ErrorMessage = msg;
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
        }

        [HttpGet("/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet("/Logout")]
        [HttpPost("/Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Login");
        }
    }
}
