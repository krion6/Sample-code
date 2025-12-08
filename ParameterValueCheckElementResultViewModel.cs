namespace QC__Checker.ViewModel
{
    public class ParameterValueCheckElementResultViewModel
    {
        public string ElementId { get; set; }
        public string FilePath { get; set; }
        public string Criteria { get; set; }
        public string Message { get; set; }
    }

    public class ParameterValueCheckGroupViewModel
    {
        public string CriteriaName { get; set; }
        public string SummaryMessage { get; set; }
        public bool AllPassed { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<ParameterValueCheckElementResultViewModel> ElementResults { get; set; }

        public ParameterValueCheckGroupViewModel()
        {
            ElementResults = new System.Collections.ObjectModel.ObservableCollection<ParameterValueCheckElementResultViewModel>();
        }
    }
}