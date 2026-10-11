using Microsoft.Playwright;
using NUnit.Framework;
using Simulacion3_Calidad_Software.Utils;
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
            TestConfig.Email,
            TestConfig.Password);

        await Page.WaitForURLAsync(
            url => new Uri(url).AbsolutePath == "/");

        Assert.That(
            new Uri(Page.Url).AbsolutePath,
            Is.EqualTo("/"));
    }
}