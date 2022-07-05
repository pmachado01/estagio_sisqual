using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoogleChromeDriver
{
    /// <summary>
    /// Dll to use the google chrome driver using selenium.
    /// </summary>
    public class Driver
    {
        #region Methods

        /// <summary>
        /// Initializes a instance of a chrome driver
        /// </summary>
        /// <param name="chromeDriverPath">The path to where the chrome driver exe is stored.</param>
        /// <param name="headless">If true means the user wants the driver to run whithout opening the browser.</param>
        /// <returns>A instance of the chrome driver.</returns>
        public static IWebDriver InitializeDriver(string chromeDriverPath, bool headless)
        {
            IWebDriver chromeDriver;
            ChromeOptions chromeOptions = new ChromeOptions();

            chromeOptions.AddArgument(Properties.ResourcesGoogleChromeDriver.chromeOptionIncognito);
            if (headless)
            {
                chromeOptions.AddArgument(Properties.ResourcesGoogleChromeDriver.chromeOptionHeadless);
            }

            chromeDriver = new ChromeDriver(chromeDriverPath, chromeOptions);
            chromeDriver.Manage().Window.Maximize();

            return chromeDriver;
        }

        /// <summary>
        /// Closes a instance of the chromeDriver
        /// </summary>
        /// <param name="driver">The instance of the chrome driver to close.</param>
        public static void CloseDriver(IWebDriver driver)
        {
            driver.Close();
        }

        /// <summary>
        /// Gets all the result links for a google search.
        /// </summary>
        /// <param name="driver">The driver where the google search will be made.</param>
        /// <returns></returns>
        public static bool GetAllGoogleSearchLinks(IWebDriver driver, string searchStatement, out List<string> links, out string errorMessage)
        {
            links = new List<string> { };

            if (string.IsNullOrEmpty(searchStatement))
            {
                errorMessage = Properties.ResourcesGoogleChromeDriver.ErrorMessageSearchStatementNullOrEmpty;
                return false;
            }

            try
            {
                if (DoGoogleSearch(driver, searchStatement, out errorMessage) == false) 
                {
                    return false;
                }

                List<IWebElement> results = driver.FindElements(By.XPath(Properties.ResourcesGoogleChromeDriver.googleSearchLinkXPath)).ToList();

                foreach (IWebElement element in results)
                {
                    links.Add(element.GetAttribute("href"));
                }

                if (links.Count > 0)
                {
                    return true;
                }
                else
                {
                    errorMessage = Properties.ResourcesGoogleChromeDriver.ErrorMessageNoLinkFound;
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Does a google search for the input searchStatement
        /// </summary>
        /// <param name="driver">The chrome driver to use in order to do the search.</param>
        /// <param name="searchStatement">The search statement to search.</param>
        /// <param name="errorMessage">The error message to output in case of an error.</param>
        /// <returns></returns>
        private static bool DoGoogleSearch(IWebDriver driver, string searchStatement, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                searchStatement = searchStatement.Replace(" ", Properties.ResourcesGoogleChromeDriver.googleSpaceCharacterReplacer);
                driver.Navigate().GoToUrl(Properties.ResourcesGoogleChromeDriver.googleSearchLink + searchStatement);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            return true;
        }

        #endregion
    }
}
