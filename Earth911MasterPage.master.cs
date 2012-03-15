using System;
using System.Configuration;
using Earth911.Utils;

/// <summary>
/// Master layout for Earth911 Search Syndication pages
/// Feel free to rename this or replace it with your own master page.
/// Just keep in mind that the search form is part of this template.
/// </summary>
public partial class Earth911MasterPage : System.Web.UI.MasterPage
{
    protected SearchArgs searchArgs;

    protected void Page_Load(object sender, EventArgs e)
    {
        string apiUrl = ConfigurationManager.AppSettings["Earth911.ApiUrl"];
        string apiKey = ConfigurationManager.AppSettings["Earth911.ApiKey"];
        Earth911.Api api = new Earth911.Api(apiUrl, apiKey);
        searchArgs = new SearchArgs(Request, api);
    }
}
