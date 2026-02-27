using Microsoft.Playwright;

namespace CampLog.Tests;

public class AuthSpecs
{
    private const string AppUrl = "http://localhost:5000";

    [Fact]
    public async Task Login_RedirectsToKeycloak()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{AppUrl}/Account/Login");

        // Should redirect to Keycloak login page
        await page.WaitForURLAsync("**/realms/camplog/**");
        Assert.Contains("realms/camplog", page.Url);
    }

    [Fact]
    public async Task Login_SuccessRedirectsToHome()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{AppUrl}/Account/Login");
        await page.WaitForURLAsync("**/realms/camplog/**");

        await page.FillAsync("#username", "testuser");
        await page.FillAsync("#password", "testpass");
        await page.ClickAsync("[type=submit]");

        await page.WaitForURLAsync($"{AppUrl}/**");
        Assert.StartsWith(AppUrl, page.Url);
    }

    [Fact]
    public async Task Logout_ClearsSession()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{AppUrl}/Account/Logout");

        // After logout, navigating to a protected page should redirect to login
        await page.GotoAsync($"{AppUrl}/Trips");
        Assert.Contains("login", page.Url.ToLower());
    }
}
