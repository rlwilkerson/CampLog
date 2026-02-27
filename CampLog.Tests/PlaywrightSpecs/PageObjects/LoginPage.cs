using Microsoft.Playwright;

namespace CampLog.Tests.PlaywrightSpecs.PageObjects;

public class LoginPage : BasePage
{
    public LoginPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public ILocator UsernameField => Page.Locator("#username");
    public ILocator PasswordField => Page.Locator("#password");
    public ILocator SubmitButton  => Page.Locator("[type=submit]").First;

    public async Task<LoginPage> GotoAsync()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.WaitForURLAsync("**/realms/**", new PageWaitForURLOptions { Timeout = 15_000 });
        return this;
    }

    public async Task LoginAsync(string username, string password)
    {
        await UsernameField.FillAsync(username);
        await PasswordField.FillAsync(password);
        await SubmitButton.ClickAsync();
        await Page.WaitForURLAsync($"{BaseUrl}/**", new PageWaitForURLOptions { Timeout = 15_000 });
    }
}
