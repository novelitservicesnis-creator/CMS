using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using VSMovingServices.Web.Models;

namespace VSMovingServices.Web.Controllers;

public class SeoController : Controller
{
    [HttpGet("/robots.txt")]
    public IActionResult Robots() => Content("User-agent: *\nAllow: /\nDisallow: /api/\nSitemap: " + SiteSeo.Url("/sitemap.xml") + "\n", "text/plain; charset=utf-8");

    [HttpGet("/sitemap.xml")]
    public IActionResult Sitemap()
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var document = new XDocument(new XElement(ns + "urlset",
            SiteSeo.Pages.Select(page => new XElement(ns + "url", new XElement(ns + "loc", SiteSeo.Url(page.Path))))));
        return Content(document.ToString(), "application/xml; charset=utf-8");
    }
}
