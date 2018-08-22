using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XCase.Evolution;
using XCase.Model;

namespace XCase.Gui.Windows
{
    /// <summary>
    /// Interaction logic for EvolutionChangesWindow.xaml
    /// </summary>
    public partial class EvolutionChangesWindow : Window
    {
        private EvolutionChangesWindow()
        {
            InitializeComponent();
        }

        public MainWindow MainWindow { get; private set; } 

        public static bool? Show(List<EvolutionChange> changes, MainWindow MainWindow, PSMDiagram diagramOldVersion, PSMDiagram diagramNewVersion)
        {
            EvolutionChangesWindow evolutionChangesWindow = new EvolutionChangesWindow();
            Changes = changes;
            evolutionChangesWindow.MainWindow = MainWindow; 
            evolutionChangesWindow.gridChanges.ItemsSource = changes;
            evolutionChangesWindow.DiagramOldVersion = diagramOldVersion;
            evolutionChangesWindow.DiagramNewVersion = diagramNewVersion;
            evolutionChangesWindow.DiagramView = MainWindow.DiagramTabManager.GetDiagramView(diagramNewVersion);
            evolutionChangesWindow.DiagramViewOldVersion = MainWindow.DiagramTabManager.GetDiagramView(diagramOldVersion);
            evolutionChangesWindow.Show();
            return true; 
        }

        protected static List<EvolutionChange> Changes { get; set; }

        protected PSMDiagram DiagramOldVersion { get; set; }

        protected PSMDiagram DiagramNewVersion { get; set; }

        private void gridChanges_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GridChanges_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //XsltTestWindow.ShowDialog(Changes, DiagramOldVersion, DiagramNewVersion);
            DependencyObject source = (DependencyObject)e.OriginalSource;
            DataGridRow row = UIExtensions.TryFindParent<DataGridRow>(source);

            //the user did not click on a row
            if (row == null) return;

            EvolutionChange change = (EvolutionChange)row.Item;

            DiagramView.SelectElement(change.Element);
            if (change.Element.ExistsInVersion(DiagramOldVersion.Version) && DiagramViewOldVersion != null)
            {
                DiagramViewOldVersion.SelectElement((Element)change.Element.GetInVersion(DiagramOldVersion.Version));
            }

            e.Handled = true;
        }

        public View.Controls.XCaseCanvas DiagramView { get; set; }
        public View.Controls.XCaseCanvas DiagramViewOldVersion { get; set; }        
    }
}
