using OpenQA.Selenium;
using Core.Utilities;

namespace UI.Web.Pages
{
    public class LoginPage : BasePage
    {
        private readonly By usernameInput = By.Id("user-name");
        private readonly By passwordInput = By.Id("password");
        private readonly By loginButton = By.Id("login-button");
        private readonly By errorContainer = By.CssSelector("[data-test='error']");

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

        public bool IsErrorDisplayed()
        {
            try
            {
                return Driver.FindElement(errorContainer).Displayed;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }

        public string GetErrorText()
        {
            try
            {
                return Driver.FindElement(errorContainer).Text;
            }
            catch (NoAlertPresentException)
            {
                return string.Empty;
            }
        }

        public void Login(string username, string password)
        {
            EnterUsername(username);
            EnterPassword(password);
            ClickLogin();
            HandleAlertIfPresent();
        }

        private void HandleAlertIfPresent()
        {
            try
            {
                var alert = Driver.SwitchTo().Alert();
                alert.Accept();
            }
            catch (NoAlertPresentException) { }
        }
    }
}
