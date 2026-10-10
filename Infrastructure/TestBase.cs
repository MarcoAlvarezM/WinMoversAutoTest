using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using WinMoversAutoTest.Pages;
using Simulacion3_Calidad_Software.Utils;

namespace WinMoversAutoTest.Infrastructure;

public abstract class TestBase
{
    protected IPlaywright Playwright = null!;
    protected IBrowser Browser = null!;
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;

    protected const string BaseUrl = "http://3.15.121.133/";
    //protected const string BaseUrl = "http://localhost:5000/";

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

    // Inicio de sesión compartido por los tests de Órdenes y Cotizaciones.
    // Va a la raíz del sitio usando la BaseUrl de este mismo archivo, así que
    // no depende de la IP que tenga LoginPage.
    protected async Task IniciarSesionAsync()
    {
        await Page.GotoAsync("/");

        var loginPage = new LoginPage(Page);
        await loginPage.LoginAsync(TestConfig.Email, TestConfig.Password);

        await Page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/");
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