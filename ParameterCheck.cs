using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

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
        // ❌ TEST ISSUE: Hardcoded secret for Snyk/DeepCode testt
        private const string ApiToken = "sk_test_1234567890_SECRET";

        private class RequiredParameterInfo
        {
            public StorageType StorageType { get; set; }
            public bool IsBoolean { get; set; }
            
        }

        private static readonly Dictionary<string, RequiredParameterInfo> RequiredParameters =
            new Dictionary<string, RequiredParameterInfo>
        {
            { "AGF_DevelopmentUse", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AGF_UseQuantum", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AGF_BuildingTypology", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AGF_SupportingFacility", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AGF_Name", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AGF_Note", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AGF_BonusGFAType", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AGF_UnitNumber", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AVF_IncludeAsGFA", new RequiredParameterInfo { StorageType = StorageType.Integer, IsBoolean = true } },
            { "AST_AreaType", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AST_LegalArea", new RequiredParameterInfo { StorageType = StorageType.Double } },
            { "AST_Extg_StrataLotNumber", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "AST_Prop_StrataLotNumber", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "ALS_LandscapeType", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "ALS_GreeneryFeatures", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "ALS_Species", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "ACN_ConnectivityType", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "ACN_ActivityGeneratingUseType", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "ACN_IsPavingSpecified", new RequiredParameterInfo { StorageType = StorageType.Integer, IsBoolean = true } },
            { "ACN_PavingSpecification", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "ACN_IsOpen24HoursToPublic", new RequiredParameterInfo { StorageType = StorageType.Integer, IsBoolean = true } },
            { "ACN_OpenTime", new RequiredParameterInfo { StorageType = StorageType.String } },
            { "ACN_CloseTime", new RequiredParameterInfo { StorageType = StorageType.String } }
        };

        // 🔧 FIX 1: Removed unused activeViewId parameter
        public List<ParameterCheckResult> CheckParameters(Document doc)
        {
            var results = new List<ParameterCheckResult>();

            // =========================
            // HOST DOCUMENT – AREAS
            // =========================
            var areaCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType();

            foreach (Element area in areaCollector)
            {
                foreach (var kvp in RequiredParameters)
                {
                    string paramName = kvp.Key;
                    var expectedType = kvp.Value.StorageType;

                    Parameter param = area.LookupParameter(paramName);

                    if (param == null)
                    {
                        results.Add(CreateResult(doc.PathName, area.Id, paramName,
                            "Parameter undefined", false, "Host", expectedType, null));
                        continue;
                    }

                    if (param.StorageType != expectedType)
                    {
                        results.Add(CreateResult(doc.PathName, area.Id, paramName,
                            $"Parameter defined as {param.StorageType} instead of {expectedType}",
                            false, "Host", expectedType, param.StorageType));
                        continue;
                    }

                    // 🔧 Time validation (Host)
                    if (IsTimeParameter(paramName))
                    {
                        ValidateTime(param, doc.PathName, area.Id, paramName,
                            "Host", expectedType, results);
                        continue;
                    }

                    results.Add(CreateResult(doc.PathName, area.Id, paramName,
                        "Pass", true, "Host", expectedType, param.StorageType));
                }
            }

            // =========================
            // NON-AREA USAGE CHECK
            // =========================
            var categoriesToCheck = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_Rooms,
                BuiltInCategory.OST_Walls
            };

            foreach (var cat in categoriesToCheck)
            {
                var collector = new FilteredElementCollector(doc)
                    .OfCategory(cat)
                    .WhereElementIsNotElementType();

                foreach (Element element in collector)
                {
                    foreach (var paramName in RequiredParameters.Keys)
                    {
                        Parameter param = element.LookupParameter(paramName);
                        if (param == null) continue;

                        string categoryName = element.Category?.Name ?? cat.ToString();

                        results.Add(CreateResult(doc.PathName, element.Id, paramName,
                            $"Parameter found on non-Area category: {categoryName}. Should exist only on Areas",
                            false, "Host",
                            RequiredParameters[paramName].StorageType,
                            param.StorageType));
                    }
                }
            }

            // =========================
            // LINKED DOCUMENTS – AREAS
            // =========================
            var linkInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>();

            foreach (var link in linkInstances)
            {
                Document linkedDoc = link.GetLinkDocument();
                if (linkedDoc == null) continue;

                var linkedAreas = new FilteredElementCollector(linkedDoc)
                    .OfCategory(BuiltInCategory.OST_Areas)
                    .WhereElementIsNotElementType();

                foreach (Element area in linkedAreas)
                {
                    foreach (var kvp in RequiredParameters)
                    {
                        string paramName = kvp.Key;
                        var expectedType = kvp.Value.StorageType;

                        Parameter param = area.LookupParameter(paramName);

                        if (param == null)
                        {
                            results.Add(CreateResult(linkedDoc.PathName, area.Id, paramName,
                                "Parameter undefined (linked file)", false, "Linked", expectedType, null));
                            continue;
                        }

                        if (param.StorageType != expectedType)
                        {
                            results.Add(CreateResult(linkedDoc.PathName, area.Id, paramName,
                                $"Parameter defined as {param.StorageType} instead of {expectedType} (linked file)",
                                false, "Linked", expectedType, param.StorageType));
                            continue;
                        }

                        // 🔧 FIX 3: Time validation for LINKED documents
                        if (IsTimeParameter(paramName))
                        {
                            ValidateTime(param, linkedDoc.PathName, area.Id, paramName,
                                "Linked", expectedType, results);
                            continue;
                        }

                        results.Add(CreateResult(linkedDoc.PathName, area.Id, paramName,
                            "Pass (linked file)", true, "Linked", expectedType, param.StorageType));
                    }
                }
            }

            return results;
        }

        // =========================
        // HELPER METHODS
        // =========================
        private static bool IsTimeParameter(string name)
        {
            return name == "ACN_OpenTime" || name == "ACN_CloseTime";
        }

        private static void ValidateTime(
            Parameter param,
            string filePath,
            ElementId elementId,
            string paramName,
            string modelType,
            StorageType expectedType,
            List<ParameterCheckResult> results)
        {
            string value = param.AsString();

            bool valid = !string.IsNullOrEmpty(value) &&
                         TimeSpan.TryParseExact(value, @"hh\:mm\:ss", null, out _);

            results.Add(CreateResult(
                filePath,
                elementId,
                paramName,
                valid ? "Pass" : "Invalid time format (expected hh:mm:ss)",
                valid,
                modelType,
                expectedType,
                param.StorageType));
        }

        private static ParameterCheckResult CreateResult(
            string filePath,
            ElementId id,
            string param,
            string message,
            bool pass,
            string modelType,
            StorageType? expected,
            StorageType? actual)
        {
            return new ParameterCheckResult
            {
                FilePath = filePath,
                ElementId = id.ToString(),
                Parameter = param,
                Message = message,
                CheckPassed = pass,
                ModelType = modelType,
                ExpectedStorageType = expected,
                ActualStorageType = actual
            };
        }
    }
}
