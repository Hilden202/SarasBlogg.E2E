using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace SarasBlogg.E2E;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class BasicTests : PageTest
{
    public override BrowserNewContextOptions ContextOptions()
        => new() { IgnoreHTTPSErrors = true };

    [Test, Category("Smoke"), Category("UI")]
    public async Task CookiePopup_Should_Disappear_WhenAccepted()
    {
        var baseUrl = TestContext.Parameters.Get("BaseUrl", "https://localhost:7130");
        await Page.GotoAsync(baseUrl);

        var acceptBtn = Page.GetByRole(AriaRole.Button, new() { Name = "Acceptera" });
        await Expect(acceptBtn).ToBeVisibleAsync();
        await acceptBtn.ClickAsync();

        await Expect(acceptBtn).Not.ToBeVisibleAsync();
    }

    [Test, Category("Kontakt"), Category("Form")]
    public async Task ContactForm_Should_Save_WhenNotSpam()
    {
        var baseUrl = TestContext.Parameters.Get("BaseUrl", "https://localhost:7130");
        await Page.GotoAsync(baseUrl + "/Contact");

        var acceptBtn = Page.GetByRole(AriaRole.Button, new() { Name = "Acceptera" });
        if (await acceptBtn.IsVisibleAsync()) await acceptBtn.ClickAsync();

        await Expect(Page.Locator("form.contact-form")).ToBeVisibleAsync();

        string[] variants =
        {
            "Hej! Jag gillar din blogg.",
            "Tack för en fin sida.",
            "Ville bara säga hej!",
            "Fint skrivet – inspirerande!",
            "Bra inlägg, ser fram emot mer."
        };
        var message = variants[Random.Shared.Next(variants.Length)] + " (E2E)";

        // Fyll fälten (unik e-post via stamp, men ren text i ämne/meddelande)
        await Page.FillAsync("#ContactMe_Name", "E2E Test");
        await Page.FillAsync("#ContactMe_Email", $"e2e+@test.local");
        await Page.FillAsync("#ContactMe_Subject", "E2E-save");
        await Page.FillAsync("#ContactMe_Message", message);


        await Task.Delay(6000); // anti-spam wait

        await Page.GetByRole(AriaRole.Button, new() { Name = "Skicka" }).ClickAsync();
        await Page.WaitForURLAsync(url => url.Contains("/Contact"));

        await Expect(Page.Locator(".alert.alert-success"))
            .ToHaveTextAsync(new Regex("Tack för ditt meddelande", RegexOptions.IgnoreCase));
    }
}
