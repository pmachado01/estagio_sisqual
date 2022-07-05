using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenQA.Selenium;

using GoogleChromeDriver;
using CompanyGetStoreLink;

namespace PostalCodeScraping
{
    /// <summary>
    /// Dll to extract/scrape all stores postal codes from the company web page with details about stores location
    /// </summary>
    public class PostalCodeScrapingClass
    {
        #region Methods

        /// <summary>
        /// Gets all stores postal code for some company name
        /// </summary>
        /// <param name="companyName">The company name</param>
        /// <param name="chromeDriverPath">The chrome driver path</param>
        /// <param name="storeLinkKeyWords">The store link key words</param>
        /// <param name="postalCodeTemplate">The template for the postal code</param>
        /// <param name="company">The company with the collection of store collected</param>
        /// <param name="errorMessage">The error message in case some error has occur</param>
        /// <returns></returns>
        public static bool GetStoresPostalCodes(string companyName, string chromeDriverPath, Dictionary<int, string> storeLinkKeyWords, Regex postalCodeTemplate, out Company company, out string errorMessage)
        {
            company = new Company(companyName);

            if (string.IsNullOrEmpty(companyName) || string.IsNullOrEmpty(chromeDriverPath) || postalCodeTemplate==null || storeLinkKeyWords==null || storeLinkKeyWords.Count==0)
            {
                errorMessage = Properties.ResourcesPostalCodeScrapping.ErrorMessageInvalidParameters;
                return false;
            }

            IWebDriver driver;
            try
            {
                driver = Driver.InitializeDriver(chromeDriverPath, false);
            }
            catch (Exception ex) {
                errorMessage = ex.Message;
                return false;
            }

            try
            {
                string companyWebLink = string.Empty;
                string companyStoresWebLink = string.Empty;

                if (CompanyGetStoreLinkClass.GetCompanyWebLinks(driver, companyName, storeLinkKeyWords, out companyWebLink, out companyStoresWebLink, out errorMessage) == false)
                {
                    return false;
                }

                company.CompanyWebLink = companyWebLink;
                company.CompanyStoresWebLink = companyStoresWebLink;

                if (GetStores(driver, companyStoresWebLink, postalCodeTemplate, out StoreCollection stores, out errorMessage) == false)
                {
                    Driver.CloseDriver(driver);
                    return false;
                }

                company.Stores = stores;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Driver.CloseDriver(driver);
                return false;
            }

            Driver.CloseDriver(driver);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="companyStoresWebLink"></param>
        /// <param name="postalCodeRegex"></param>
        /// <param name="stores"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private static bool GetStores(IWebDriver driver, string companyStoresWebLink, Regex postalCodeRegex, out StoreCollection stores, out string errorMessage)
        {
            stores = new StoreCollection();

            if (GetPostalCodes(driver, companyStoresWebLink, postalCodeRegex, out List<string> zipCodes, out errorMessage) == false)
            {
                return false;
            }

            foreach (string zipCode in zipCodes)
            {
                stores.Add(new Store(zipCode));
            }

            if (stores.Count == 0)
            {
                errorMessage = Properties.ResourcesPostalCodeScrapping.ErrorMessageNoStoreFound;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="companyStoresWebLink"></param>
        /// <param name="postalCodeRegex"></param>
        /// <param name="zipCodes"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private static bool GetPostalCodes(IWebDriver driver, string companyStoresWebLink, Regex postalCodeRegex, out List<string> zipCodes, out string errorMessage)
        {
            errorMessage = string.Empty;
            zipCodes = new List<string>();

            try
            {
                driver.Navigate().GoToUrl(companyStoresWebLink);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            string fullCompanyStoreLinkSource = driver.PageSource;

            foreach (Match match in postalCodeRegex.Matches(fullCompanyStoreLinkSource))
            {
                if (zipCodes.Contains(match.Value) == false)
                {
                    zipCodes.Add(match.Value);
                }
            }

            return true;
        }

        #endregion
    }
}
