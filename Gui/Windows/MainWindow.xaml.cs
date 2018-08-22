using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Diagnostics;
using System.Linq;
using System.IO;
using AvalonDock;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;
using XCase.Gui.LogWindow;
using XCase.Gui.MainMenuCommands;
using EDraggedConnectionType = XCase.View.Controls.XCaseCanvas.DraggingConnectionState.EDraggedConnectionType;
using System.Windows.Controls;
using Version = XCase.Model.Version;
namespace XCase.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow
    {
        public MainWindowDiagramTabManager DiagramTabManager { get; private set; }

        public ShortcutActionManager ShortcutActionManager { get; private set; }

        //internal NavigatorWindow navigatorWindow = new NavigatorWindow();
        //internal PropertiesWindow propertiesWindow = new PropertiesWindow();
        //internal ProjectsWindow projectsWindow = new ProjectsWindow();
#if DEBUG
        internal LogWindow.LogWindow logWindow = new LogWindow.LogWindow();
        internal DockableContent logWindowWrap;
#endif

        private string filename;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
            : this(null)
        {
            this.Icon = (System.Windows.Media.ImageSource)FindResource("X");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="file">File to be opened on startup</param>
        public MainWindow(string file)
        {
            this.Icon = (System.Windows.Media.ImageSource)FindResource("XCaseIcon");
#if DEBUG
            PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
#endif
            InitializeComponent();

            InitializeRegistrationSets();

            DiagramTabManager = new MainWindowDiagramTabManager(this);
            ShortcutActionManager = new ShortcutActionManager(this);
            ShortcutActionManager.RegisterStandardShortcuts();

            projectsWindow.DiagramDClick += DiagramTabManager.DiagramDoubleClick;
            projectsWindow.DiagramRemove += DiagramTabManager.DiagramRemoveHandler;
            projectsWindow.DiagramRename += DiagramTabManager.DiagramRenameHandler;
            projectsWindow.MainWindow = this;
            navigatorWindow.NavigatorSelectedClass += OnNavigatorSelectedClass;

            ActiveDiagramChanged += OnActiveDiagramChanged;

            ProjectChanged += OnProjectChanged;
            ProjectChanging += OnProjectChanging;

            filename = file;

#if DEBUG
            WindowTraceListener traceListener = new WindowTraceListener { TextBox = logWindow.tbDebug };
            Debug.Listeners.Add(traceListener);
#else
			//logBrowserMenuItem.Visibility = Visibility.Hidden;
#endif
        }

        #region projects

        private Project project;

        /// <summary>
        /// Currently open project
        /// </summary>
        public Project CurrentProject
        {
            get
            {
                return project;
            }
            internal set
            {
                InvokeProjectChanging(project, value);
                project = value;
                InvokeProjectChanged();
            }
        }


        /// <summary>
        /// Closes actually opened project - closes all open tabs and clears the projects window
        /// </summary>
        internal void CloseProject()
        {
            List<DocumentContent> managedContentCollection = dockManager.Documents.ToList();
            foreach (DocumentContent document in managedContentCollection)
            {
                document.Close();
            }

            List<FloatingWindow> floatingWindows = dockManager.FloatingWindows.ToList();
            foreach (DockableFloatingWindow floatingWindow in floatingWindows)
            {
                //floatingWindow.Close(true);
                floatingWindow.Close();
            }

            projectsWindow.UnbindProject();
            projectsWindow.UnbindVersionManager();
            navigatorWindow.UnbindProject();
            project = null;

        }

        public Project OpenProject(string openedFile)
        {
            cmdOpenProject open = new cmdOpenProject(this, null) { NoDialogs = true, Filename = openedFile };
            open.Execute();

            return this.CurrentProject;
        }
#if DEBUG
        static string lastFilePath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                       "\\lastfile.ini";
            }
        }
