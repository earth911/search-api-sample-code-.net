using System;
using System.Configuration;
using Earth911.Utils;
using Jayrock.Json;

/// <summary>
/// Search result details page
/// </summary>
public partial class Details : System.Web.UI.Page
{
    /// <summary>
    /// Arguments to the current search
    /// </summary>
    protected SearchArgs searchArgs;

    /// <summary>
    /// Details of this search result
    /// </summary>
    protected JsonObject details;

    protected void Page_Load(object sender, EventArgs e)
    {
        string apiUrl = ConfigurationManager.AppSettings["Earth911.ApiUrl"];
        string apiKey = ConfigurationManager.AppSettings["Earth911.ApiKey"];
        Earth911.Api api = new Earth911.Api(apiUrl, apiKey);

        searchArgs = new SearchArgs(Request, api);

        string type = Request.QueryString["type"];
        string id = Request.QueryString["id"];
        
        JsonObject args = new JsonObject();
        string method = null;

        if (type == "location")
        {
            method = "earth911.getLocationDetails";
            args["location_id"] = id;
        }
        else if (type == "program")
        {
            method = "earth911.getProgramDetails";
            args["program_id"] = id;
        }
        
        details = null;
        
        if (method != null)
        {
            JsonObject result = (JsonObject)api.Call(method, args);
            details = (JsonObject)result[id];
        }

        if (details == null)
        {
            Response.StatusCode = 404;
        }
    }
}
