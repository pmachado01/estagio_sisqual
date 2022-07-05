using System.Diagnostics;

namespace CompanyGetStoreLink
{
    /// <summary>
    /// Company informations such as name, general web page link, stores details link and stores colletion
    /// </summary>
    [DebuggerDisplay("{DebugMessage}")]
    public class Company
    {
        
        #region Properties

        private string companyName;
        private string companyWebLink;
        private string companyStoresWebLink;
        private StoreCollection stores;

        public string CompanyName
        {
            get { return companyName; }
            set { companyName = value; }
        }

        public string CompanyWebLink
        {
            get { return companyWebLink; }
            set { companyWebLink = value; }
        }

        public string CompanyStoresWebLink
        {
            get { return companyStoresWebLink; }
            set { companyStoresWebLink = value; }
        }

        public StoreCollection Stores
        {
            get { return stores; }
            set { stores = value; }
        }

        private string DebugMessage {
            get 
            {
                string output = string.Empty;

                output += "Company name: " + this.companyName + "\r\n";
                output += "Company web link: " + this.companyWebLink + "\r\n";
                output += "Stores information link: " + this.companyStoresWebLink + "\r\n";
                output += "Total number of stores: " + this.stores.Count + "\r\n\r\n";

                foreach (Store store in stores)
                {
                    output += store.ToString() + "\r\n";
                }

                return output;
            }
        }

        #endregion

        #region Constructors

        public Company()
        {
            companyName = string.Empty;
            companyWebLink = string.Empty;
            companyStoresWebLink = string.Empty;
            stores = new StoreCollection();
        }
        
        public Company(string companyName)
            : this()
        {
            this.companyName = companyName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Puts all the information in a string ready to be printed
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string output = string.Empty;

            output += "Company name: " + this.companyName + "\r\n";
            output += "Company web link: " + this.companyWebLink + "\r\n";
            output += "Stores information link: " + this.companyStoresWebLink + "\r\n";
            output += "Total number of stores: " + this.stores.Count + "\r\n\r\n";

            foreach (Store store in stores)
            {
                output += store.ToString() + "\r\n";
            }

            return output;
        }

        #endregion
    }
}
