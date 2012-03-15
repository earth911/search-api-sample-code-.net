<%@ Page Title="" Language="C#" MasterPageFile="~/Earth911MasterPage.master" AutoEventWireup="true" CodeFile="Details.aspx.cs" Inherits="Details" %>
<%@ Import Namespace="Jayrock.Json" %>

<asp:Content ID="Content1" ContentPlaceHolderID="title" Runat="Server">
    <% if (details == null) { %>
        Not Found
    <% } else { %>
        <%= Server.HtmlEncode(details["description"].ToString())%>
    <% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <% if (details == null) { %>
        <br />
        <h1>404 - Page not found</h1>
        <p>Sorry, the page you requested could not be found.</p>
    <% } else { %>
        <div class="search-details">
            <% if (searchArgs.What != "" && searchArgs.Where != "") { %>
                <div class="back-link">
                    &laquo; <a href="Search.aspx?<%= Server.HtmlEncode(searchArgs.QueryString) %>">Back to search results</a>
                    for &ldquo;<%= Server.HtmlEncode(searchArgs.What)%>&rdquo;
                    near &ldquo;<%= Server.HtmlEncode(searchArgs.Where)%>&rdquo;
                </div>
            <% } %>

            <div class="contact">
                <% if (details["phone"] != null) { %>
                    <div class="phone">
                        <%= Server.HtmlEncode(details["phone"].ToString())%>
                    </div>
                <% } %>
                <% if (details["address"] != null) { %>
                    <div class="address">
                        <% if (details["address"].ToString() != "") { %>
                            <%= Server.HtmlEncode(details["address"].ToString()) %><br />
                        <% } %>
                        <%= Server.HtmlEncode(details["city"].ToString()) %>,
                        <%= Server.HtmlEncode(details["province"].ToString()) %>
                        <%= Server.HtmlEncode(details["postal_code"].ToString()) %>
                    </div>
                <% } %>
            </div>

            <h1><%= Server.HtmlEncode(details["description"].ToString()) %></h1>
            
            <div class="notes"><%= Server.HtmlEncode(details["notes"].ToString()) %></div>
            
            <div class="hours"><%= Server.HtmlEncode(details["hours"].ToString()) %></div>
            
            <% if (details["url"].ToString() != "") { %>
                <div class="website">
                    <a href="<%= Server.HtmlEncode(details["url"].ToString())%>">
                        <%= Server.HtmlEncode(details["url"].ToString())%></a>
                </div>
            <% } %>

            <table>
                <tr>
                    <th width="33%">Materials Accepted</th>
                    <th colspan="2">Services</th>
                    <th width="50%">&nbsp;</th>
                </tr>
                <tr class="subhead">
                    <th>Material</th>
                    <th>Residential</th>
                    <th>Business</th>
                    <th>Notes</th>
                </tr>
                <% int row = 0; %>
                <% foreach (JsonObject material in (JsonArray)details["materials"]) { %>
                   <%
                       row++;
                       string classes = (row % 2 == 0) ? "even" : "odd";
                       if (row == 1) classes += " first";
                   %>
                   <tr class="<%= classes %>">
                        <td><%= Server.HtmlEncode(material["description"].ToString()) %></td>
                        <td><%= Server.HtmlEncode(material["residential_method"].ToString()) %></td>
                        <td><%= Server.HtmlEncode(material["business_method"].ToString()) %></td>
                        <td><%= Server.HtmlEncode(material["notes"].ToString()) %></td>
                   </tr>
                <% } %>
            </table>
        </div>
    <% } %>
</asp:Content>
