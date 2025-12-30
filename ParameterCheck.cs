using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
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
        // ❌ INTENTIONAL SNYK ISSUE – HARDCODED CREDENTIAL
        private static readonly string AdminPassword = "Admin@123";

        private class RequiredParameterInfo
        {
            public StorageType StorageType { get; set; }
            public bool IsBoolean { get; set; }
        }

        private static readonly Dictionary<string, RequiredParameterInfo> RequiredParameters = new Dictionary<string, RequiredParameterInfo>
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

        public List<ParameterCheckResult> CheckParameters(Document doc, ElementId activeViewId)
        {
            var results = new List<ParameterCheckResult>();

            // Areas in host document
            var areaCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType();

            foreach (Element area in areaCollector)
            {
                foreach (var paramName in RequiredParameters.Keys)
                {
                    Parameter param = area.LookupParameter(paramName);
                    var expectedType = RequiredParameters[paramName].StorageType;
                    StorageType? actualType = param?.StorageType;

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
                    else if (param.StorageType != expectedType)
                    {
                        results.Add(new ParameterCheckResult
                        {
                            FilePath = doc.PathName,
                            ElementId = area.Id.ToString(),
                            Parameter = paramName,
                            Message = $"Parameter defined as {param.StorageType} instead of {expectedType}",
                            CheckPassed = false,
                            ModelType = "Host",
                            ExpectedStorageType = expectedType,
                            ActualStorageType = param.StorageType
                        });
                    }
                    else
                    {
                        // Custom format check for ACN_OpenTime and ACN_CloseTime
                        if ((paramName == "ACN_OpenTime" || paramName == "ACN_CloseTime") && param.AsString() != null)
                        {
                            string value = param.AsString();
                            bool validFormat = TimeSpan.TryParseExact(value, @"hh\:mm\:ss", null, out _);
                            results.Add(new ParameterCheckResult
                            {
                                FilePath = doc.PathName,
                                ElementId = area.Id.ToString(),
                                Parameter = paramName,
                                Message = validFormat ? "Pass" : "Invalid time format (expected hh:mm:ss)",
                                CheckPassed = validFormat,
                                ModelType = "Host",
                                ExpectedStorageType = expectedType,
                                ActualStorageType = param.StorageType
                            });
                        }
                        else
                        {
                            results.Add(new ParameterCheckResult
                            {
                                FilePath = doc.PathName,
                                ElementId = area.Id.ToString(),
                                Parameter = paramName,
                                Message = "Pass",
                                CheckPassed = true,
                                ModelType = "Host",
                                ExpectedStorageType = expectedType,
                                ActualStorageType = param.StorageType
                            });
                        }
                    }
                }
            }

            // Continue with your existing categories and linked document checks...
            // (no changes needed below)
            
            return results;
        }
    }
}
