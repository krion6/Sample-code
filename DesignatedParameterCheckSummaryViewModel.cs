using Autodesk.Revit.DB;
using System.Collections.ObjectModel;
using System.Linq;

namespace QC__Checker.ViewModel
{
    public class DesignatedParameterCheckSummaryViewModel
    {
        public ObservableCollection<DesignatedParameterCheckGroupViewModel> Groups { get; set; }

        public DesignatedParameterCheckSummaryViewModel(Document doc, ElementId viewId)
        {
            Groups = new ObservableCollection<DesignatedParameterCheckGroupViewModel>();

            var checker = new DesignatedParameterCheck();
            var results = checker.Check(doc, viewId);

            if (results.Count == 0)
            {
                Groups.Add(new DesignatedParameterCheckGroupViewModel
                {
                    GroupName = "All Parameters",
                    AllPassed = true,
                    SummaryMessage = "All designated parameters have acceptable values.",
                    ElementResults = new ObservableCollection<DesignatedParameterCheckElementResultViewModel>(),
                    PassCount = 0,
                    FailCount = 0,
                    TotalCount = 0
                });
            }
            else
            {
                var checkerParams = DesignatedParameterCheck.AcceptableValues.Keys.ToList();
                foreach (var paramName in checkerParams)
                {
                    var group = results.Where(r => r.Parameter == paramName).ToList();
                    int passCount = group.Count(r => r.IsPass);
                    int failCount = group.Count(r => !r.IsPass);
                    int totalCount = passCount + failCount;
                    bool allPassed = failCount == 0;
                    string summaryMsg = allPassed ? $"{paramName} - All Passed" : $"{paramName} - {failCount} failed, {passCount} passed";

                    var groupVM = new DesignatedParameterCheckGroupViewModel
                    {
                        GroupName = paramName,
                        AllPassed = allPassed,
                        SummaryMessage = summaryMsg,
                        PassCount = passCount,
                        FailCount = failCount,
                        TotalCount = totalCount,
                        ElementResults = new ObservableCollection<DesignatedParameterCheckElementResultViewModel>()
                    };
                    foreach (var result in group)
                    {
                        groupVM.ElementResults.Add(new DesignatedParameterCheckElementResultViewModel
                        {
                            ElementId = result.ElementId,
                            FilePath = result.FilePath,
                            Parameter = result.Parameter,
                            Value = result.Value,
                            Message = result.Message,
                        });
                    }
                    Groups.Add(groupVM);
                }
            }
        }
    }
}
