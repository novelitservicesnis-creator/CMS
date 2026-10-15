namespace VSMovingServices.Web.Models;

// One catalog keeps page metadata, canonical URLs and sitemap entries aligned.
public record SeoPage(string Action, string Path, string Title, string Description, string SchemaType = "WebPage");

public static class SiteSeo
{
    // Public website address already published in the privacy policy.
    public const string Origin = "https://www.vsmoving.com";
    public const string Name = "VS Moving Services";
    public static readonly IReadOnlyList<SeoPage> Pages = new[]
    {
        new SeoPage("Index", "/", "Halifax Movers & Junk Removal | VS Moving Services",
            "Moving or clearing clutter in Halifax? Get a free quote for home and office moves, packing and junk removal in Halifax, Dartmouth, Bedford and nearby areas."),
        new SeoPage("AboutUs", "/AboutUs", "About Our Halifax Moving Team | VS Moving Services",
            "Meet VS Moving Services, your Halifax team for residential and commercial moves, careful furniture handling and junk removal across surrounding communities.", "AboutPage"),
        new SeoPage("PrivacyPolicy", "/PrivacyPolicy", "Privacy Policy | VS Moving Services Halifax",
            "Learn how VS Moving Services collects, uses and protects personal information when you visit our website, request a quote or book moving and junk removal."),
        new SeoPage("TermsAndConditions", "/TermsAndConditions", "Moving & Junk Removal Terms | VS Moving Services Halifax",
            "Read the terms for VS Moving Services in Halifax, including moving and junk removal quotes, scheduling, cancellations and customer responsibilities.")
    };
    public static SeoPage? Find(string? action) => Pages.FirstOrDefault(p => string.Equals(p.Action, action, StringComparison.OrdinalIgnoreCase));
    public static string Url(string path) => Origin + path;
}
