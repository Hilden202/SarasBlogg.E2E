using Microsoft.Playwright;
using NUnit.Framework;

namespace SarasBlogg.E2E;

[TestFixture, Category("Debug"), Explicit]
public class DebugTests
{
    private IPlaywright _pw = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _ctx = null!;
    private IPage _page = null!;

    [SetUp]
    public async Task Setup()
    {
        _pw = await Playwright.CreateAsync();
        _browser = await _pw.Chromium.LaunchAsync(new()
        {
            Headless = false,         // visa browsern i VS
            SlowMo = 200,             // se stegen
            // Channel = "msedge"     // valfritt: kör i Edge
        });
        _ctx = await _browser.NewContextAsync(new() { IgnoreHTTPSErrors = true });
        _page = await _ctx.NewPageAsync();
    }

    [TearDown]
    public async Task Teardown()
    {
        await _ctx.CloseAsync();
        await _browser.CloseAsync();
        _pw.Dispose();
    }

    [Test]
    public async Task Contact_Debug_Run_Live()
    {
        var baseUrl = TestContext.Parameters.Get("BaseUrl", "https://localhost:7130");
        await _page.GotoAsync(baseUrl + "/Contact");

        // Stäng ev. cookie-banner
        var acceptBtn = _page.GetByRole(AriaRole.Button, new() { Name = "Acceptera" });
        if (await acceptBtn.IsVisibleAsync()) await acceptBtn.ClickAsync();

        // Inspektor: pausa här, klicka Play i Inspector när du vill fortsätta
        await _page.PauseAsync();

        // Fyll fält
        await _page.FillAsync("#ContactMe_Name", "E2E Debug");
        await _page.FillAsync("#ContactMe_Email", "e2e+debug@test.local");
        await _page.FillAsync("#ContactMe_Subject", "E2E debug post");
        await _page.FillAsync("#ContactMe_Message", "Live debug post (E2E)");

        // Anti-spam: vänta >= 5s innan submit
        await Task.Delay(6000);

        // Skicka
        await _page.GetByRole(AriaRole.Button, new() { Name = "Skicka" }).ClickAsync();

        // Redirect tillbaka + bekräftelse
        await _page.WaitForURLAsync(u => u.Contains("/Contact"));
        await _page.WaitForSelectorAsync(".alert.alert-success"); // "Tack för ditt meddelande"
    }

}
