using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Forms;

using CompanyGetStoreLink;
using PostalCodeScraping;
using GetStoreInformation;
using Metereologic;


namespace CompanyStoresLocationFinder
{
    public partial class Form1 : Form
    {
        #region Properties

        private static readonly string zipCodeApiKey = "68ec08c0-dda0-11ec-a5b8-37e45cef2e25";

        private static readonly string meteoStatApiKey = "44c4e8aa61mshc176acd39516672p1c4ac4jsnda3b61382253";

        private static readonly Dictionary<string, Regex> postalCodesTemplates = new Dictionary<string, Regex>() {
            {"pt", new Regex("[1-9][0-9]{3}-[0-9]{3}")},
            {"en", new Regex("[0-9]{4}-[0-9]{3}")},
        };

        private static readonly Dictionary<int, string> keyWordsWeightPT = new Dictionary<int, string>() {
            {1, "loj"},
            {2, "contact"},
        };

        private static Dictionary<int, string> keyWordsWeightEN = new Dictionary<int, string>() {
            {1, "shop"},
            {2, "contact"},
        };

        private static readonly Dictionary<string, Dictionary<int, string>> keyWordsWeight = new Dictionary<string, Dictionary<int, string>>() {
            {"pt", keyWordsWeightPT},
            {"en", keyWordsWeightEN},
        };

        private static readonly string chromeDriver = "C:\\Users\\Pedro\\MEOCloud";

        private DateTime startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        private DateTime endDate = DateTime.Today.AddDays(-1);
        private static readonly int meteoStatApiSingleStationRequestLimit = 5;
        private static readonly int meteoStatApiSleepTimeSecondsBetweenFailure = 2;

        private bool locations = false;
        private bool metereologic = false;
        private bool campaigns = false;

        #endregion

        public Form1()
        {
            InitializeComponent();
            dateTimePicker1.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dateTimePicker2.Value = DateTime.Today.AddDays(-1);
            dateTimePicker2.MaxDate = DateTime.Today.AddDays(-1);
        }

        public void ShowOutputLocations(string text) 
        {
            outputLocations.Text += text;
        }

        public void ShowOutputMetereologic(string text)
        {
            outputMetereologic.Text += text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            outputLocations.Text = string.Empty;
            outputMetereologic.Text = string.Empty;

            List<Company> companiesList = new List<Company>();
            List<Station> stationsList = new List<Station>();
            List<string> companyNames = companyNamesList.Text.Split(';').ToList();

            companyNamesList.Text = string.Empty;

            if (locations == false)
            {
                return;
            }

            foreach (string companyName in companyNames)
            {
                PostalCodeScrapingClass.GetStoresPostalCodes(companyName, chromeDriver, keyWordsWeightPT, postalCodesTemplates["pt"], out Company companyToAdd, out string errorMessage);

                GetStoreInformationClass.GetStoresLocation(companyToAdd.Stores, zipCodeApiKey, out StoreCollection stores);

                if (metereologic)
                {
                    HashSet<Station> stations = new HashSet<Station>();
                    MeteostatApi.GetNearbyStations(stores, meteoStatApiKey, meteoStatApiSingleStationRequestLimit, meteoStatApiSleepTimeSecondsBetweenFailure, out StoreCollection storesFilledWithMetereologicId, out stations, out errorMessage);
                    
                    MeteostatApi.GetMeteorologicDataForStations(stations, startDate, endDate, meteoStatApiKey, meteoStatApiSingleStationRequestLimit, meteoStatApiSleepTimeSecondsBetweenFailure, out stationsList, out errorMessage);

                    companyToAdd.Stores = storesFilledWithMetereologicId;
                    companiesList.Add(companyToAdd);
                }
                else
                {
                    companyToAdd.Stores = stores;
                    companiesList.Add(companyToAdd);
                }
            }

            ShowOutputLocations("Total number of companies: " + companiesList.Count + "\r\n\r\n");
            foreach (Company company in companiesList)
            {
                ShowOutputLocations(company.ToString());
                ShowOutputLocations("-------------------------------------\r\n");
            }

            ShowOutputMetereologic("Total number of stations: " + stationsList.Count + "\r\n\r\n");
            foreach (Station station in stationsList)
            {
                ShowOutputMetereologic(station.ToString());
                ShowOutputMetereologic("-------------------------------------\r\n");
            }
        }

        private void StartDate_ValueChanged(object sender, EventArgs e)
        {
            startDate = dateTimePicker1.Value;
        }

        private void EndDate_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePicker2.Value < dateTimePicker1.Value)
            {
                dateTimePicker2.Value = dateTimePicker2.Value.AddDays(1);
            }
            endDate = dateTimePicker2.Value;
        }

        private void LocationsCheckedChanged(object sender, EventArgs e)
        {
            locations = checkBox1.Checked;
        }

        private void MetereologicCheckedChanged(object sender, EventArgs e)
        {
            metereologic = checkBox2.Checked;
        }

        private void CampaignsCheckedChanged(object sender, EventArgs e)
        {
            campaigns = checkBox2.Checked;
        }

        private void closeFormButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}
