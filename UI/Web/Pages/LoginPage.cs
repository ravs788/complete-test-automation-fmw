using OpenQA.Selenium;
using Core.Utilities;

namespace UI.Web.Pages
{
    public class LoginPage : BasePage
    {
        private readonly By usernameInput = By.Id("user-name");
        private readonly By passwordInput = By.Id("password");
        private readonly By loginButton = By.Id("login-button");

        public LoginPage(IWebDriver driver) : base(driver) { }

        public void EnterUsername(string username)
        {
            Driver.FindElement(usernameInput).Clear();
            Driver.FindElement(usernameInput).SendKeys(username);
        }

        public void EnterPassword(string password)
        {
            Driver.FindElement(passwordInput).Clear();
            Driver.FindElement(passwordInput).SendKeys(password);
        }

        public void ClickLogin()
        {
            Driver.FindElement(loginButton).Click();
        }

        public void Login(string username, string password)
        {
            EnterUsername(username);
            EnterPassword(password);
            ClickLogin();
        }
    }
}
