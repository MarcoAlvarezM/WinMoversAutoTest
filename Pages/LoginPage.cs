using Microsoft.Playwright;
using NUnit.Framework;
using WinMoversAutoTest.Infrastructure;
using WinMoversAutoTest.Pages;

namespace WinMoversAutoTest.Tests;

public class AuthenticationTests : TestBase
{
    [Test]
    [Category("Authentication")]
    public async Task TC_AUT_001_InicioDeSesionDeUsuario()
    {
        var loginPage = new LoginPage(Page);

        await loginPage.NavigateAsync();

        await Page.GetByLabel("Correo")
            .WaitForAsync();

        await loginPage.LoginAsync(
            "marcoalvmejia@gmail.com",
            "Marco1003");

        await Page.WaitForURLAsync(
            "**http://3.15.121.133/");

        Assert.That(
            Page.Url,
            Does.Contain("http://3.15.121.133/"));
    }
}