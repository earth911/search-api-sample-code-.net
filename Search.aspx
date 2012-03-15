<%@ Page Title="" Language="C#" MasterPageFile="~/Earth911MasterPage.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="Search" %>
<%@ Import Namespace="Jayrock.Json" %>
<%@ Import Namespace="Earth911.Utils" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    <% if (searchArgs.What != "" && searchArgs.Where != "") { %>
        <%= Server.HtmlEncode(searchArgs.What)%> near
        <%= Server.HtmlEncode(searchArgs.Where)%> -
    <% } %>
    Recycling Center Search
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <% if (!searchArgs.FoundWhere) { %>
        <br />
        <h1>ZIP Code not found</h1>
        <p>We could not find the ZIP Code you entered. Please try again.</p>
    <% } %>
    <div class="search-results">
        <ul>
            <% int row = 0; %>
            <% foreach (JsonObject result in results) { %>
                <% 
                   JsonObject details = new JsonObject();
                   if (result["type"] == "location" && locationDetails[result["id"].ToString()] != null)
                   {
                       details = (JsonObject)locationDetails[result["id"].ToString()];
                   }
                   if (result["type"] == "program" && programDetails[result["id"].ToString()] != null)
                   {
                       details = (JsonObject)programDetails[result["id"].ToString()];
                   }
                   row++;
                   string classes = (row % 2 == 0) ? "even" : "odd";
                   if (row == 1) classes += " first";
                %>
                <li class="<%= classes %>">
                    <img class="icon" src="images/map-icon.png" alt="" width="22" height="29" />
                    <div class="materials">
                        <% int num = 0; %>
                        <% if (details["materials"] != null) { %>
                            <% foreach (JsonObject material in (JsonArray)details["materials"]) { %>
                                <% num++; %>
                                <% if (num < 9) { %>
                                    <span class="material"><%= Server.HtmlEncode(material["description"].ToString()) %></span>
                                <% } else { %>
                                    &hellip;
                                    <% break; %>
                                <% } %>
                            <% } %>
                        <% } %>
                    </div>
                    <h2>
                        <a href="<%= Server.HtmlEncode(result["url"].ToString()) %>">
                            <%= Server.HtmlEncode(result["description"].ToString()) %></a>
                    </h2>
                    <div class="meta">
                        <% if (result["type"] == "location") { %>
                            <%= Server.HtmlEncode(result["distance"].ToString())%> mi.
                        <% } %>
                        <%= result["municipal"].ToString() == "True" ? "municipal" : "" %>
                        <%= result["curbside"].ToString() == "True" ? "curbside" : "" %>
                        <%= Server.HtmlEncode(result["type"].ToString()) %>
                    </div>
                    <% if (details["phone"] != null) { %>
                        <div class="phone">
                            <%= Server.HtmlEncode(details["phone"].ToString())%>
                        </div>
                    <% } %>
                    <% if (details["address"] != null) { %>
                        <div class="address">
                            <% if (details["address"].ToString() != "") { %>
                                <%= Server.HtmlEncode(details["address"].ToString()) %>,
                            <% } %>
                            <%= Server.HtmlEncode(details["city"].ToString()) %>,
                            <%= Server.HtmlEncode(details["province"].ToString()) %>
                            <%= Server.HtmlEncode(details["postal_code"].ToString()) %>
                        </div>
                    <% } %>
                </li>
            <% } %>
        </ul>
    </div>

    <% if (searchPager.Total > 1)
       { %>
        <% SearchPagerNav nav = searchPager.Nav(); %>
        <% SearchPagerWindow window = searchPager.Window(10); %>
        
        <div class="search-pager">
            <% if (searchPager.Page > 1)
               { %>
                <a class="prev" href="<%= baseUrl %>&amp;page=<%= nav.Prev %>">&laquo; Prev</a>
            <% }
               else
               { %>
                <span class="no-prev">&laquo; Prev</span>
            <% } %>
            
            <% foreach (int i in window.Before)
               { %>
                <a class="before" href="<%= baseUrl %>&amp;page=<%= i %>"><%= i%></a>
            <% } %>

            <span class="current"><%= nav.Page%></span>

            <% foreach (int i in window.After)
               { %>
                <a class="after" href="<%= baseUrl %>&amp;page=<%= i %>"><%= i%></a>
            <% } %>
            
            <% if (searchPager.Page < searchPager.Total)
               { %>
                <a class="next" href="<%= baseUrl %>&amp;page=<%= nav.Next %>">Next &raquo;</a>
            <% }
               else
               { %>
                <span class="no-next">Next &raquo;</span>
            <% } %>    
        </div>
    <% } %>
</asp:Content>
