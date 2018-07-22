using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    class Program
    {
        static List<Server> servers = null;
        static List<Package> packages = null;
        static vmAllServerHotFix serverKbReport =null;
        static void Main(string[] args)
        {
            AddDummyData();
            AddDummySearchValues();
            List<vmAllServerHotFix> hv = (from i in servers
                                          join j in packages on i.intServerID equals j.intServerID
                                          select new vmAllServerHotFix()
                                          {
                                              strName = j.strName,
                                              strDeploymentName = i.strDeploymentName,
                                              strPackageName = j.strPackageName,
                                              strArticleID = j.strArticleID,
                                              strHotFixName = i.strHotFixName,
                                              applied = i.applied,
                                              intServerID = i.intServerID,
                                              intDeploymentID = i.intDeploymentID,
                                              intPackageID = j.intPackageID,
                                              intDeploymentType = i.intDeploymentType,
                                          }).AsQueryable().Where(GenerateFilter()).
                                          OrderBy(x=>string.IsNullOrEmpty(serverKbReport.sortColumn)?x.strName:serverKbReport.sortColumn).ToList();
        }
        public static Expression<Func<vmAllServerHotFix, bool>> GenerateFilter()
        {
            var predicate = PredicateBuilder.True<vmAllServerHotFix>();
            if (serverKbReport.searchTerm != null && serverKbReport.searchTerm == "Enter server name")
                serverKbReport.searchTerm = null;
            if (!string.IsNullOrWhiteSpace(serverKbReport.searchTerm))
            {
                predicate = predicate.And(p => p.strName.ToLower().Contains(serverKbReport.searchTerm.Trim().ToLower())); 
            }
            if (serverKbReport.deploymentFilter!=null)
            {
                predicate = predicate.And(p => serverKbReport.deploymentFilter.Contains(p.intDeploymentID));
            }

            if (!string.IsNullOrEmpty(serverKbReport.deploymentNames))
            {
                List<string> deploymentNames = serverKbReport.deploymentNames.Split(',').Select(p => p.Trim()).ToList();
                predicate = predicate.And(p => deploymentNames.Contains(p.strDeploymentName));
            }

            if (serverKbReport.articleFilter != null)
            {
                predicate = predicate.And(p => serverKbReport.articleFilter.Contains(p.strHotFixName));// ----
            }

            if (!string.IsNullOrEmpty(serverKbReport.kbArticleIds))
            {
                string[] kbArticleIds = serverKbReport.kbArticleIds.Split(',');
                predicate = predicate.And(p => kbArticleIds.Contains(p.strArticleID));
            }
            if (serverKbReport.applied)
            {
                predicate = predicate.And(p => p.applied== true);
            }
            if (!string.IsNullOrEmpty(serverKbReport.serverNames))
            {
                List<string> serverNames = serverKbReport.serverNames.Split(new string[]{"\r\n"}, StringSplitOptions.None).Select(p => p.Trim().ToUpper()).ToList();
                predicate = predicate.And(p => serverNames.Contains(p.strName.Trim())); //---
            }

            return predicate;
        }
        private static void AddDummySearchValues()
        {
            serverKbReport = new vmAllServerHotFix()
                {
                    applied = true,
                    searchTerm = "Name14",
                    deploymentNames = "Deployment34,Deployment14",
                    kbArticleIds= "1,3,5,9,10",
                };
           
        }
        private static void AddDummyData()
        {
            servers = new List<Server>();
            for (int i = 1; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    servers.Add(new Server()
                    {
                        intServerID = i,
                        intDeploymentID = (j + i),
                        intDeploymentType = (j * i),
                        applied = true,
                        strHotFixName = "HotFix" + i + j,
                        strDeploymentName = "Deployment" + i + j
                    });
                }
            }
            packages = new List<Package>();
            for (int i = 1; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    packages.Add(new Package()
                    {
                        intServerID = i,
                        strArticleID = (j + i).ToString(),
                        intPackageID = (j * i),
                        strName = "Name" + i + j,
                        strPackageName = "Package" + i + j
                    });
                }
            }
        }
    }

    class Server
    {
        public string strDeploymentName { get; set; }
        public string strHotFixName { get; set; }
        public bool applied { get; set; }
        public int intServerID { get; set; }
        public int intDeploymentID { get; set; }
        public int intDeploymentType { get; set; }
    }


    class Package
    {
        public int intServerID { get; set; }
        public string strName { get; set; }
        public string strPackageName { get; set; }
        public string strArticleID { get; set; }
        public int intPackageID { get; set; }
    }

    class vmAllServerHotFix
    {
        public string strName { get; set; }
        public string strDeploymentName { get; set; }
        public string strPackageName { get; set; }
        public string strArticleID { get; set; }
        public string strHotFixName { get; set; }
        public bool applied { get; set; }
        public int intServerID { get; set; }
        public int intDeploymentID { get; set; }
        public int intPackageID { get; set; }
        public int intDeploymentType { get; set; }

        // filter params
        public string searchTerm { get; set; }
        public List<int> deploymentFilter { get; set; }
        public string deploymentNames { get; set; }
        public string kbArticleIds { get; set; }
        public List<int> packageFilter { get; set; }
        public List<string> articleFilter { get; set; }
        public string serverNames { get; set; }
        public string sortColumn { get; set; }
    }
}
