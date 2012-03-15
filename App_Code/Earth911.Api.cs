using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace Earth911
{
    /// <summary>
    /// Earth911 Search Syndication API wrapper
    /// This class allows API calls to be made using JSON objects. It handles
    /// parameter passing, serialization, and HTTP request building.
    /// </summary>
    public class Api
    {
        private string apiUrl;
        private string apiKey;

        /// <summary>
        /// Constructor, takes a base URL and an API key
        /// </summary>
        /// <param name="apiUrl">Base URL for API (eg. http://api.earth911.com/ )</param>
        /// <param name="apiKey">API key provided by Earth911</param>
        public Api(string apiUrl, string apiKey)
        {
            this.apiUrl = apiUrl;
            this.apiKey = apiKey;
        }

        /// <summary>
        /// Makes an API call given a method name and JSON argument object
        /// </summary>
        /// <param name="method">Method name (eg. earth911.searchLocations)</param>
        /// <param name="args">JSON object containing method arguments</param>
        /// <returns>JSON result (typically a JsonObject or JsonArray)</returns>
        /// <exception cref="ApiError">Raised if an API error occurred</exception>
        public object Call(string method, JsonObject args)
        {
            args.Put("api_key", this.apiKey);
            string url = this.apiUrl + method + '?' + BuildQuery(args);
            string text = GetURL(url);
            JsonObject result = (JsonObject)JsonConvert.Import(text);

            if (result["error"] != null)
            {
                throw new ApiError((string)result["error"]);
            }
            else
            {
                return result["result"];
            }
        }

        private static string GetURL(string url)
        {
            Stream stream = new WebClient().OpenRead(url);
            return new StreamReader(stream).ReadToEnd();
        }

        private string BuildQuery(JsonObject values)
        {
            List<string[]> pairs = new List<String[]>();
            foreach (string k in values.Names)
            {
                AddValues(pairs, new List<string>(), k, values[k]);
            }
            StringBuilder result = new StringBuilder();
            bool first = true;
            foreach (string[] pair in pairs)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    result.Append('&');
                }
                result.Append(HttpUtility.UrlEncode(pair[0]));
                result.Append('=');
                result.Append(HttpUtility.UrlEncode(pair[1]));
            }
            return result.ToString();
        }

        public static string BuildName(List<string> keys)
        {
            if (keys.Count == 0)
            {
                return "";
            }
            else if (keys.Count == 1)
            {
                return keys[0];
            }
            else
            {
                StringBuilder result = new StringBuilder();
                bool first = true;
                foreach (string key in keys)
                {
                    if (first)
                    {
                        first = false;
                        result.Append(key);
                    }
                    else
                    {
                        result.Append('[');
                        result.Append(key);
                        result.Append(']');
                    }
                }
                return result.ToString();
            }
        }

        private static void AddValues(List<string[]> pairs, List<string> keys, string key, object value)
        {
            keys = new List<string>(keys);
            keys.Add(key);
            if (value == null)
            {
                // pass
            }
            else if (value is JsonObject)
            {
                JsonObject o = (JsonObject)value;
                foreach (string k in o.Names)
                {
                    AddValues(pairs, keys, k, o[k]);
                }
            }
            else if (value is JsonArray)
            {
                JsonArray a = (JsonArray)value;
                for (int i = 0; i < a.Count; i++)
                {
                    AddValues(pairs, keys, i.ToString(), a[i]);
                }
            }
            else
            {
                pairs.Add(new string[] { BuildName(keys), value.ToString() });
            }
        }
    }

    /// <summary>
    /// Exception class for API errors
    /// Any error reported by the API server will be raised as an instance
    /// of this exception class.
    /// </summary>
    public class ApiError : Exception
    {
        public ApiError(string message) : base(message) { }
    }
}
