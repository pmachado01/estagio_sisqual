using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using CompanyGetStoreLink;
using System.Globalization;
using System;

namespace GetStoreInformation
{
    /// <summary>
    /// Dll to get all stores locations details for a company based on the previous postal code obtained
    /// </summary>
    public class GetStoreInformationClass
    {
        #region Methods

        /// <summary>
        /// Gets the location details for each store present in a store collection
        /// </summary>
        /// <param name="stores">The collection of stores to get locarion details from</param>
        /// <param name="zipCodeApiToken">Zipcode API token to use</param>
        /// <param name="storesFilled">The collection of stores with the details already filled</param>
        /// <returns></returns>
        public static void GetStoresLocation(StoreCollection stores, string zipCodeApiToken, out StoreCollection storesFilled)
        {
            storesFilled = new StoreCollection();
            
            foreach (Store store in stores)
            {
                if (FillStoreInformation(store.PostalCode, zipCodeApiToken, out Store storeFilledToAdd))
                {
                    storesFilled.Add(storeFilledToAdd);
                }
            }
        }

        /// <summary>
        /// Fils a store with details about its location
        /// </summary>
        /// <param name="postalCode">The store postal code</param>
        /// <param name="zipCodeApiToken">The ZipCode API token to use</param>
        /// <param name="storeFilledToAdd">The store with the details filled</param>
        /// <returns></returns>
        private static bool FillStoreInformation(string postalCode, string zipCodeApiToken, out Store storeFilledToAdd)
        {
            storeFilledToAdd = new Store(postalCode);
            
            HttpClient client = new HttpClient();
            
            string content = client.GetStringAsync($"https://app.zipcodebase.com/api/v1/search?apikey={zipCodeApiToken}&codes={postalCode}").Result;
            
            JObject json = JObject.Parse(content);
            

            if (json == null || json["query"] == null || json["query"].Count() == 0)
            {
                return false;
            }

            try
            {
                if (json["results"][postalCode][0]["latitude"].Type != JTokenType.Null && json["results"][postalCode][0]["longitude"].Type != JTokenType.Null)
                {
                    string latitude = json["results"][postalCode][0]["latitude"].ToString();
                    string longitude = json["results"][postalCode][0]["longitude"].ToString();

                    storeFilledToAdd.Location = new Location(float.Parse(latitude, CultureInfo.InvariantCulture.NumberFormat), float.Parse(longitude, CultureInfo.InvariantCulture.NumberFormat));
                }

                if (json["results"][postalCode][0]["city"].Type != JTokenType.Null)
                {
                    storeFilledToAdd.City = json["results"][postalCode][0]["city"].ToString();
                }

                if (json["results"][postalCode][0]["country_code"].Type != JTokenType.Null)
                {
                    storeFilledToAdd.Country = json["results"][postalCode][0]["country_code"].ToString();
                }

                if (json["results"][postalCode][0]["state"].Type != JTokenType.Null)
                {
                    storeFilledToAdd.State = json["results"][postalCode][0]["state"].ToString();
                }

                if (json["results"][postalCode][0]["province"].Type != JTokenType.Null)
                {
                    storeFilledToAdd.Province = json["results"][postalCode][0]["province"].ToString();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
