using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace QC__Checker.ViewModel
{
    public class DesignatedParameterCheckResult
    {
        public string FilePath { get; set; }
        public string ElementId { get; set; }
        public string Parameter { get; set; }
        public string Value { get; set; }
        public string Message { get; set; }
        public bool IsPass { get; set; } // Explicit pass/fail flag
    }

    public class DesignatedParameterCheck
    {
        public static readonly Dictionary<string, HashSet<string>> AcceptableValues = new Dictionary<string, HashSet<string>>
        {
            { "AGF_DevelopmentUse", new HashSet<string> {
                "Residential (Landed)", "Residential (Non-landed)", "Cemetery", "Commercial", "Hotel", "Business Park", "Business 1", "Business 2",
                "Health & Medical Care", "Educational Institution", "Place of Worship", "Civic & Community Institution", "Open Space", "Park",
                "Beach Area", "Sports & Recreation", "Road", "Transport Facilities", "Rapid Transit", "Utility", "Agriculture", "Port/Airport",
                "Special Use", "Waterbody", "Reserve Site"
            }},
            { "AGF_BuildingTypology", new HashSet<string> {
                "Flats", "Condominium", "Shophouse", "Terrace House", "Detached House", "Semi-Detached House", "Good Class Bungalow",
                "Strata-Landed Housing", "Light Industry", "Clean Industry", "General Industry", "Special Industry", "Serviced Apartments"
            }},
            { "AGF_BonusGFAType", new HashSet<string> {
                "Balcony Incentive Scheme", "Conserved Bungalows Scheme", "Indoor Recreation Spaces Scheme", "Built Environment Transformation Scheme",
                "Community and Sports Facilities Scheme", "Rooftop ORA on Landscaped Roofs", "ORA within Privately-Owned Public Spaces (POPS)",
                "CBD Incentive Scheme", "Strategic Development Incentive (SDI) Scheme", "Facade Articulation Scheme", "Utility GFA for DCS/CCS networks"
            }},
            { "AST_AreaType", new HashSet<string> {
                "Strata (Private)", "Strata (Communal)", "Communal Area"
            }},
        };

        private static readonly Dictionary<string, string> FailMessages = new Dictionary<string, string>
        {
            { "AGF_DevelopmentUse", "Value of AGF_DevelopmentUse not found within acceptable list." },
            { "AGF_BuildingTypology", "Value of AGF_BuildingTypology not found within acceptable list." },
            { "AGF_BonusGFAType", "Value of AGF_BonusGFAType not found within acceptable list." },
            { "AST_AreaType", "Value of AST_AreaType not found within acceptable list." }
        };

        public List<DesignatedParameterCheckResult> Check(Document doc, ElementId activeViewId)
        {
            var results = new List<DesignatedParameterCheckResult>();
            var areas = new FilteredElementCollector(doc)
             .OfCategory(BuiltInCategory.OST_Areas)
             .WhereElementIsNotElementType();

            foreach (var area in areas)
            {
                foreach (var paramName in AcceptableValues.Keys)
                {
                    var param = area.LookupParameter(paramName);
                    var value = param?.AsString() ?? string.Empty;
                    bool isPass = !string.IsNullOrWhiteSpace(value) && AcceptableValues[paramName].Any(v => v.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
                    results.Add(new DesignatedParameterCheckResult
                    {
                        FilePath = doc.PathName,
                        ElementId = area.Id.ToString(),
                        Parameter = paramName,
                        Value = value,
                        Message = isPass ? "Pass" : FailMessages[paramName],
                        IsPass = isPass
                    });
                }
            }
            return results;
        }
    }
}
