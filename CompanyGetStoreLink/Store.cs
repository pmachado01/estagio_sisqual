using System.Diagnostics;

namespace CompanyGetStoreLink
{
    /// <summary>
    /// Store details such as details about its location and nearest metereologic station
    /// </summary>
    [DebuggerDisplay("{DebugMessage}")]
    public class Store
    {

        #region Proprerties

        private string postalCode;
        private Location location;
        private string city;
        private string country;
        private string state;
        private string province;
        private string metereologyStationID;

        public string PostalCode
        {
            get { return postalCode; }
            set { postalCode = value; }
        }

        public Location Location
        {
            get { return location; }
            set { location = value; }
        }

        public string City
        {
            get { return city; }
            set { city = value; }
        }

        public string Country
        {
            get { return country; }
            set { country = value; }
        }

        public string State
        {
            get { return state; }
            set { state = value; }
        }

        public string Province
        {
            get { return province; }
            set { province = value; }
        }

        public string MetereologyStationID
        {
            get { return metereologyStationID; }
            set { metereologyStationID = value; }
        }

        private string DebugMessage
        {
            get
            {
                string output = string.Empty;

                output += "Postal Code: " + this.postalCode + "\r\n";
                output += location.ToString() + "\r\n";
                output += "city: " + city + "\r\n";
                output += "Country: " + country + "\r\n";
                output += "State: " + state + "\r\n";
                output += "Province: " + province + "\r\n";
                output += "Metereologic Station Id: " + metereologyStationID + "\r\n";

                return output;
            }
        }

        #endregion

        #region Constructors

        public Store()
        {
            postalCode = string.Empty;
            location = new Location();
            city = string.Empty;
            country = string.Empty;
            state = string.Empty;
            province = string.Empty;
            metereologyStationID = string.Empty;
        }
        public Store(string postalCode) :
            this()
        {
            this.postalCode = postalCode;
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

            output += "Postal Code: " + this.postalCode + "\r\n";
            output += location.ToString() + "\r\n";
            output += "city: " + city + "\r\n";
            output += "Country: " + country + "\r\n";
            output += "State: " + state + "\r\n";
            output += "Province: " + province + "\r\n";
            output += "Metereologic Station Id: " + metereologyStationID + "\r\n";

            return output;
        }

        #endregion
    }
}
