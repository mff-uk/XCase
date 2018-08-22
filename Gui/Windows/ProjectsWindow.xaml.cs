//#define bindAllVersions

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using XCase.Controller;
using XCase.Model;
using XCase.Controller.Commands;
using System.Globalization;
using Version=XCase.Model.Version;
using XCase.Gui.MainMenuCommands;
using XCase.View.Interfaces;

namespace XCase.Gui
{
    /// <summary>
    /// Interaction logic for ProjectsWindow.xaml
    /// </summary>
    public partial class ProjectsWindow
    {
        #region Fields

        internal VersionManager versionManager;

        /// <summary>
        /// Project to which this window is actually bound.
        /// </summary>
        private Project project = null;

        /// <summary>
        /// Collection of projects to which this window is actually bound.
        /// This collection always has only one member, collection is used only for convenience while using TreeView.
        /// </summary>
        private ObservableCollection<Project> projects = new ObservableCollection<Project>();

        #endregion

        #region Events

        /// <summary>
        /// Event invoked after double click on a diagram
        /// </summary>
        public event EventHandler<DiagramDClickArgs> DiagramDClick;

        /// <summary>
        /// Event invoked when a diagram was removed
        /// </summary>
        public event EventHandler<DiagramDClickArgs> DiagramRemove;

        /// <summary>
        /// Event invoked when a diagram was renamed
        /// </summary>
        public event EventHandler<DiagramRenameArgs> DiagramRename;

        #endregion

