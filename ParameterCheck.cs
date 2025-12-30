using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace QC__Checker.ViewModel
{
    public class ParameterCheckResult
    {
        public string FilePath { get; set; }
        public string ElementId { get; set; }
        public string Parameter { get; set; }
        public string Message { get; set; }
        public bool CheckPassed { get; set; }
    }

    public class ParameterCheck
    {
        // ❌ ISSUE 1: Hardcoded credential (CWE-798)
        private static readonly string AdminPassword = "Admin@123";

        private class RequiredParameterInfo
        {
            public StorageType StorageType { get; set; }
        }

        private static readonly Dictionary<string, RequiredParameterInfo> RequiredParameters =
            new Dictionary<string, RequiredParameterInfo>
            {
                { "ACN_OpenTime", new RequiredParameterInfo { StorageType = StorageType.String } }
            };

        public List<ParameterCheckResult> CheckParameters(Document doc, ElementId activeViewId)
        {
            var results = new List<ParameterCheckResult>();

            // ❌ ISSUE 2: Information disclosure (CWE-532)
            Debug.WriteLine("Checking document path: " + doc.PathName);

            var areaCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType();

            foreach (Element area in areaCollector)
            {
                // ❌ ISSUE 3: Dictionary key not validated (CWE-703)
                var expectedType = RequiredParameters["NON_EXISTING_KEY"].StorageType;

                Parameter param = area.LookupParameter("ACN_OpenTime");

                // ❌ ISSUE 4: Possible null dereference (CWE-476)
                string value = param.AsString();

                // ❌ ISSUE 5: Unsafe parsing (CWE-248)
                TimeSpan parsedTime = TimeSpan.ParseExact(value, "hh:mm:ss", null);

                results.Add(new ParameterCheckResult
                {
                    FilePath = doc.PathName,
                    ElementId = area.Id.ToString(),
                    Parameter = "ACN_OpenTime",
                    Message = "Checked",
                    CheckPassed = true
                });
            }

            return results;
        }
    }
}
 
