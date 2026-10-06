using AgamEstates.Core;
using AgamEstates.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AgamEstates.Web.Controllers
{
    public class BaseController : Controller
    {
        public readonly AgamEntities agamEntity;
        protected readonly IConfiguration configuration;
        protected readonly ILogger<BaseController> _logger;
        protected readonly IMemoryCache? _cache;

        public BaseController(
            AgamEntities agamEntities,
            IConfiguration configuration,
            ILogger<BaseController> logger,
            IMemoryCache? cache = null)
        {
            this.agamEntity = agamEntities;
            this.configuration = configuration;
            this._logger = logger;
            this._cache = cache;
        }

        private AgamUnitOfWork? _agamUnitOfWork;
        protected AgamUnitOfWork agamUnitOfWork => _agamUnitOfWork ??= new AgamUnitOfWork(agamEntity, configuration);

        protected DateTime GetIstTimeNow()
        {
            TimeZoneInfo indianZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianZone);
            return indianTime;
        }

        protected string GetCurrentIpAddress()
        {
            var ipAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            return ipAddress;
        }

        protected async Task LogException(Exception ex)
        {
            _logger.LogError(new EventId(), ex.ToString());
            await Task.CompletedTask;
        }

        protected string GenerateSixDigitOtp()
        {
            var random = new Random();
            return random.Next(100000, 1000000).ToString("D6");
        }

        protected IActionResult JsonSuccess(object? data = null, string message = "Operation completed successfully.")
        {
            return Json(new
            {
                success = true,
                message = message,
                data = data
            });
        }

        protected IActionResult JsonError(string message = "An error occurred.", object? details = null, int statusCode = 400)
        {
            Response.StatusCode = statusCode;
            return Json(new
            {
                success = false,
                message = message,
                errors = details
            });
        }
    }
}