        public MainWindow MainWindow { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectsWindow()
        {
            InitializeComponent();

            //this.Icon = (ImageSource)FindResource("view_remove");
        }

        /// <summary>
        /// Creates binding between <paramref name="proj"/> and this window
        /// <param name="proj">Project to which this window should be bound</param>
        /// </summary>
        public void BindToProject(Project proj)
        {
            project = proj;
            #if bindAllVersions
            #else 
            projects.Clear();
            #endif
            if (!projects.Contains(project))
                projects.Add(project);
            projectView.ItemsSource = projects;
            if (MainWindow.CurrentProject != project)
                MainWindow.CurrentProject = project;
            if (cbBranches.DataContext != null)
            {
                cbBranches.SelectedItem = proj;
            }
        }

        public void BindToVersionManager(VersionManager versionManager)
        {
            this.versionManager = versionManager;
            projects.Clear();
            #if bindAllVersions
            foreach (Project p in versionManager.VersionedProjects.Values)
            {
                projects.Add(p);
            }
            #else 
            projects.Add(versionManager.LatestVersion);
            #endif
            foreach (Project p in versionManager.VersionedProjects.Values)
            {
                p.GetModelController().getUndoStack().ItemsChanged += MainWindow.UndoStack_ItemsChanged;
            }
            
            project = versionManager.LatestVersion;
            MainWindow.CurrentProject = project; 
            cbBranches.DataContext = versionManager;
            cbBranches.SelectedItem = versionManager.LatestVersion.Version;
        }

        /// <summary>
        /// Destroys binding between this window and actual project
        /// </summary>
        public void UnbindProject()
        {
            project = null;
        }


        public void UnbindVersionManager()
        {
            versionManager = null;
            cbBranches.DataContext = null;
            foreach (Project project in projects)
            {
                project.GetModelController().getUndoStack().ItemsChanged -=  MainWindow.UndoStack_ItemsChanged;
            }
            projects.Clear();
        }

        private static void DeselectAll(DependencyObject parent)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is TreeViewItem) (child as TreeViewItem).IsSelected = false;
                DeselectAll(child);
            }
        }

        #region Event handlers

        private void OnMemberDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as TreeViewItem).DataContext is Diagram)
            {
                InvokeDiagramDClick((sender as TreeViewItem).DataContext as Diagram);
            }
        }

        private void OnAddPIMDiagramClick(object sender, RoutedEventArgs e)
        {
            AddPIMDiagramCommand addDiagramCommand = AddPIMDiagramCommandFactory.Factory().Create(project.GetModelController()) as AddPIMDiagramCommand;
            addDiagramCommand.Set(project, null);
            addDiagramCommand.Execute();
            InvokeDiagramDClick(addDiagramCommand.Diagram);
        }

        private void OnAddPSMDiagramClick(object sender, RoutedEventArgs e)
        {
            AddPSMDiagramCommand addDiagramCommand = AddPSMDiagramCommandFactory.Factory().Create(project.GetModelController()) as AddPSMDiagramCommand;
            addDiagramCommand.Set(project, null);
            addDiagramCommand.Execute();
            InvokeDiagramDClick(addDiagramCommand.Diagram);
        }

        private void OnProjectRenameClick(object sender, RoutedEventArgs e)
        {
            string projName;
            if (InputBox.Show("Input project caption", project.Caption, out projName) == true &&
                projName != project.Caption)
            {
                RenameProjectCommand renameProjectCommand = RenameProjectCommandFactory.Factory().Create(project.GetModelController()) as RenameProjectCommand;
                renameProjectCommand.Set(project, projName);
                renameProjectCommand.Execute();
            }
        }

        private void OnMemberRenameClick(object sender, RoutedEventArgs e)
        {
            MenuItem m = sender as MenuItem;
            if (m.DataContext is Diagram)
            {
                Diagram diagram = m.DataContext as Diagram;
                string diagramName;
                if (InputBox.Show("Insert diagram caption", diagram.Caption, out diagramName) == true &&
                    diagramName != diagram.Caption)
                {
                    InvokeDiagramRename(diagram, diagramName);
                }
            }
        }

        private void OnMemberRemoveClick(object sender, RoutedEventArgs e)
        {
            MenuItem m = sender as MenuItem;
            if (m.DataContext is Diagram)
            {
                InvokeDiagramRemove(m.DataContext as Diagram);
            }
        }

        private void OnMemberClick(object sender, MouseButtonEventArgs e)
        {
            DeselectAll(projectView);
            (sender as TreeViewItem).IsSelected = true;
        }

        private void OnItemSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Project)
            {
                (projectView.ItemContainerGenerator.ContainerFromItem(e.NewValue) as TreeViewItem).IsSelected = false;
            }
        }

        private void OnProjectChangeNamespaceClick(object sender, RoutedEventArgs e)
        {
            string newString;
            if (InputBox.Show("Change project's XML namespace", project.GetModelController().Project.Schema.XMLNamespace, out newString) == true &&
                newString != project.GetModelController().Project.Schema.XMLNamespace)
            {
                RenameProjectNamespaceCommand c = RenameProjectNamespaceCommandFactory.Factory().Create(project.GetModelController()) as RenameProjectNamespaceCommand;
                c.NewNamespaceName = newString;
                c.Execute();
            }
        }

        private void OnChangeDiagramTargetNamespaceClick(object sender, RoutedEventArgs e)
        {
            MenuItem m = sender as MenuItem;
            if (m.DataContext is Diagram)
            {
                Diagram diagram = m.DataContext as Diagram;
                DiagramController controller = new DiagramController(diagram, project.GetModelController());
                string tn;
                string currentTargetNamespace = (diagram as PSMDiagram).TargetNamespace;
                if (InputBox.Show("Insert diagram target namespace", currentTargetNamespace, out tn) == true &&
                    tn != currentTargetNamespace)
                {
                    controller.ChangeTargetNamespace(tn);
                }
            }
        }

        #endregion

        #region Event invokers

        /// <summary>
        /// Invokes diagram double click event with given <paramref name="diagram"/> as an argument
        /// <param name="diagram">Double clicked diagram</param>
        /// </summary>
        public void InvokeDiagramDClick(Diagram diagram)
        {
            // Copy to a temporary variable to be thread-safe.
            EventHandler<DiagramDClickArgs> temp = DiagramDClick;
            if (temp != null)
                temp(this, new DiagramDClickArgs(diagram));
        }

        /// <summary>
        /// Invokes diagram remove event with given <paramref name="diagram"/> as an argument
        /// <param name="diagram">Diagram to be removed</param>
        /// </summary>
        public void InvokeDiagramRemove(Diagram diagram)
        {
            // Copy to a temporary variable to be thread-safe.
            EventHandler<DiagramDClickArgs> temp = DiagramRemove;
            if (temp != null)
                temp(this, new DiagramDClickArgs(diagram));
        }

        /// <summary>
        /// Invokes diagram rename event with given <paramref name="diagram"/> and its <paramref name="newCaption"/> as arguments
        /// <param name="diagram">Diagram to be renamed</param>
        /// <param name="newCaption">New caption of the diagram</param>
        /// </summary>
        public void InvokeDiagramRename(Diagram diagram, String newCaption)
        {
            // Copy to a temporary variable to be thread-safe.
            EventHandler<DiagramRenameArgs> temp = DiagramRename;
            if (temp != null)
                temp(this, new DiagramRenameArgs(diagram, newCaption));
        }

        #endregion

        private void Branch_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            ElementWatcher.ClearRecording();
            ElementWatcher.Recording = true;
#endif

            Project branchProject;

            if (versionManager == null)
            {
                versionManager = new VersionManager();
                branchProject = versionManager.BranchProject(project, MainWindow.PIMRepresentantsSet.ElementRepresentationOrder, true);
                cbBranches.DataContext = versionManager;
            }
            else
            {
                branchProject = versionManager.BranchProject(project, MainWindow.PIMRepresentantsSet.ElementRepresentationOrder, false);
            }

            branchProject.CreateModelController();
            branchProject.GetModelController().getUndoStack().ItemsChanged += MainWindow.UndoStack_ItemsChanged;

            cbBranches.SelectedItem = branchProject.Version;
            MainWindow.HasUnsavedChanges = true;

