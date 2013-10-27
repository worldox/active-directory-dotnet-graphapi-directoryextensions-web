﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.ActiveDirectory.GraphHelper;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Neo4jClient;

namespace OrgChart.Controllers
{
    public class User
    {
        public String displayName {get; set; }
        public String objectId { get; set; }
        public String mailNickname { get; set; }
        public String jobTitle { get; set; }
        public String department { get; set; }
    }

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // RESTORE_WHEN_NEEDING_FULL_GRAPH_DB_POWER
            // connect to neo4j
            //var neo4jClient = new GraphClient(new Uri("http://OrgChart2:6IyKHclsfCHR6nOB9Eg5@OrgChart2.sb01.stations.graphenedb.com:24789/db/data/"));
            //neo4jClient.Connect();

            // use ADAL library to connect to AAD tenant
            string baseGraphUri = StringConstants.baseGraphUri + StringConstants.tenant;
            GraphQuery graphCall = new GraphQuery();
            graphCall.apiVersion = StringConstants.apiVersion;
            graphCall.baseGraphUri = baseGraphUri;
            // get token using OAuth Authorization Code
            AzureADAuthentication aadAuthentication = new AzureADAuthentication();
            AuthenticationResult authenticationResult = aadAuthentication.GetAuthenticationResult(StringConstants.tenant,
                                             StringConstants.clientId, StringConstants.clientSecret,
                                             StringConstants.resource, StringConstants.authenticationEndpoint);
            if (authenticationResult != null)
            {
                graphCall.aadAuthentication = aadAuthentication;
                graphCall.aadAuthentication.aadAuthenticationResult = authenticationResult;
                // graph call: get users (default page size 99)
                AadUsers foundUsers = graphCall.getUsers();
                ViewBag.foundUsers = foundUsers;
                if (foundUsers == null)
                {
                    ViewBag.Message = "No users found!";
                }
                else
                {
                    ViewBag.Message = "Users found!";
                    /* RESTORE_WHEN_NEEDING_FULL_GRAPH_DB_POWER
                    // iterate over all users to load into neo4j
                    foreach (AadUser user in foundUsers.user)
                    {
                        // declare new user object
                        var newUser = new User {
                            displayName = user.displayName,
                            objectId = user.objectId,
                            mailNickname = user.mailNickname,
                            jobTitle = user.jobTitle,
                            department = user.department
                        };
                        // MERGE doesn't support map properties, need to explicitly specify properties
                        string strMerge = @"(user:User 
                            { displayName: {newUser}.displayName, 
                              objectId: {newUser}.objectId, 
                              mailNickname: {newUser}.mailNickname, 
                              jobTitle: {newUser}.jobTitle, 
                              department: {newUser}.department 
                            })";
                        // neo4j call to store user
                        neo4jClient.Cypher
                                .Merge(strMerge)
                                .WithParam("newUser", newUser)
                                .ExecuteWithoutResults();
                    }
                    // iterate again to create :MANAGES links
                    foreach (AadUser user in foundUsers.user)
                    {
                        //set WHERE string for this user
                        String strWhere1 = "u.mailNickname = \"";
                        strWhere1 += user.mailNickname;
                        strWhere1 += "\"";
                        String strMatch2;
                        String strWhere2;
                        String strLinkCreation;

                        // graph call to get manager
                        AadUser manager = graphCall.getUsersManager(user.userPrincipalName);
                        
                        // set strings for node that will point to this user
                        if (manager != null)
                        {
                            strMatch2 = "(m:User)";
                            strWhere2 = "m.mailNickname = \"";
                            strWhere2 += manager.mailNickname;
                            strWhere2 += "\"";
                            strLinkCreation = "m-[:MANAGES]->u";
                        }
                        else
                        {
                            strMatch2 = "(m)";
                            strWhere2 = "NOT (m:User)";
                            strLinkCreation = "m-[:CONTAINS]->u";
                        }
                        // neo4j call to set :MANAGES or :CONTAINS link
                        neo4jClient.Cypher
                            .Match("(u:User)", strMatch2)
                            .Where(strWhere1)
                            .AndWhere(strWhere2)
                            .CreateUnique(strLinkCreation)
                            .ExecuteWithoutResults();
                    }
                    */
                }
            }
            else
            {
                ViewBag.Message = "Authentication Failed!";
            }
            ViewBag.UPN = RouteData.Values["id"];
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Here is where you learn about the app.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Here is where you learn about the authors.";

            return View();
        }

    }
}
