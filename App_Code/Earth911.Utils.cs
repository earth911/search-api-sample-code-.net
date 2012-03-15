using System;
using System.Collections.Generic;
using System.Web;
using Jayrock.Json;

namespace Earth911.Utils
{
    /// <summary>
    /// Structure containing search arguments
    /// These arguments are parsed from the request's QueryString and
    /// are assembled here to make it easier to pass them around and
    /// rebuild query strings for links.
    /// </summary>
    public struct SearchArgs
    {
        public string What;
        public string Where;
        public JsonNumber Latitude;
        public JsonNumber Longitude;
        public bool FoundWhere;

        /// <summary>
        /// Constructor, uses the Earth911 API to look up zip codes
        /// </summary>
        /// <param name="request">The current page's request</param>
        /// <param name="api">An instance of the API wrapper class</param>
        public SearchArgs(HttpRequest request, Api api)
        {
            What = request.QueryString["what"];
            if (What == null) What = "";

            Where = request.QueryString["where"];
            if (Where == null) Where = "";

            FoundWhere = false;

            if (Where != "")
            {
                JsonObject args = new JsonObject();
                args["postal_code"] = Where;
                args["country"] = "US";
                
                JsonObject postal = null;
                try
                {
                    postal = (JsonObject)api.Call("earth911.getPostalData", args);
                    FoundWhere = true;
                }
                catch (Earth911.ApiError) { }

                if (postal != null)
                {
                    Latitude = (JsonNumber)postal["latitude"];
                    Longitude = (JsonNumber)postal["longitude"];
                }
            }
        }

        /// <summary>
        /// Builds a query string containing these search arguments
        /// </summary>
        public string QueryString
        {
            get
            {
                return "what=" + HttpUtility.UrlEncode(What)
                    + "&where=" + HttpUtility.UrlEncode(Where);
            }
        }
    }

    /// <summary>
    /// Utility class to aid in pagination of results
    /// </summary>
    /// <typeparam name="T">A type for each result to be paginated</typeparam>
    public class SearchPager<T>
    {
        private List<T> list;
        private int size;
        private int page;

        /// <summary>
        /// Constructor, takes a list of results, page number, and page size
        /// </summary>
        /// <param name="list">List of results</param>
        /// <param name="page">Page number (starting with 1)</param>
        /// <param name="size">Page size (number of results per page)</param>
        public SearchPager(List<T> list, int page, int size)
        {
            this.list = list;
            this.size = size;
            this.page = this.Bound(page);
        }

        public SearchPager(List<T> list, int page) : this(list, page, 10) { }
        public SearchPager(List<T> list) : this(list, 1) { }

        private int Bound(int page)
        {
            return Math.Max(1, Math.Min(page, this.Total));
        }

        /// <summary>
        /// The current page
        /// </summary>
        public int Page
        {
            get
            {
                return this.page;
            }
        }

        /// <summary>
        /// The total number of pages
        /// </summary>
        public int Total
        {
            get
            {
                return (int)Math.Floor((this.list.Count - 1.0) / this.size) + 1;
            }
        }

        /// <summary>
        /// Returns list of results after pagination
        /// </summary>
        /// <param name="page">Page number</param>
        /// <returns>A list of results after pagination is applied</returns>
        public List<T> Result(int page)
        {
            if (page < 1) page = 1;
            int offset = (page - 1) * this.size;
            int length = this.size;
            if (offset >= this.list.Count) return new List<T>();
            if (offset + length >= this.list.Count) length = this.list.Count - offset;
            return this.list.GetRange(offset, length);
        }

        /// <summary>
        /// Returns list of results for current page
        /// </summary>
        /// <returns></returns>
        public List<T> Result()
        {
            return this.Result(this.page);
        }

        /// <summary>
        /// Returns a structure that can be used to navigate between pages
        /// </summary>
        /// <param name="page">Page number</param>
        /// <returns>A SearchPagerNav structure</returns>
        public SearchPagerNav Nav(int page)
        {
            int total = this.Total;
            int prev = Math.Max(1, page - 1);
            int prevCount = this.Result(prev).Count;
            int next = Math.Min(page + 1, total);
            int nextCount = this.Result(next).Count;
            return new SearchPagerNav(page, total, prev, prevCount, next, nextCount);
        }

        /// <summary>
        /// Returns a navigation structure for the current page
        /// </summary>
        /// <returns>A SearchPagerNav structure</returns>
        public SearchPagerNav Nav()
        {
            return this.Nav(this.page);
        }

        /// <summary>
        /// Returns a structure containing lists of nearby page numbers
        /// </summary>
        /// <param name="width">The number of result items in the window</param>
        /// <returns>A SearchPagerWindow structure</returns>
        public SearchPagerWindow Window(int width)
        {
            int page = this.page;
            int total = this.Total;

            List<int> before = new List<int>();
            List<int> after = new List<int>();

            for (int i = 1; i <= total; i++)
            {
                if ((page - i) < (width / 2) && (i - page) < (width / 2))
                {
                    if (i < page)
                    {
                        before.Add(i);
                    }
                    else if (i > page)
                    {
                        after.Add(i);
                    }
                }
            }

            return new SearchPagerWindow(before, after);
        }

        /// <summary>
        /// Returns a page number window structure for a default window size of 20
        /// </summary>
        /// <returns>A SearchPagerWindow structure</returns>
        public SearchPagerWindow Window()
        {
            return this.Window(20);
        }
    }

    /// <summary>
    /// Search pager navigation structure
    /// </summary>
    public struct SearchPagerNav
    {
        /// <summary>
        /// The current page
        /// </summary>
        public int Page;

        /// <summary>
        /// The total number of pages
        /// </summary>
        public int Total;

        /// <summary>
        /// The page number of the previous page
        /// </summary>
        public int Prev;

        /// <summary>
        /// The number of items in the previous page
        /// </summary>
        public int PrevCount;

        /// <summary>
        /// The page number of the next page
        /// </summary>
        public int Next;

        /// <summary>
        /// The number of items in the next page
        /// </summary>
        public int NextCount;

        public SearchPagerNav(int page, int total, int prev, int prevCount, int next, int nextCount)
        {
            this.Page = page;
            this.Total = total;
            this.Prev = prev;
            this.PrevCount = prevCount;
            this.Next = next;
            this.NextCount = nextCount;
        }
    }

    /// <summary>
    /// A pager window structure
    /// </summary>
    public struct SearchPagerWindow
    {
        /// <summary>
        /// A list of page numbers that appear before the current page
        /// </summary>
        public List<int> Before;

        /// <summary>
        /// A list of page numbers that appear after the current page
        /// </summary>
        public List<int> After;

        public SearchPagerWindow(List<int> before, List<int> after)
        {
            this.Before = before;
            this.After = after;
        }
    }

    /// <summary>
    /// A comparison utility that sorts search results by distance, descending
    /// </summary>
    public class DistanceComparer : IComparer<JsonObject>
    {
        public int Compare(JsonObject a, JsonObject b)
        {
            JsonNumber d1 = (JsonNumber)a["distance"];
            JsonNumber d2 = (JsonNumber)b["distance"];
            return (int)(d1.ToDouble() - d2.ToDouble());
        }
    }
}
