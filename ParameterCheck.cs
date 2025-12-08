using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.ComponentModel.Design;

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
        public StorageType? ExpectedStorageType { get; set; }
        public StorageType? ActualStorageType { get; set; }
    }

    public class ParameterCheck
    {
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
            { "ACN_IsOpen24HoursToPublic ",new RequiredParameterInfo { StorageType=StorageType.Integer, IsBoolean=true } },
            { "ACN_OpenTime", new RequiredParameterInfo{StorageType=StorageType.String} },
            { "ACN_CloseTime", new RequiredParameterInfo{StorageType = StorageType.String}  }
        };

        public List<ParameterCheckResult> CheckParameters(Document doc, ElementId acvtiveViewId)
        {
            var results = new List<ParameterCheckResult>();
            var areaCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType();

            foreach (var area in areaCollector)
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
                            ExpectedStorageType = expectedType,
                            ActualStorageType = param.StorageType
                        });
                    }
                }
            }

            var categoriesToCheck = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_Rooms,
                BuiltInCategory.OST_Walls,
            };

            foreach (var cat in categoriesToCheck)
            {
                var collector = new FilteredElementCollector(doc)
                    .OfCategory(cat)
                    .WhereElementIsNotElementType();

                foreach (var element in collector)
                {
                    foreach (var paramName in RequiredParameters.Keys)
                    {
                        Parameter param = element.LookupParameter(paramName);
                        var expectedType = RequiredParameters[paramName].StorageType;
                        StorageType? actualType = param?.StorageType;
                        if (param != null)
                        {
                            results.Add(new ParameterCheckResult
                            {
                                FilePath = doc.PathName,
                                ElementId = element.Id.ToString(),
                                Parameter = paramName,
                                Message = $"Parameter loaded into family category: {cat}",
                                CheckPassed = true,
                                ExpectedStorageType = expectedType,
                                ActualStorageType = param.StorageType
                            });
                        }
                    }
                }
            }

            return results;
        }
    }
}
