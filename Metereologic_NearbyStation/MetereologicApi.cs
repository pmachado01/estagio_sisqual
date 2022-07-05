using System;

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

using Newtonsoft.Json.Linq;

using CompanyGetStoreLink;

namespace Metereologic
{
    /// <summary>
    /// 
    /// </summary>
    public class MeteostatApi
    {
        /// <summary>
        /// Gets nearby metereologic station ids for a collection of stores
        /// </summary>
        /// <param name="stores">The colletion of stores to get the nearby metereologic station</param>
        /// <param name="apiKey">The meteostat api key to use</param>
        /// <param name="apiSingleStationRequestLimit">The maximum number or tries per station</param>
        /// <param name="apiSleepTimeSecondsBetweenFailure">The seconds to wait between each failed try</param>
        /// <param name="storesFilledWithStationId">The collection of stores filled with the metereologic station ids</param>
        /// <param name="stations">The stations discovered in the process</param>
        /// <param name="errorMessage">The error message in case some error has occured</param>
        /// <returns></returns>
        public static bool GetNearbyStations(StoreCollection stores, string apiKey, int apiSingleStationRequestLimit, int apiSleepTimeSecondsBetweenFailure, out StoreCollection storesFilledWithStationId, out HashSet<Station> stations, out string errorMessage)
        {
            stations = new HashSet<Station>();
            storesFilledWithStationId = new StoreCollection();
            errorMessage = string.Empty;

            foreach (Store store in stores)
            {
                string latitudeString = store.Location.Latitude.ToString().Replace(",", ".");
                string longitudeString = store.Location.Longitude.ToString().Replace(",", ".");

                Store newStore = store;
                if (GetNearbyStation(latitudeString, longitudeString, apiKey, apiSingleStationRequestLimit, apiSleepTimeSecondsBetweenFailure, out Station newStation, out errorMessage))
                {
                    stations.Add(newStation);
                    newStore.MetereologyStationID = newStation.Id;
                }

                storesFilledWithStationId.Add(store);
            }

            if (stations.Count == 0)
            {
                errorMessage = "No station was found";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets nearby metereologic station id for a single store
        /// </summary>
        /// <param name="locationLatitude">The store location latitude</param>
        /// <param name="locationLongitude">The store location longitude</param>
        /// <param name="apiKey">The meteostat api key to use</param>
        /// <param name="apiSingleStationRequestLimit">The maximum number or tries per station</param>
        /// <param name="apiSleepTimeSecondsBetweenFailure">The seconds to wait between each failed try</param>
        /// <param name="station">The station discovered in the process</param>
        /// <param name="errorMessage">The error message in case some error has occured</param>
        /// <returns></returns>
        private static bool GetNearbyStation(string locationLatitude, string locationLongitude, string apiKey, int apiSingleStationRequestLimit, int apiSleepTimeSecondsBetweenFailure, out Station station, out string errorMessage)
        {
            errorMessage = string.Empty;
            station = null;
            int triesCounter = 0;

            while (triesCounter < apiSingleStationRequestLimit)
            {
                triesCounter++;
                try
                {
                    station = ApiRequestNearbyStation(locationLatitude, locationLongitude, apiKey);
                    break;
                }
                catch (HttpRequestException)
                {   
                    Thread.Sleep(apiSleepTimeSecondsBetweenFailure * 1000);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return false;
                }
            }

            if (station == null)
            {
                errorMessage = "Unable to get information about the nearest station";
                return false;
            }

            return true;
        }

        /// <summary>
        /// The request to the api for the nearby metereologic station for a single store
        /// </summary>
        /// <param name="latitude">The store location latitude</param>
        /// <param name="longitude">The store location longitude</param>
        /// <param name="apiKey">The meteostat api key to use</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static Station ApiRequestNearbyStation(string latitude, string longitude, string apiKey)
        {
            if (string.IsNullOrEmpty(latitude))
            {
                throw new ArgumentException($"'{nameof(latitude)}' cannot be null or empty.", nameof(latitude));
            }

            if (string.IsNullOrEmpty(longitude))
            {
                throw new ArgumentException($"'{nameof(longitude)}' cannot be null or empty.", nameof(longitude));
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException($"'{nameof(apiKey)}' cannot be null or empty.", nameof(apiKey));
            }

            string id = string.Empty;
            string name = string.Empty;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-RapidAPI-Key", apiKey);
            string content = client.GetStringAsync($"https://meteostat.p.rapidapi.com/stations/nearby?lat={latitude}&lon={longitude}").Result;
            JObject json = JObject.Parse(content);

            if (json == null)
            {
                return null;
            }

            if (json["data"][0]["id"].Type != JTokenType.Null)
            {
                id = json["data"][0]["id"].ToString();
            }

            if (json["data"][0]["name"]["en"].Type != JTokenType.Null)
            {
                name = json["data"][0]["name"]["en"].ToString();
            }           

            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(id))
            {
                return null;
            }

            return new Station(id, name);
        }

        /// <summary>
        /// Gets metereologic data for some stations in the time period between the start and the end dates
        /// </summary>
        /// <param name="stations">The station to collect data from</param>
        /// <param name="start">The start date</param>
        /// <param name="end">the end date</param>
        /// <param name="apiKey">the metostat Api key to use</param>
        /// <param name="apiSingleStationRequestLimit">The maximum number or tries per station</param>
        /// <param name="apiSleepTimeSecondsBetweenFailure">The seconds to wait between each failed try</param>
        /// <param name="stationsWithData">The station filled with the collected data</param>
        /// <param name="errorMessage">The error message in case some error has occured</param>
        /// <returns></returns>
        public static bool GetMeteorologicDataForStations(HashSet<Station> stations, DateTime start, DateTime end, string apiKey, int apiSingleStationRequestLimit, int apiSleepTimeSecondsBetweenFailure, out List<Station> stationsWithData, out string errorMessage)
        {
            stationsWithData = new List<Station>();
            errorMessage = string.Empty;

            int numDays = DaysCounter(start, end);
            string fixedFormatDateFromMeteostatApi = "yyyy-MM-dd";
            string startDateString = start.ToString(fixedFormatDateFromMeteostatApi);
            string endDateString = end.ToString(fixedFormatDateFromMeteostatApi);

            foreach (Station station in stations)
            {
                if (GetMeteorologicDataForStation(station.Id, startDateString, endDateString, numDays, apiKey, apiSingleStationRequestLimit, apiSleepTimeSecondsBetweenFailure, out Dictionary<DateTime, Meteorology> meteorologicData))
                {
                    station.MeteorologicData = meteorologicData;
                }
                stationsWithData.Add(station);
            }

            return true;
        }

        /// <summary>
        /// Gets metereologic data for some station in the time period between the start and the end dates
        /// </summary>
        /// <param name="stationId">The station ID to collect data from</param>
        /// <param name="startDateString">The start date formated in a string</param>
        /// <param name="endDateString">The end date formated in a string</param>
        /// <param name="numDays">The number of days between the start and the end date</param>
        /// <param name="apiKey">The meteostat api key to use</param>
        /// <param name="apiSingleStationRequestLimit">The maximum number or tries per station</param>
        /// <param name="apiSleepTimeSecondsBetweenFailure">The seconds to wait between each failed try</param>
        /// <param name="meteorologicData">The metereologic data collected</param>
        /// <returns></returns>
        private static bool GetMeteorologicDataForStation(string stationId, string startDateString, string endDateString, int numDays, string apiKey, int apiSingleStationRequestLimit, int apiSleepTimeSecondsBetweenFailure, out Dictionary<DateTime, Meteorology> meteorologicData)
        {
            meteorologicData = new Dictionary<DateTime, Meteorology>();

            int triesCounter = 0;

            while (triesCounter < apiSingleStationRequestLimit)
            {
                try
                {
                    meteorologicData = GetDataMeteo(stationId, startDateString, endDateString, numDays, apiKey);
                    break;
                }
                catch (HttpRequestException)
                {
                    triesCounter++;
                    Thread.Sleep(apiSleepTimeSecondsBetweenFailure * 1000);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            if (meteorologicData.Count == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The request to the api to collect data from a station
        /// </summary>
        /// <param name="stationId">The station ID to collect the data from</param>
        /// <param name="startDateString">The start date formated in a string</param>
        /// <param name="endDateString">The end date formated in a string</param>
        /// <param name="numDays">The number of days between the start and the end date</param>
        /// <param name="apiKey">The meteostat api key to use</param>
        /// <returns></returns>
        private static Dictionary<DateTime, Meteorology> GetDataMeteo(string stationId, string startDateString, string endDateString, int numDays, string apiKey)
        {
            Dictionary<DateTime, Meteorology> meteorologicData = new Dictionary<DateTime, Meteorology>();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-RapidAPI-Key", apiKey);
            string content = client.GetStringAsync($"https://meteostat.p.rapidapi.com/stations/hourly?station={stationId}&start={startDateString}&end={endDateString}").Result;
            JObject json = JObject.Parse(content);

            DateTime startDate = DateTime.ParseExact(startDateString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            for (int dayIndex = 0; dayIndex < numDays; dayIndex++)
            {
                for (int hourIndex = 0; hourIndex < 24; hourIndex++)
                {
                    if (ParseApiResponseMeteorologicData(json, dayIndex, hourIndex, out Meteorology meteorology))
                    {
                        meteorologicData[startDate.AddDays(dayIndex).AddHours(hourIndex)] = meteorology;
                    }
                }
            }

            return meteorologicData;
        }

        /// <summary>
        /// The parse function to extract metereologic data from the api response
        /// </summary>
        /// <param name="json">The json wich contains the api response to the previous request made</param>
        /// <param name="dateIndex">The index of the date of this request</param>
        /// <param name="hourIndex">The index of the hour of this request</param>
        /// <param name="meteorologicData">The meteorologic data extracted</param>
        /// <returns></returns>
        private static bool ParseApiResponseMeteorologicData(JObject json, int dateIndex, int hourIndex, out Meteorology meteorologicData)
        {
            meteorologicData = new Meteorology();
            int jsonDataIndex = dateIndex * 24 + hourIndex;

            if (json["data"][jsonDataIndex]["time"].Type == JTokenType.Null)
            {
                return false;
            }

            if (json["data"][jsonDataIndex]["temp"].Type != JTokenType.Null)
            {
                meteorologicData.Temperature = float.Parse(json["data"][jsonDataIndex]["temp"].ToString());
            }

            if (json["data"][jsonDataIndex]["dwpt"].Type != JTokenType.Null)
            {
                meteorologicData.DewPoint = float.Parse(json["data"][jsonDataIndex]["dwpt"].ToString());
            }

            if (json["data"][jsonDataIndex]["rhum"].Type != JTokenType.Null)
            {
                meteorologicData.Humidity = float.Parse(json["data"][jsonDataIndex]["rhum"].ToString());
            }

            if (json["data"][jsonDataIndex]["prcp"].Type != JTokenType.Null)
            {
                meteorologicData.Precipitation = float.Parse(json["data"][jsonDataIndex]["prcp"].ToString());
            }

            if (json["data"][jsonDataIndex]["snow"].Type != JTokenType.Null)
            {
                meteorologicData.Snow = float.Parse(json["data"][jsonDataIndex]["snow"].ToString());
            }

            if (json["data"][jsonDataIndex]["wdir"].Type != JTokenType.Null)
            {
                meteorologicData.WindDirection = float.Parse(json["data"][jsonDataIndex]["wdir"].ToString());
            }

            if (json["data"][jsonDataIndex]["wspd"].Type != JTokenType.Null)
            {
                meteorologicData.WindSpeed = float.Parse(json["data"][jsonDataIndex]["wspd"].ToString());
            }

            if (json["data"][jsonDataIndex]["wpgt"].Type != JTokenType.Null)
            {
                meteorologicData.WindPeakGust = float.Parse(json["data"][jsonDataIndex]["wpgt"].ToString());
            }

            if (json["data"][jsonDataIndex]["pres"].Type != JTokenType.Null)
            {
                meteorologicData.Pressure = float.Parse(json["data"][jsonDataIndex]["pres"].ToString());
            }

            if (json["data"][jsonDataIndex]["tsun"].Type != JTokenType.Null)
            {
                meteorologicData.TotalSunshineTime = float.Parse(json["data"][jsonDataIndex]["tsun"].ToString());
            }

            return true;
        }

        /// <summary>
        /// The number of days between two dated
        /// </summary>
        /// <param name="start">The start date</param>
        /// <param name="end">The end date</param>
        /// <returns></returns>
        private static int DaysCounter(DateTime start, DateTime end)
        {
            return Convert.ToInt32((end - start).TotalDays) + 1;
        }
    }
}