#endif

        public void OpenProjectDiagrams()
        {
#if DEBUG
            if (Environment.MachineName.Contains("TRUPIK"))
            {
                string currentFile = CurrentProject.FilePath;
                List<string> recentFiles;
                if (File.Exists(lastFilePath))
                    recentFiles = File.ReadAllLines(lastFilePath).ToList();
                else
                    recentFiles = new List<string>();
                if (!String.IsNullOrEmpty(currentFile))
                {
                    if (recentFiles.Contains(currentFile))
                        recentFiles.Remove(currentFile);
                    recentFiles.Insert(0, currentFile);
                }
                bOpenProject.ContextMenu = new ContextMenu();
                foreach (string recentFile in recentFiles)
                {
                    if (!string.IsNullOrEmpty(recentFile))
                    {
                        string name = Path.GetFileNameWithoutExtension(recentFile);
                        ContextMenuItem cmi = new ContextMenuItem(name);
                        cmi.DataContext = recentFile;
                        cmi.Click += delegate { OpenProject((string)cmi.DataContext); };
                        bOpenProject.ContextMenu.Items.Add(cmi);
                    }
                }
                File.WriteAllLines(lastFilePath, recentFiles.Take(15).ToArray());
                if (System.Windows.Forms.Screen.AllScreens.Length > 1)
                {
                    this.WindowState = WindowState.Normal;
                    this.Left = System.Windows.Forms.Screen.AllScreens[0].Bounds.X;
                    this.Top = System.Windows.Forms.Screen.AllScreens[0].Bounds.Y;
                    this.Width = System.Windows.Forms.Screen.AllScreens[0].Bounds.Width;
                    this.Height = System.Windows.Forms.Screen.AllScreens[0].Bounds.Height;
                }
            }
#endif
            if (CurrentProject != null)
            {
                VersionManager versionManager = CurrentProject.VersionManager;
                // HACK - TRUPIK: added for simplier debugging, only on my machine.. 
                if (Environment.MachineName.Contains("TRUPIK") && versionManager != null && versionManager.Versions.Count == 2)
                {
                    Version v1 = versionManager.Versions[0];
                    Version v2 = versionManager.Versions[1];

                    ObservableCollection<PSMDiagram> diagramsV1 = versionManager.VersionedProjects[v1].PSMDiagrams;
                    ObservableCollection<PSMDiagram> diagramsV2 = versionManager.VersionedProjects[v2].PSMDiagrams;
                    if (diagramsV1.Count > 0)
                    {
                        DiagramTabManager.AddTab(diagramsV1.Last());
                        propertiesWindow.BindDiagram(ref dockManager);
                    }
                    if (diagramsV2.Count > 0)
                    {
                        DiagramTabManager.AddTab(diagramsV2.Last());
                        propertiesWindow.BindDiagram(ref dockManager);
                    }

                    if (diagramsV1.Count == 0 && diagramsV2.Count == 0)
                    {
                        ObservableCollection<PIMDiagram> pimDiagramsV1 = versionManager.VersionedProjects[v1].PIMDiagrams;
                        ObservableCollection<PIMDiagram> pimDiagramsV2 = versionManager.VersionedProjects[v2].PIMDiagrams;

                        if (pimDiagramsV1.Count > 0)
                        {
                            DiagramTabManager.AddTab(pimDiagramsV1.Last());
                            propertiesWindow.BindDiagram(ref dockManager);
                        }
                        if (pimDiagramsV2.Count > 0)
                        {
                            DiagramTabManager.AddTab(pimDiagramsV2.Last());
                            propertiesWindow.BindDiagram(ref dockManager);
                        }
                    }

                    //DocumentPane.NewVerticalTabGroupCommand.Execute(null, DiagramTabManager.ActivePane);
                    DiagramTabManager.ActivePane.CreateNewVerticalTabGroup();
                    if (diagramsV2.Count > 0)
                    {
                        DiagramTabManager.ActivateDiagram(diagramsV2.Last());
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Min(CurrentProject.Diagrams.Count, 8); i++)
                    {
                        DiagramTabManager.AddTab(CurrentProject.Diagrams[i]);
                        propertiesWindow.BindDiagram(ref dockManager);
                    }
                }
            }
        }

        #endregion

        #region diagrams

        private XCaseCanvas activeDiagram;

        /// <summary>
        /// Actually active canvas.
        /// </summary>
        public XCaseCanvas ActiveDiagram
        {
            get { return activeDiagram; }
            private set
            {
                if (value != null)
                    value.State = ECanvasState.Normal;
                activeDiagram = value;
            }
        }

        /// <summary>
        /// Defines handler header for the <see cref="MainWindow.ActiveDiagramChanged"/> event.
        /// </summary>
        public delegate void ActiveDiagramChangedEventHandler(PanelWindow panelWindow);

        public delegate void ProjectChangingEventHandler(Project oldProject, Project newProject);

        public delegate void ProjectChangedEventHandler();

        /// <summary>
        /// Event invoked when active diagram is changed
        /// </summary>
        public event ActiveDiagramChangedEventHandler ActiveDiagramChanged;

        public event ProjectChangingEventHandler ProjectChanging;

        public event ProjectChangedEventHandler ProjectChanged;

        private void OnProjectChanging(Project oldProject, Project newProject)
        {
            if (oldProject != null)
            {
                oldProject.DiagramAdded -= DiagramTabManager.project_DiagramAdded;
                oldProject.DiagramRemoved -= DiagramTabManager.project_DiagramRemoved;
                oldProject.GetModelController().ExecutedCommand -= MainWindow_ExecutedCommand;
            }
        }

        private void OnProjectChanged()
        {
            if (CurrentProject != null)
            {
                CurrentProject.DiagramAdded += DiagramTabManager.project_DiagramAdded;
                CurrentProject.DiagramRemoved += DiagramTabManager.project_DiagramRemoved;
                CurrentProject.GetModelController().ExecutedCommand += MainWindow_ExecutedCommand;

                bUndo.Command = CurrentProject.GetModelController().UndoCommand;
                bRedo.Command = CurrentProject.GetModelController().RedoCommand;
            }
        }

        private void OnActiveDiagramChanged(PanelWindow panelWindow)
        {
            if (panelWindow != null && panelWindow.xCaseDrawComponent.Canvas.Diagram != null)
            {
                Project diagramProject = panelWindow.xCaseDrawComponent.Canvas.Diagram.Project;
                if (diagramProject != CurrentProject)
                {
                    CurrentProject = diagramProject;
                }
            }

            if (panelWindow != null && panelWindow.xCaseDrawComponent.Canvas.Diagram != null)
            {
                //if (panelWindow.xCaseDrawComponent.Canvas.Diagram is PSMDiagram)
                //    View.TreeLayout.SwitchOff();
                ActiveDiagram = panelWindow.xCaseDrawComponent.Canvas;
                Debug.WriteLine("ActiveDiagramChanged (" + ActiveDiagram.Diagram.Caption + ")");
            }
            else
            {
                ActiveDiagram = null;
                Debug.WriteLine("ActiveDiagramChanged (null)");
            }

            navigatorWindow.ActiveDiagram = ActiveDiagram;
            propertiesWindow.ActiveDiagram = ActiveDiagram;

            InvokeDiagramSelectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            UIElement[] pimMenus = new UIElement[] { /*PIMmenu, AlignmentMenu, PIMDeleteMenu, DeriveMenu,*/ tbPIM};
            UIElement[] psmMenus = new UIElement[] { /*PSMmenuClass, PSMmenuElements, OrderingMenu, PSMDeleteMenu, bXMLSchema, bSampleDocument, ReverseMenu, EvolutionMenu,*/ tbPSM };

            bool showPIMMenus = ActiveDiagram != null && ActiveDiagram.Diagram != null && ActiveDiagram.Diagram is PIMDiagram;
            bool showPSMMenus = ActiveDiagram != null && ActiveDiagram.Diagram != null && ActiveDiagram.Diagram is PSMDiagram;
            foreach (UIElement menu in pimMenus)
            {
                menu.Visibility = showPIMMenus ? Visibility.Visible : Visibility.Collapsed;
            }
            foreach (UIElement menu in psmMenus)
            {
                menu.Visibility = showPSMMenus ? Visibility.Visible : Visibility.Collapsed;
            }

            if (showPIMMenus && Tabs.SelectedItem == tbPSM) Tabs.SelectedItem = tbPIM;
            else if (showPSMMenus && Tabs.SelectedItem == tbPIM) Tabs.SelectedItem = tbPSM;
            else if (ActiveDiagram == null && Tabs.SelectedItem != tbExtensions) Tabs.SelectedItem = tbGeneral;

            if (ActiveDiagram != null)
            {
                ActiveDiagram.SelectedItems.CollectionChanged += navigatorWindow.SelectedItems_CollectionChanged;
            }

        }

        public void InvokeDiagramSelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (DiagramSelectionChanged != null)
            {
                DiagramSelectionChanged(sender, e);
            }
        }

        /// <summary>
        /// Invokes <see cref="ActiveDiagramChanged"/> event.
        /// </summary>
        /// <param name="panelWindow">New active panel window</param>
        internal void InvokeActiveDiagramChanged(PanelWindow panelWindow)
        {
            ActiveDiagramChangedEventHandler temp = ActiveDiagramChanged;
            if (temp != null)
                temp(panelWindow);
        }


        private void InvokeProjectChanging(Project oldProject, Project newProject)
        {
            ProjectChangingEventHandler changing = ProjectChanging;
            if (changing != null) changing(oldProject, newProject);
        }

        internal void InvokeProjectChanged()
        {
            ProjectChangedEventHandler changed = ProjectChanged;
            if (changed != null) changed();
        }

        /// <summary>
        /// Event invoked when selection on canvas is changed
        /// </summary>
        public event NotifyCollectionChangedEventHandler DiagramSelectionChanged;

        public IList<XCaseCanvas> Diagrams
        {
            get
            {
                return (from PanelWindow tab in dockManager.Documents select tab.xCaseDrawComponent.Canvas).ToList();
            }
        }

        #endregion

        #region commands

        public static cmdIncludeClass cmdIncludeClass = new cmdIncludeClass();
        public cmdSelectParent cmdSelectParent;
        public cmdSelectLeftSibling cmdSelectLeftSibling;
        public cmdSelectRightSibling cmdSelectRightSibling;
        public cmdSelectChild cmdSelectChild;
        List<IDraggedConnectionProcessor> connectionCommands;
        IEnumerable<ToggleButton> connectionButtons;

        internal void InitializeMainMenu()
        {
            #region project commands

            bNewPIMDiagram.Command = new cmdNewPIMDiagram(this, bNewPIMDiagram);
            bNewPSMDiagram.Command = new cmdNewPSMDiagram(this, bNewPSMDiagram);
            bNewProject.Command = new cmdNewProject(this, bNewProject);
            bSaveProject.Command = new cmdSaveProject(this, bSaveProjectAs);
            bSaveProjectAs.Command = new cmdSaveProjectAs(this, bSaveProjectAs);
            bOpenProject.Command = new cmdOpenProject(this, bOpenProject);
            bDeleteDiagram.Command = new cmdDeleteDiagram(this, bDeleteDiagram);

            bUndo.Command = CurrentProject.GetModelController().UndoCommand;
            bRedo.Command = CurrentProject.GetModelController().RedoCommand;

            #endregion

            #region delete commands

            bDeleteFromDiagram.Command = new cmdDeleteFromDiagram(this, bDeleteFromDiagram);
            bDeleteFromModel.Command = new cmdDeleteFromModel(this, bDeleteFromModel);
            bDeleteFromPSMDiagram.Command = new cmdDeleteFromPSMDiagram(this, bDeleteFromPSMDiagram);
            bDeleteContainer.Command = new cmdDeleteContainer(this, bDeleteContainer);

            #endregion

            #region alignment commands

            bAlignTop.Command = new cmdAlign(this, bAlignTop, EAlignment.Top);
            bAlignBottom.Command = new cmdAlign(this, bAlignBottom, EAlignment.Bottom);
            bAlignLeft.Command = new cmdAlign(this, bAlignLeft, EAlignment.Left);
            bAlignRight.Command = new cmdAlign(this, bAlignRight, EAlignment.Right);
            bAlignCenterV.Command = new cmdAlign(this, bAlignCenterV, EAlignment.CenterV);
            bAlignCenterH.Command = new cmdAlign(this, bAlignCenterH, EAlignment.CenterH);
            bDistributeVertical.Command = new cmdAlign(this, bDistributeVertical, EAlignment.DistributeV);
            bDistributeHorizontal.Command = new cmdAlign(this, bDistributeHorizontal, EAlignment.DistributeH);

            #endregion

            #region PIM commands

            bClass.Command = new cmdNewClass(this, bClass);
            bCommentary.Command = new cmdComment(this, bCommentary);
            bAssociate.Command = new cmdAssociate(this, bAssociate);
            bPIMAddAttribute.Command = new cmdAddAttribute(this, bPIMAddAttribute);
            bPIMAddOperation.Command = new cmdAddOperation(this, bPIMAddOperation);
            bDeriveNew.Command = new cmdDeriveNew(this, bDeriveNew);
            bDeriveExisting.Command = new cmdDeriveExisting(this, bDeriveExisting);
            connectionCommands = new List<IDraggedConnectionProcessor>();

            bGeneralization.Command = new cmdDragConnection(this, bGeneralization, EDraggedConnectionType.Generalization) { ToggleButtonGroup = connectionCommands };
            bAssociation.Command = new cmdDragConnection(this, bAssociation, EDraggedConnectionType.Association) { ToggleButtonGroup = connectionCommands };
            bAggregation.Command = new cmdDragConnection(this, bAggregation, EDraggedConnectionType.Aggregation) { ToggleButtonGroup = connectionCommands };
            bComposition.Command = new cmdDragConnection(this, bComposition, EDraggedConnectionType.Composition) { ToggleButtonGroup = connectionCommands };

            connectionCommands.Add((IDraggedConnectionProcessor)bGeneralization.Command);
            connectionCommands.Add((IDraggedConnectionProcessor)bAssociation.Command);
            connectionCommands.Add((IDraggedConnectionProcessor)bAggregation.Command);
            connectionCommands.Add((IDraggedConnectionProcessor)bComposition.Command);

            connectionButtons = new[] { bGeneralization, bAssociation, bAggregation, bComposition };

            bAssociationClass.Command = new cmdAssociationClass(this, bAssociationClass);

            // this command is for draging from Navigator window
            CommandBindings.Add(new CommandBinding(cmdIncludeClass, cmdIncludeClass.Executed));

            #endregion

            #region PSM commands

            cmdMoveToLeft cmdMoveToLeft = new cmdMoveToLeft(this, bMoveToLeft);
            cmdMoveToRight cmdMoveToRight = new cmdMoveToRight(this, bMoveToRight);
            cmdMoveToLeft.cmdMoveToRight = cmdMoveToRight;
            cmdMoveToRight.cmdMoveToLeft = cmdMoveToLeft;
            bMoveToLeft.Command = cmdMoveToLeft;
            bMoveToRight.Command = cmdMoveToRight;

            bPSMAddAttribute.Command = new cmdAddPSMAttribute(this, bPSMAddAttribute);
            bMoveOutOfContainer.Command = new cmdMoveComponentOutOfContainer(this, bMoveOutOfContainer);

            bPSMCommentary.Command = new cmdComment(this, bPSMCommentary);
            bAddChildren.Command = new cmdAddChildren(this, bAddChildren);
            bAddAttributes.Command = new cmdAddAttributes(this, bAddAttributes);
            bClassChoice.Command = new cmdIntroduceClassUnion(this, bClassChoice);
            bContentChoice.Command = new cmdIntroduceContentChoice(this, bContentChoice);
            bAttributeContainer.Command = new cmdIntroduceAttributeContainer(this, bAttributeContainer);
            bContentContainer.Command = new cmdIntroduceContentContainer(this, bContentContainer);
            bAddSpecifications.Command = new cmdAddSpecializations(this, bAddSpecifications);
            bXMLSchema.Command = new cmdXmlSchema(this, bXMLSchema);
            bSampleDocument.Command = new cmdSampleDocument(this, bSampleDocument);

            bAddPSMReference.Command = new cmdAddPSMDiagramReference(this, bAddPSMReference);
            bRemoveReference.Command = new cmdRemovePSMDiagramReference(this, bRemoveReference);
            #endregion

            #region PSM tree traversing commands

            cmdSelectParent = new cmdSelectParent(this, null);
            cmdSelectRightSibling = new cmdSelectRightSibling(this, null);
            cmdSelectLeftSibling = new cmdSelectLeftSibling(this, null);
            cmdSelectChild = new cmdSelectChild(this, null);

            #endregion

            #region Reverse Engineering
            bXSDtoPSM.Command = new cmdXSDtoPSM(this, bXSDtoPSM);
            bPSMtoPIM.Command = new cmdPSMtoPIM(this, bPSMtoPIM);
            #endregion

            #region Semantic Web Services Extension
            bOWLtoPIM.Command = new cmdOWLtoPIM(this, bOWLtoPIM);
            bPIMtoOWL.Command = new cmdPIMtoOWL(this, bPIMtoOWL);
            bLiftingXSLT.Command = new cmdLiftingXSLT(this, bLiftingXSLT);
            bLoweringXSLT.Command = new cmdLoweringXSLT(this, bLoweringXSLT);
            #endregion

            #region Evolution

            bFindChanges.Command = new cmdFindChanges(this, bFindChanges);
            bEvolve.Command = new cmdEvolve(this, bEvolve);

            bCreateVersionMappingDiagram.Command = new cmdCreateVersionMappingDiagram(this, bCreateVersionMappingDiagram);
            bMultipleMapping.Command = new cmdMultipleMapping(this, bMultipleMapping);
            bCreateVersionMappingAttribute.Command = new cmdCreateVersionMappingAttribute(this, bCreateVersionMappingAttribute);
            bRemoveVersionMapping.Command = new cmdRemoveVersionMapping(this, bRemoveVersionMapping);
            bLocatePreviousVersion.Command = new cmdLocatePreviousVersion(this, bLocatePreviousVersion);
            bMapDirectly.Command = new cmdMapDirectly(this, bMapDirectly);

            #endregion

            ShortcutActionManager.Actions.Clear();
            ShortcutActionManager.RegisterStandardShortcuts();
        }

        public void OnMenuButtonClick(object sender)
        {
            if (!(sender is ToggleButton) ||
                !connectionButtons.Contains((ToggleButton)sender) && ActiveDiagram.State == ECanvasState.DraggingConnection)
            {
                foreach (ToggleButton connectionButton in connectionButtons)
                {
                    connectionButton.IsChecked = false;
                }
                if (ActiveDiagram != null)
                {
                    ActiveDiagram.State = ECanvasState.Normal;
                }
            }
        }

        void MainWindow_ExecutedCommand(CommandBase command, bool isPartOfMacro, CommandBase macroCommand)
        {
            ActivateDiagramCommand adc = command as ActivateDiagramCommand;
            if (adc != null)
            {
                if (adc.ActivatedDiagram != null && adc.Element != null)
                    DiagramTabManager.ActivateDiagramWithElement(adc.ActivatedDiagram, adc.Element);

                if (adc.ActivatedDiagram == null && adc.Element != null && adc.Element is PIMClass)
                    DiagramTabManager.GoToPimClass((PIMClass)adc.Element);

                if (adc.ActivatedDiagram != null && adc.Element == null)
                    DiagramTabManager.ActivateDiagram(((ActivateDiagramCommand)command).ActivatedDiagram);
            }
        }

        #endregion

        #region registration sets

        public static void InitializeRegistrationSets()
        {
            PIMRepresentantsSet = new RegistrationSet
			                      	{
			                      		new RepresentantRegistration(typeof(AssociationClass), typeof(PIM_AssociationClass), typeof(ClassController), typeof(AssociationClassViewHelper), 6, 0),
			                      		new RepresentantRegistration(typeof(Association), typeof(PIM_Association), typeof(AssociationController), typeof(AssociationViewHelper), 8, 1),
			                      		new RepresentantRegistration(typeof(PIMClass), typeof(PIM_Class), typeof(ClassController), typeof(ClassViewHelper), 5, 2),
			                      		new RepresentantRegistration(typeof(Comment), typeof(XCaseComment), typeof(CommentController), typeof(CommentViewHelper), 15, 10),
			                      		new RepresentantRegistration(typeof(Generalization), typeof(PIM_Generalization), typeof(GeneralizationController), typeof(GeneralizationViewHelper), 10, 10)
			                      	};
            PSMRepresentantsSet = new RegistrationSet
			                      	{
			                      		new RepresentantRegistration(typeof(PSMClass), typeof(PSM_Class), typeof(PSM_ClassController), typeof(PSMElementViewHelper), 1, 10),
			                      		new RepresentantRegistration(typeof(PSMAttributeContainer), typeof(PSM_AttributeContainer), typeof(PSM_AttributeContainerController), typeof(PSMElementViewHelper), 2, 10),
			                      		new RepresentantRegistration(typeof(PSMClassUnion), typeof(PSM_ClassUnion), typeof(PSM_ClassUnionController), typeof(PSMElementViewHelper), 3, 10),
			                      		new RepresentantRegistration(typeof(PSMContentContainer), typeof(PSM_ContentContainer), typeof(PSM_ContentContainerController), typeof(PSMElementViewHelper), 4, 10),
			                      		new RepresentantRegistration(typeof(PSMContentChoice), typeof(PSM_ContentChoice), typeof(PSM_ContentChoiceController), typeof(PSMElementViewHelper), 5, 10),
			                      		new RepresentantRegistration(typeof(Comment), typeof(XCaseComment), typeof(CommentController), typeof(CommentViewHelper), 15, 10),
			                      		new RepresentantRegistration(typeof(PSMAssociation), typeof(PSM_Association), typeof(PSM_AssociationController), typeof(PSMAssociationViewHelper), 12, 10), 
			                      		new RepresentantRegistration(typeof(Generalization), typeof(PIM_Generalization), typeof(GeneralizationController), typeof(GeneralizationViewHelper), 13, 10), 
                                        new RepresentantRegistration(typeof(PSMDiagramReference), typeof(PSM_DiagramReference), typeof(PSM_DiagramReferenceController), typeof(PSMElementViewHelper), 16, 10)
			                      	};
        }

        public static RegistrationSet PIMRepresentantsSet { get; set; }

        public static RegistrationSet PSMRepresentantsSet { get; set; }

        #endregion

        #region Unsaved changes detecting

        /// <summary>
        /// Occurs when undo stack for any PIM/PSM diagram in current project is changed
        /// (a new command is added or removed)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UndoStack_ItemsChanged(object sender, EventArgs e)
        {
            if (!HasUnsavedChanges)
            {
                HasUnsavedChanges = true;
            }
            else
            {
                if (projectsWindow.versionManager == null)
                {
                    if (CurrentProject.GetModelController().getUndoStack().Count == 0)
                    {
                        HasUnsavedChanges = false;
                    }
                }
                else
                {
                    if (projectsWindow.versionManager.VersionedProjects.Values.All(p => p.GetModelController().getUndoStack().Count == 0))
                    {
                        HasUnsavedChanges = false;
                    }
                }
            }
        }

        private bool hasUnsavedChanges = false;

        // Unsaved changes in current XCase project
        public bool HasUnsavedChanges
        {
            get
            {
                return hasUnsavedChanges;
            }
            set
            {
                hasUnsavedChanges = value;
                if (hasUnsavedChanges)
                {
                    if (!Title.EndsWith("*"))
                        Title = Title + "*";
                }
                else
                {
                    if (Title.EndsWith("*"))
                    {
                        if (String.IsNullOrEmpty(CurrentProject.FilePath))
                            Title = "XCase editor - " + CurrentProject.Caption;
                        else
                            Title = "XCase editor - " + CurrentProject.FilePath;
                    }
                }
            }
        }

        #endregion

        #region Handlers

        protected override void OnKeyUp(KeyEventArgs e)
        {
            ShortcutActionManager.PerformActions(e);
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private bool documentPaneLoaded = false;

        private void DocumentPane_Loaded(object sender, RoutedEventArgs e)
        {
            if (documentPaneLoaded) return;
            documentPaneLoaded = true;

#if DEBUG
            logWindowWrap = new DockableContent { Title = "Log window", Name = "logWindow" };
            logWindowWrap.Content = logWindow;
            logWindowWrap.Show(dockManager, AnchorStyle.Right);
            //dockManager.Show(logWindowWrap, DockableContentState.Docked, AnchorStyle.Right);
            //propertiesWindowWrap.BringIntoView();
#endif

            if (File.Exists(LayoutSettingsFile))
            {
                try
                {
                    dockManager.RestoreLayout(LayoutSettingsFile);
                }
                catch (FormatException)
                {
                    
                }
            }

            propertiesWindow.MainWindowRef = this;

            if (filename == null)
            {
#if DEBUG
                if (Environment.MachineName.Contains("TRUPIK") && File.Exists(lastFilePath))
                {
                    List<string> recentFiles = File.ReadAllLines(lastFilePath).ToList();
                    string lastfile = recentFiles[0];
                    if (File.Exists(lastfile))
                    {
                        new cmdNewProject(this, null).Execute();
                        OpenProject(lastfile);
                        filename = null;
                    }
                }
#endif
            }

            if (filename != null)
            {
                OpenProject(filename);
            }
            else if (CurrentProject == null)
            {
                new cmdNewProject(this, null).Execute();
                filename = null;
            }
        }

        private void miResetLayout_Click(object sender, RoutedEventArgs e)
        {
            StringReader sr = new StringReader(Properties.Resources.DefaultLayout);
            dockManager.RestoreLayout(sr);
        }

        private void OnNavigatorSelectedClass(object sender, ClassEventArgs arg)
        {
            if (ActiveDiagram != null)
            {
                XCaseViewBase classView = null;
                if (ActiveDiagram.ElementRepresentations.IsElementPresent(arg.SelectedClass))
                {
                    classView = (PIM_Class)ActiveDiagram.ElementRepresentations[arg.SelectedClass];
                }
                if (classView != null)
                {
                    foreach (ISelectable i in ActiveDiagram.SelectedItems)
                    {
                        i.IsSelected = false;
                    }
                    classView.IsSelected = true;
                    ActiveDiagram.SelectedItems.Clear();
                    ActiveDiagram.SelectedItems.Add(classView);
                }
            }

            // Display the class in the Properties window
            if (arg != null && arg.SelectedClass != null)
                propertiesWindow.DisplayModelClass(arg.SelectedClass);

        }

        static void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ExceptionWindow w = new ExceptionWindow(e.Exception);
            if (w.ShowDialog() == true)
            {
                e.Handled = true;
                Application.Current.Shutdown();
            }
            else
            {
                e.Handled = true;
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (HasUnsavedChanges)
                new cmdCloseProject(this, null).Execute(e);
            else
                CloseProject();
            dockManager.SaveLayout(LayoutSettingsFile);

            CloseProject();
        }

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local
        private void ExitApplication(object sender, EventArgs e)
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Local
        {
            CloseProject();
            Application.Current.Shutdown();
        }

        internal ContentControl GetContentFromTypeString(string type)
        {
            if (type == typeof(PropertiesWindow).ToString())
                return propertiesWindow;
            else if (type == typeof(ProjectsWindow).ToString())
                return projectsWindow;
            else if (type == typeof(NavigatorWindow).ToString())
                return navigatorWindow;
#if DEBUG
            else if (type == typeof(LogWindow.LogWindow).ToString())
                return logWindow;
#endif
            return null;
        }

        #endregion

        #region show/hide panes

        private void ShowDockableContent(DockableContent dockableContent)
        {
            if (dockableContent.State == DockableContentState.Hidden)
            {
                //if (dockableContent. SavedStateAndPosition == null || dockableContent.SavedStateAndPosition.Anchor == AnchorStyle.None)
                //    //dockManager.Show(dockableContent, DockableContentState.FloatingWindow);
                //    dockableContent.ShowAsFloatingWindow(true);
                //else
                dockableContent.Show();
                //dockManager.Show(dockableContent, DockableContentState.Docked);
            }
            else
            {
                dockableContent.Show();
            }
        }

        private void ShowNavigatorWindow(object sender, RoutedEventArgs e)
        {
            ShowDockableContent(navigatorWindowWrap);
        }

        private void ShowProjectsWindow(object sender, RoutedEventArgs e)
        {
            ShowDockableContent(projectsWindowWrap);
        }

        private void ShowPropertiesWindow(object sender, RoutedEventArgs e)
        {
            ShowDockableContent(propertiesWindowWrap);
        }


        #endregion

        public static string XCaseAppDataFolder
        {
            get
            {
                if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XCase")))
                {
                    Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XCase"));
                }
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XCase");
            }
        }

        private static string LayoutSettingsFile
        {
            get
            {
                return Path.Combine(XCaseAppDataFolder, "layout.xml");
            }
        }

#if DEBUG

        #region translation test

        private const string TEST_PATH = @"D:\Programování\XCase\Test\";
        private const string TEST_OUTPUT_PATH = @"D:\Programování\XCase\Test\output\";

        private List<string> testfiles;

        private void testall_Click(object sender, RoutedEventArgs e)
        {
            if (testfiles == null)
                testfiles = new List<string>(Directory.GetFiles(TEST_PATH));
            while (testfiles.Count > 0)
                test_Click(null, null);
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            if (testfiles == null)
                testfiles = new List<string>(Directory.GetFiles(TEST_PATH));

            string file = testfiles.FirstOrDefault();

            if (file != null)
            {
                try
                {
                    cmdOpenProject openCommand = new cmdOpenProject(this, null) { NoDialogs = true, Filename = file };
                    openCommand.Execute();
                    cmdXmlSchema c = (cmdXmlSchema)bXMLSchema.Command;
                    c.Execute();
                    File.WriteAllText(TEST_OUTPUT_PATH + Path.GetFileNameWithoutExtension(file) + ".xsd", c.Window.XMLSchemaText,
                                      System.Text.Encoding.UTF8);
                    c.Window.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error: file {0}, {1}", Path.GetFileNameWithoutExtension(file), ex.Message));
                }
                finally
                {
                    testfiles.Remove(file);
                }
            }
        }

        #endregion
#else 
        private void testall_Click(object sender, RoutedEventArgs e) { }

        private void test_Click(object sender, RoutedEventArgs e) { }
#endif


        private void MainWindow_FileDropped(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFilePaths = (string[])e.Data.GetData(DataFormats.FileDrop, true);

                if (droppedFilePaths.Length > 0)
                {
                    if (droppedFilePaths[0].ToUpper().EndsWith(".XCASE"))
                    {
                        cmdOpenProject open = new cmdOpenProject(this, null) { Filename = droppedFilePaths[0], NoOpenFileDialog = true };
                        open.Execute();
                    }
                    else if (droppedFilePaths[0].ToUpper().EndsWith(".XSD"))
                    {
                        cmdXSDtoPSM schemaImport = new cmdXSDtoPSM(this, null) { Filename = droppedFilePaths[0] };
                        schemaImport.Execute();
                    }

                }
            }
        }


    }
}
