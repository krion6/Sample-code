using Autodesk.Revit.DB;
using QC__Checker.ViewModel;
using System.Windows;

namespace QC__Checker.Views
{
    public partial class DesignatedParameterCheckView : Window
    {
        public DesignatedParameterCheckView(Document doc, ElementId viewId)
        {
            InitializeComponent();
            DataContext = new DesignatedParameterCheckSummaryViewModel(doc, viewId);
        }
    }
}