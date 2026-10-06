using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace WinMoversAutoTest.Infrastructure;

public abstract class TestBase
{
    protected IPlaywright Playwright = null!;
    protected IBrowser Browser = null!;
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;

    protected const string BaseUrl = "https://localhost:7058";

    [SetUp]
    public async Task SetUp()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true
        });

        Page = await Context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status
            == TestStatus.Failed)
        {
            var screenshotsDirectory = Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                "screenshots");

            Directory.CreateDirectory(screenshotsDirectory);

            var fileName = $"{TestContext.CurrentContext.Test.Name}.png"
                .Replace(" ", "_");

            var screenshotPath = Path.Combine(
                screenshotsDirectory,
                fileName);

            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });

            TestContext.AddTestAttachment(
                screenshotPath,
                "Screenshot del fallo");
        }

        await Context.CloseAsync();
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
}