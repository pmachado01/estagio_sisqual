﻿
using System.Diagnostics;

namespace CompanyGetStoreLink
{
    /// <summary>
    /// Location details such as longitude and latitude
    /// </summary>
    [DebuggerDisplay("{DebugMessage}")]
    public class Location
    {
        #region Properties
        
        private float latitude;
        private float longitude;

        public float Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }
        public float Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }

        private string DebugMessage
        {
            get 
            {
                string output = "Latitude: " + latitude + "\r\n" + "Longitude: " + longitude;
                return output;
            }
        }

        #endregion

        #region Constructors

        public Location()
        {
            latitude = 0;
            longitude = 0;
        }

        public Location(float latitude, float longitude)
            : this()
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        #endregion


        #region Methods

        /// <summary>
        /// Puts all the information in a string ready to be printed
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string output = "Latitude: " + latitude + "\r\n" + "Longitude: " + longitude;
            return output;
        }

        #endregion
    }
}
