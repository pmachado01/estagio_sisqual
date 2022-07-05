using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

using GoogleChromeDriver;

namespace CompanyGetStoreLink
{
    /// <summary>
    /// Dll to get for a company name, both its general web page link (www.company.com) and its web link with store location details (www.company.com/stores)
    /// </summary>
    public class CompanyGetStoreLinkClass
    {
        #region Methods

        /// <summary>
        /// Gets both links (general and stores details) for a company name
        /// </summary>
        /// <param name="driver">The google web driver to use in the search</param>
        /// <param name="companyName">The company name to search</param>
        /// <param name="keyWords">The key words to use in the search for the store details link</param>
        /// <param name="companyWebLink">The company general web page link</param>
        /// <param name="companyStoresWebLink">the company wep page with stores details link</param>
        /// <param name="errorMessage">The error message in case some error occurs</param>
        /// <returns></returns>
        public static bool GetCompanyWebLinks(IWebDriver driver, string companyName, Dictionary<int, string> keyWords, out string companyWebLink, out string companyStoresWebLink, out string errorMessage)
        {
            companyWebLink = string.Empty;
            companyStoresWebLink = string.Empty;

            if (string.IsNullOrEmpty(companyName) || driver == null)
            {
                errorMessage = Properties.ResourceCompanyGetStoreLink.ErrorMessageCompanyNameOrWebDriver;
                return false;
            }

            if (GetCompanyWebLink(driver, companyName, out companyWebLink, out errorMessage) == false)
            {
                return false;
            }

            if (GetCompanyStoresWeblink(driver, companyWebLink, keyWords, out companyStoresWebLink, out errorMessage) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the company general web page link
        /// </summary>
        /// <param name="driver">The chrome driver to do the search</param>
        /// <param name="companyName">The company name to search</param>
        /// <param name="companyWebLink">The company general web page link</param>
        /// <param name="errorMessage">The error message in case some error occurs</param>
        /// <returns></returns>
        private static bool GetCompanyWebLink(IWebDriver driver, string companyName, out string companyWebLink, out string errorMessage)
        {
            companyWebLink = string.Empty;

            if (Driver.GetAllGoogleSearchLinks(driver, companyName, out List<string> googleResultsList, out errorMessage) == false)
            {
                return false;
            }
            else
            {
                companyWebLink = googleResultsList.ElementAt(0);
                return true;
            }
        }

        /// <summary>
        /// Gets the company web page with stores details link
        /// </summary>
        /// <param name="driver">The driver to use in the search</param>
        /// <param name="companyWebLink">The company general web page already found</param>
        /// <param name="keyWords">the key words to help find the stores details link</param>
        /// <param name="companyStoresWebLink">The company web page with stores details link</param>
        /// <param name="errorMessage">The error message in case some error occurs</param>
        /// <returns></returns>
        private static bool GetCompanyStoresWeblink(IWebDriver driver, string companyWebLink, Dictionary<int, string> keyWords, out string companyStoresWebLink, out string errorMessage)
        {
            errorMessage = string.Empty;
            companyStoresWebLink= string.Empty;

            try
            {
                driver.Navigate().GoToUrl(companyWebLink);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            
            Dictionary<IWebElement, int> allPossibleCompanyStoresLinks = GetAllCompanyStoresLinks(driver, keyWords);
            List<string> bestCompanyStoresLinks = GetBestCompanyStoresLink(allPossibleCompanyStoresLinks, keyWords);

            if (bestCompanyStoresLinks.Count == 0)
            {
                errorMessage = Properties.ResourceCompanyGetStoreLink.ErrorMessageUnableToFindStoresLink;
                return false;
            }
            else 
            {
                companyStoresWebLink = bestCompanyStoresLinks.ElementAt(0);
                return true;
            }
        }

        /// <summary>
        /// Gets all the links related to store from the company general web page based on the inputed keywords
        /// </summary>
        /// <param name="driver">The google driver to use.</param>
        /// <param name="keyWords">The key words to help filter the page links</param>
        /// <returns></returns>
        private static Dictionary<IWebElement, int> GetAllCompanyStoresLinks(IWebDriver driver, Dictionary<int, string> keyWords)
        {
            Dictionary<IWebElement, int> usefulLinks = new Dictionary<IWebElement, int> { };

            System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> iWebElements = driver.FindElements(By.TagName("a"));

            if (iWebElements != null)
            {
                List<IWebElement> allLinks = iWebElements.ToList();
                int linkTextWeight;

                foreach (IWebElement link in allLinks)
                {
                    try
                    {
                        if (String.IsNullOrEmpty(link.Text))
                        {
                            continue;
                        }

                        if (link.Displayed && link.Enabled)
                        {
                            linkTextWeight = CorrectKeyWord(keyWords, link.Text);
                            if (linkTextWeight > 0)
                            {
                                usefulLinks.Add(link, linkTextWeight);
                            }
                        }
                    }
                    catch (StaleElementReferenceException)
                    {
                        continue;
                    }
                }
            }

            return usefulLinks;
        }

        /// <summary>
        /// Filters all the company links found related to stores based on the int value that defines the preference between all key words inputed
        /// </summary>
        /// <param name="storeLinks">All the links found before</param>
        /// <param name="keyWords">The key words inputed and its weights</param>
        /// <returns></returns>
        private static List<string> GetBestCompanyStoresLink(Dictionary<IWebElement, int> storeLinks, Dictionary<int, string> keyWords)
        {
            if (storeLinks.Count == 0) 
            {
                return null;
            }

            IOrderedEnumerable<KeyValuePair<IWebElement, int>> sortedStoreLinks = from entry in storeLinks orderby entry.Value ascending select entry;
            int lowestValue = sortedStoreLinks.ElementAt(0).Value; //the lowest value is equivalent to the first entry in the sorted dictionary
            List<string> bestsStoreLinks = new List<string>();
            
            foreach (KeyValuePair<IWebElement, int> storeLink in sortedStoreLinks)
            {
                if (storeLink.Value > lowestValue)
                {
                    break;
                }
                bestsStoreLinks.Add(storeLink.Key.GetAttribute("href"));
            }

            List<string> bestsStoreLinksBasedOnHref = new List<string>();
            
            foreach (string link in bestsStoreLinks)
            {
                if (CorrectKeyWord(keyWords, link) > 0)
                {
                    if (bestsStoreLinksBasedOnHref.Contains(link) == false)
                    {
                        bestsStoreLinksBasedOnHref.Add(link);
                    }
                }
            }

            return bestsStoreLinksBasedOnHref;
        }

        /// <summary>
        /// Checks if a word matches any of the key words. Example of desired match - word: lojas, keyWord: loja
        /// </summary>
        /// <param name="keyWords">All the key words inputed</param>
        /// <param name="word">The word to check the match</param>
        /// <returns></returns>
        private static int CorrectKeyWord(Dictionary<int, string> keyWords, String word) //TODO: Move to SisqualUtils
        {
            if (string.IsNullOrEmpty(word))
            {
                return -1;
            }

            word = word.ToLower(); // to be compared in lower case

            foreach (KeyValuePair<int, string> keyWord in keyWords)
            {
                if (word.Contains(keyWord.Value))
                {
                    return keyWord.Key;
                }
            }

            return -1;
        }

        #endregion
    }
}
