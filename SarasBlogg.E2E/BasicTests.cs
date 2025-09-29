using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace SarasBlogg.E2E;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class BasicTests : PageTest
{
    // Tillåt dev-cert lokalt
    public override BrowserNewContextOptions ContextOptions()
        => new() { IgnoreHTTPSErrors = true };

    // Avkommentera för att se browsern när du felsöker:
    // public override BrowserTypeLaunchOptions LaunchOptions() => new() { Headless = false };

    [Test]
    public async Task HomePage_ShouldHave_Title()
    {
        var baseUrl = TestContext.Parameters.Get("BaseUrl", "https://localhost:7130");
        await Page.GotoAsync(baseUrl);
        var title = await Page.TitleAsync();
        TestContext.WriteLine($"Title: {title}");
        Assert.That(title, Does.Contain("med Hjärtat som Kompass").IgnoreCase);

    }
}
