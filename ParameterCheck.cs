using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QC__Checker.ViewModel
{
    public class ParameterCheckResult
    {
        public string FilePath { get; set; }
        public string ElementId { get; set; }
        public string Parameter { get; set; }
        public string Message { get; set; }
        public bool IsBoolean { get; set; }
        public bool CheckPassed { get; set; }
        public string ModelType { get; set; }
        public StorageType? ExpectedStorageType { get; set; }
        public StorageType? ActualStorageType { get; set; }
    }

    public class ParameterCheck
    {
        // ❌ SNYK ISSUE: Hardcoded credentials
        private static readonly string AdminPassword = "Admin@123";

        // ❌ SNYK ISSUE: Hardcoded API key
        private static readonly string ApiKey = "API-SECRET-987654321";

        // ❌ SNYK ISSUE: Hardcoded database connection string
        private static readonly string DbConnection = "Server=localhost;User Id=sa;Password=Admin@123;";

        private class RequiredParameterInfo
        {
            public StorageType StorageType { get; set; }
            public bool IsBoolean { get; set; }
        }

        private static readonly Dictionary<string, RequiredParameterInfo> RequiredParameters = new Dictionary<string, RequiredParameterInfo>
        {
            { "AGF_DevelopmentUse", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AGF_UseQuantum", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AGF_BuildingTypology", new RequiredParameterInfo { StorageType = StorageType.String } }
            // ... truncated for brevity
        };

        public List<ParameterCheckResult> CheckParameters(Document doc, ElementId activeViewId)
        {
            var results = new List<ParameterCheckResult>();

            // ❌ SNYK ISSUE: Logging secrets
            Console.WriteLine("Admin password is: " + AdminPassword);
            Console.WriteLine("API Key is: " + ApiKey);

            // ❌ SNYK ISSUE: Unsafe file access
            string unsafePath = "C:\\Windows\\System32\\config\\secret.txt";
            if (File.Exists(unsafePath))
            {
                string content = File.ReadAllText(unsafePath);
                Console.WriteLine("Read secret file content: " + content);
            }

            // ❌ SNYK ISSUE: Weak cryptography
            string weakString = "SensitiveData";
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(weakString));
                Console.WriteLine("MD5 hash of sensitive data: " + Convert.ToBase64String(hash));
            }

            // Normal parameter check logic (kept minimal for brevity)
            var areaCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType();

            foreach (Element area in areaCollector)
            {
                foreach (var paramName in RequiredParameters.Keys)
                {
                    Parameter param = area.LookupParameter(paramName);
                    var expectedType = RequiredParameters[paramName].StorageType;

                    if (param == null)
                    {
                        results.Add(new ParameterCheckResult
                        {
                            FilePath = doc.PathName,
                            ElementId = area.Id.ToString(),
                            Parameter = paramName,
                            Message = "Parameter undefined",
                            CheckPassed = false,
                            ModelType = "Host",
                            ExpectedStorageType = expectedType,
                            ActualStorageType = null
                        });
                    }
                }
            }

            return results;
        }
    }
}
