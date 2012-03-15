using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Earth911.Utils;
using Jayrock.Json;

/// <summary>
/// Search results page
/// </summary>
public partial class Search : System.Web.UI.Page
{
    /// <summary>
    /// Arguments to the current search
    /// </summary>
    protected SearchArgs searchArgs;

    /// <summary>
    /// Results from the current search
    /// </summary>
    protected List<JsonObject> results;

    /// <summary>
    /// Location details for this page of search results
    /// </summary>
    protected JsonObject locationDetails;

    /// <summary>
    /// Program details for this page of search results
    /// </summary>
    protected JsonObject programDetails;

    /// <summary>
    /// Pager for the current search
    /// </summary>
    protected SearchPager<JsonObject> searchPager;

    /// <summary>
    /// Base URL for search results pagination
    /// </summary>
    protected string baseUrl;

    protected void Page_Load(object sender, EventArgs e)
    {
        string apiUrl = ConfigurationManager.AppSettings["Earth911.ApiUrl"];
        string apiKey = ConfigurationManager.AppSettings["Earth911.ApiKey"];
        Earth911.Api api = new Earth911.Api(apiUrl, apiKey);

        searchArgs = new SearchArgs(Request, api);
        baseUrl = "Search.aspx?" + searchArgs.QueryString;

        JsonArray locations = new JsonArray();
        JsonArray programs = new JsonArray();

        JsonObject args;

        // Perform search queies

        if (searchArgs.What != "" && searchArgs.FoundWhere)
        {
            // Find matching materials

            args = new JsonObject();
            args["query"] = searchArgs.What;
            JsonArray materials = (JsonArray)api.Call("earth911.searchMaterials", args);

            JsonArray materialIds = new JsonArray();
            foreach (JsonObject material in materials)
            {
                materialIds.Add((JsonNumber)material["material_id"]);
            }
            
            // If materials were found, run the query

            if (materialIds.Count > 0)
            {
                args = new JsonObject();
                args["latitude"] = searchArgs.Latitude;
                args["longitude"] = searchArgs.Longitude;
                args["material_id"] = materialIds;
                locations = (JsonArray)api.Call("earth911.searchLocations", args);
                programs = (JsonArray)api.Call("earth911.searchPrograms", args);
            }
        }

        // Combine locations and programs, sort by distance

        results = new List<JsonObject>();
        foreach (JsonObject location in locations)
        {
            // Filtering of undesirable locations can be done here,
            // prior to pagination.
            //
            // if (location["description"].ToString() == "Company X")
            // {
            //     continue;
            // }

            location["type"] = "location";
            location["id"] = location["location_id"];
            results.Add(location);
        }
        foreach (JsonObject program in programs)
        {
            program["type"] = "program";
            program["id"] = program["program_id"];
            results.Add(program);
        }
        results.Sort(new DistanceComparer());

        // Paginate results

        int page = Convert.ToInt32(Request.QueryString["page"]);
        searchPager = new SearchPager<JsonObject>(results, page);
        results = searchPager.Result();

        // Load details for this page of results

        JsonArray locationIds = new JsonArray();
        JsonArray programIds = new JsonArray();

        foreach (JsonObject result in results)
        {
            if (result["type"] == "location")
            {
                locationIds.Add(result["id"]);
            }
            if (result["type"] == "program")
            {
                programIds.Add(result["id"]);
            }
            result["url"] =
                "Details.aspx?type="
                + result["type"]
                + "&id="
                + result["id"]
                + "&" + searchArgs.QueryString;
        }

        locationDetails = new JsonObject();
        if (locationIds.Count > 0)
        {
            args = new JsonObject();
            args["location_id"] = locationIds;
            locationDetails = (JsonObject)api.Call("earth911.getLocationDetails", args);
        }

        programDetails = new JsonObject();
        if (programIds.Count > 0)
        {
            args = new JsonObject();
            args["program_id"] = programIds;
            programDetails = (JsonObject)api.Call("earth911.getProgramDetails", args);
        }
    }
}