#if DEBUG
            Tests.ModelIntegrity.ModelConsistency.CheckEverything(project);
            Tests.ModelIntegrity.ModelConsistency.CheckEverything(branchProject);
            Tests.ModelIntegrity.ModelConsistency.CheckElementSchema(ElementWatcher.CreatedElements, branchProject.Schema, null);

            Tests.ModelIntegrity.VersionsConsistency.CheckVersionsConsistency(versionManager);
            foreach (Element element in ElementWatcher.CreatedElements)
            {
                if (element.Version != branchProject.Version && !(element is InstantiatedProperty) && !(element is StereotypeInstance))
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("{0} element without version", element));
                }
            }
#endif
        }

        private void cbBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (versionManager != null)
            {
                UnbindProject();

                if (cbBranches.SelectedItem != null)
                {
                    SwitchToVersion((Version)cbBranches.SelectedItem);
                }

                bRename.IsEnabled = cbBranches.SelectedItem != null;
                bDelete.IsEnabled = cbBranches.SelectedItem != null;
            }
        }

        public void SwitchToVersion(Version version)
        {
            BindToProject(versionManager.VersionedProjects[version]);
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            string newName; 
            if (InputBox.Show("Rename version", (cbBranches.SelectedItem as Version).Label, out newName) == true)
            {
                RenameVersionCommand command = (RenameVersionCommand) RenameVersionCommandFactory.Factory().Create(project.GetModelController());
                command.Set(project.Version, newName);
                command.Execute();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Version version = cbBranches.SelectedValue as Version;
            if (version != null &&  versionManager != null && versionManager.Versions.Count > 1 &&
                XCaseYesNoBox.Show("Remove version", string.Format("Do you really want to remove version {0}?\r\nThis action can not be undone.", version)) == MessageBoxResult.Yes)
            {
                UnbindProject();
                versionManager.DeleteVersion(version);
                BindToProject(versionManager.LatestVersion);
                cbBranches.SelectedItem = versionManager.LatestVersion.Version;
                MainWindow.HasUnsavedChanges = true;

                #if DEBUG
                Tests.ModelIntegrity.ModelConsistency.CheckEverything(project);
                //Tests.ModelIntegrity.ModelConsistency.CheckElementSchema(ElementWatcher.CreatedElements, project.Schema, null);

                Tests.ModelIntegrity.VersionsConsistency.CheckVersionsConsistency(versionManager);
                foreach (Element element in ElementWatcher.CreatedElements)
                {
                    if (element.Version == null && !(element is InstantiatedProperty) && !(element is StereotypeInstance))
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("{0} element without version", element));
                    }
                }
                #endif
                
            }
        }

        private void bLocatePrev_Click(object sender, RoutedEventArgs ea)
        {
            if (MainWindow.ActiveDiagram != null && MainWindow.ActiveDiagram.SelectedItems.Count > 0)
            {
                ISelectable i = MainWindow.ActiveDiagram.SelectedItems[0];
                if (i is IModelElementRepresentant)
                {
                    Element e = MainWindow.ActiveDiagram.ElementRepresentations.GetElementRepresentedBy((IModelElementRepresentant)i);
                    Version v = MainWindow.ActiveDiagram.Diagram.Project.VersionManager.Versions[0];

                    if (e.ExistsInVersion(v) && MainWindow.DiagramTabManager.GetDiagramView((Diagram)MainWindow.ActiveDiagram.Diagram.GetInVersion(v)) != null)
                    {
                        MainWindow.DiagramTabManager.GetDiagramView((Diagram)MainWindow.ActiveDiagram.Diagram.GetInVersion(v)).SelectElement((Element)e.GetInVersion(v));
                    }
                }

            }
        }
    }

    #region Event arguments

    /// <summary>
    /// Event arguments for diagram selection and diagram remove.
    /// </summary>
    public class DiagramDClickArgs : EventArgs
    {
        private Diagram diag;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="diagram">Diagram which is being manipulated</param>
        public DiagramDClickArgs(Diagram diagram)
        {
            diag = diagram;
        }

        /// <summary>
        /// Diagram which is being manipulated
        /// </summary>
        public Diagram Diagram
        {
            get { return diag; }
            set { diag = value; }
        }
    }

    /// <summary>
    /// Event arguments for diagram rename.
    /// </summary>
    public class DiagramRenameArgs : EventArgs
    {
        private Diagram diag;
        private String newCapt;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="diagram">Renamed diagram</param>
        /// <param name="newCaption">New name of the diagram</param>
        public DiagramRenameArgs(Diagram diagram, String newCaption)
        {
            diag = diagram;
            newCapt = newCaption;
        }

        /// <summary>
        /// Renamed diagram
        /// </summary>
        public Diagram Diagram
        {
            get { return diag; }
            set { diag = value; }
        }

        /// <summary>
        /// New name of the diagram
        /// </summary>
        public String NewCaption
        {
            get { return newCapt; }
            set { newCapt = value; }
        }
    }

    #endregion

    /// <summary>
    /// Convertor for displaying project namespace only when a namespace is defined
    /// </summary>
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class NamespaceVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility v = value == null || (value as string).Length == 0 ? Visibility.Collapsed : Visibility.Visible;
            return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
