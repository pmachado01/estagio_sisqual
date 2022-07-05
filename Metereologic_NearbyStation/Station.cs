using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Metereologic
{
    /// <summary>
    /// Meterologic Station details such as its id and name, and metereologicData collected for the station
    /// </summary>
    [DebuggerDisplay("{DebugMessage}")]
    public class Station
    {

        #region Properties

        private string id;
        private string name;
        private Dictionary<DateTime, Meteorology> meteorologicData = new Dictionary<DateTime, Meteorology>();

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Dictionary<DateTime, Meteorology> MeteorologicData
        {
            get { return meteorologicData; }
            set { meteorologicData = value; }
        }

        private string DebugMessage
        {
            get
            {
                string result = string.Empty;

                result += "Name: " + name + "\r\n";
                result += "ID: " + id + "\r\n";
                result += "Total metereologic records: " + meteorologicData.Count + "\r\n\r\n";

                foreach (KeyValuePair<DateTime, Meteorology> meteorology in meteorologicData)
                {
                    result += "DateTime: " + meteorology.Key.ToString("yyyy-MM-dd HH:mm") + "\r\n";
                    result += meteorology.Value.ToString() + "\r\n";
                }

                return result;
            }
        }

        #endregion

        #region Constructors

                public Station()
        {
            this.id = string.Empty;
            this.name = string.Empty;
        }

        public Station(string id, string name) 
            : this()
        {
            this.id = id;
            this.name = name;
        }

        #endregion


        #region Methods

        public override string ToString()
        {
            string result = string.Empty;

            result += "Name: " + name + "\r\n";
            result += "ID: " + id + "\r\n";
            result += "Total metereologic records: " + meteorologicData.Count + "\r\n\r\n";

            foreach (KeyValuePair<DateTime, Meteorology> meteorology in meteorologicData)
            {
                result += "DateTime: " + meteorology.Key.ToString("yyyy-MM-dd HH:mm") + "\r\n";
                result += meteorology.Value.ToString() + "\r\n";
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            return Id.Equals(((Station)obj).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}
