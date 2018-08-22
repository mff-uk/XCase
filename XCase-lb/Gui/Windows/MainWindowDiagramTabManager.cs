using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Linq;
using AvalonDock;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;
using XCase.Controller;
using XCase.Controller.Dialogs;

namespace XCase.Gui
{
	public class MainWindowDiagramTabManager
	{
		public MainWindow MainWindow
		{
			get;
			private set;
		}

		private DockingManager dockManager
		{
			get
			{
				return MainWindow.dockManager;
			}
		}

        public DocumentPane ActivePane
        {
            get
            {
                if (dockManager.ActiveDocument != null && dockManager.ActiveDocument.Parent != null
                    && dockManager.ActiveDocument.Parent is DocumentPane)
                {
                    return (DocumentPane) dockManager.ActiveDocument.Parent;
                }
                else
                {
                    return dockManager.MainDocumentPane;
                }
            }
        }

        public List<DocumentFloatingWindow> CreatedFloatingWindows
        {
            get; set;
        }

	    /// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public MainWindowDiagramTabManager(MainWindow mainWindow)
		{
			MainWindow = mainWindow;
            CreatedFloatingWindows = new List<DocumentFloatingWindow>();

			dockManager.ActiveDocumentChanged += DockManager_ActiveTabChanged;
		}

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MainWindow.InvokeDiagramSelectionChanged(sender, e);
        }

		/// <summary>
		/// Activates a diagram
		/// </summary>
		/// <param name="diagram">Diagram to be activated</param>
        public PanelWindow ActivateDiagram(Diagram diagram)
		{
			PanelWindow Tab = FindTab(diagram);
			if (Tab == null)
			{
				if (diagram is PSMDiagram) View.TreeLayout.SwitchOff();
				Tab = AddTab(diagram);
				MainWindow.propertiesWindow.BindDiagram(ref MainWindow.dockManager);
			}
			else
			{
			    dockManager.ActiveDocument = Tab;
				Tab.xCaseDrawComponent.Canvas.SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
			}

            ActivePane.BringHeaderToFront(Tab);
		    return Tab;
		}

        public void GoToPimClass(PIMClass pimClass)
        {
            ElementDiagramDependencies dependentDiagrams = ElementDiagramDependencies.FindElementDiagramDependencies(MainWindow.CurrentProject, new[] { pimClass }, null);

            if (dependentDiagrams.Count == 1)
            {
                if (dependentDiagrams[pimClass].Count == 1)
                {
                    ActivateDiagramWithElement(dependentDiagrams[pimClass][0], pimClass);
                }
                else
                {
                    SelectItemsDialog d = new SelectItemsDialog();
                    d.ToStringAction = diagram => ((Diagram) diagram).Caption;
                    d.SetItems(dependentDiagrams[pimClass]);
                    if (d.ShowDialog() == true)
                    {
                        foreach (Diagram diagram in d.selectedObjects.Cast<Diagram>())
                        {
                            ActivateDiagramWithElement(diagram, pimClass);
                        }
                    }
                }
            }
            else if (dependentDiagrams.Count == 0)
            {
                XCaseYesNoBox.ShowOK("PIM class not used in diagrams", "PIM class is not used in any diagram. You can edit it via Navigator window. ");
            }
        }

		/// <summary>
		/// Activates given diagram and selects given element on it (used from Properties window)
		/// </summary>
		/// <param name="diagram"></param>
        /// <param name="selectedElement"></param>
		public void ActivateDiagramWithElement(Diagram diagram, Element selectedElement)
		{
		    PanelWindow tab = ActivateDiagram(diagram);
			tab.xCaseDrawComponent.Canvas.SelectedItems.SetSelection();

            if (selectedElement != null)
			{
                IModelElementRepresentant r = tab.xCaseDrawComponent.Canvas.ElementRepresentations[selectedElement];
			    if (r != null && r is ISelectable)
					tab.xCaseDrawComponent.Canvas.SelectedItems.SetSelection((ISelectable) r);
			}

			MainWindow.InvokeActiveDiagramChanged(tab);
		}

		/// <summary>
		/// Finds among currently open PanelWindows the one which is associated with given <paramref name="diag"/>
		/// <param name="diag">Diagram whose tab has to be found</param>
		/// </summary>
		private PanelWindow FindTab(Diagram diag)
		{
			foreach (PanelWindow tab in MainWindow.dockManager.Documents)
			{
				if (tab.Diagram == diag)
				{
					return tab;
				}
			}
			return null;
		}

		/// <summary>
		/// Adds a new tab to the tab panel, associated with given <paramref name="diagram"/>
		/// <param name="diagram">Diagram to which the new tab should be bound</param>
		/// </summary>
		internal PanelWindow AddTab(Diagram diagram)
		{
			PanelWindow newTab = new PanelWindow();
			if (diagram is PIMDiagram)
				newTab.xCaseDrawComponent.Canvas.InitializeRegistrationSet(MainWindow.PIMRepresentantsSet);
			if (diagram is PSMDiagram)
				newTab.xCaseDrawComponent.Canvas.InitializeRegistrationSet(MainWindow.PSMRepresentantsSet);
            
            newTab.BindToDiagram(diagram, diagram.Project.GetModelController());
            newTab.xCaseDrawComponent.Canvas.SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

		    ActivePane.Items.Add(newTab);
            ActivePane.UpdateLayout();
            ActivePane.BringHeaderToFront(newTab);
		    dockManager.ActiveDocument = newTab;
            #if DEBUG
		    newTab.BindToLogWIndow(MainWindow.logWindow);
            #endif
			return newTab;
		}

		/// <summary>
		/// Closes active PanelWindow
		/// </summary>
		internal void RemoveActiveTab()
		{
			if (dockManager.ActiveDocument != null)
			{
			    int index = dockManager.MainDocumentPane.Items.IndexOf(dockManager.ActiveDocument);
                PanelWindow pw = dockManager.ActiveDocument as PanelWindow;
                if (pw != null)
                    RemoveTab(pw);
                else
                {
                    (dockManager.ActiveDocument as DocumentContent).Close();
                }
			}
		}

		/// <summary>
		/// Closes given PanelWindow
		/// </summary>
		/// <param name="tab">PanelWindow to be closed</param>
		internal void RemoveTab(PanelWindow tab)
		{
			if (dockManager.ActiveDocument == tab)
			{
				MainWindow.ActiveDiagram.Unbind();
			}
		    tab.Close();
            if (dockManager.ActiveDocument == null && dockManager.Documents.Count() > 0)
            {
                dockManager.ActiveDocument = dockManager.Documents.Last();
            }
			//InvokeActiveDiagramChanged(dockManager.ActiveDocument as PanelWindow);
		}

		/// <summary>
		/// Handles double click on a diagram in Projects window - activates/reopens the tab with selected diagram
		/// </summary>
		internal void DiagramDoubleClick(object sender, DiagramDClickArgs arg)
		{
			ActivateDiagram(arg.Diagram);
		}

		/// <summary>
		/// Handles diagram removing event invoked by Projects window
		/// </summary>
		internal void DiagramRemoveHandler(object sender, DiagramDClickArgs arg)
		{
			if (arg.Diagram is PIMDiagram)
			{
				RemoveDiagramCommand removeDiagramCommand = (RemoveDiagramCommand)RemoveDiagramCommandFactory.Factory().Create(arg.Diagram.Project.GetModelController());
				removeDiagramCommand.Set(arg.Diagram.Project, arg.Diagram);
				removeDiagramCommand.Execute();
			}
			else if (arg.Diagram is PSMDiagram)
			{
				PanelWindow Tab = FindTab(arg.Diagram);
				if (Tab != null)
				{
					RemovePSMDiagramMacroCommand c = (RemovePSMDiagramMacroCommand)RemovePSMDiagramMacroCommandFactory.Factory().Create(arg.Diagram.Project.GetModelController());
					c.Set(arg.Diagram.Project, arg.Diagram as PSMDiagram, Tab.xCaseDrawComponent.Canvas.Controller);
					if (c.Commands.Count > 0) c.Execute();
				}
                else
				{
                    RemovePSMDiagramMacroCommand c = (RemovePSMDiagramMacroCommand)RemovePSMDiagramMacroCommandFactory.Factory().Create(MainWindow.CurrentProject.GetModelController());
                    c.Set(arg.Diagram.Project, arg.Diagram as PSMDiagram, new DiagramController(arg.Diagram as PSMDiagram, MainWindow.CurrentProject.GetModelController()));
                    if (c.Commands.Count > 0) c.Execute();
				}

			}
			else throw new NotImplementedException("Unknown diagram type");
		}

		/// <summary>
		/// Handles diagram renaming event invoked by Projects window
		/// </summary>
		internal void DiagramRenameHandler(object sender, DiagramRenameArgs arg)
		{
			PanelWindow tab = FindTab(arg.Diagram);
			tab.RenameDiagram(arg.NewCaption);
		}

		/// <summary>
		/// Reaction on change of active document.
		/// Invokes ActiveDiagramChanged event.
		/// </summary>
		/// <param name="content">New active document</param>
		private void DockManager_ActiveTabChanged(DocumentContent content)
		{
			//if (content is PanelWindow || content == null)
			{
				MainWindow.InvokeActiveDiagramChanged(content as PanelWindow);
			}
		}
        
		internal void project_DiagramRemoved(object sender, DiagramEventArgs e)
		{
			PanelWindow tab = FindTab(e.Diagram);
			if (tab != null)
			{
				RemoveTab(tab);
			}
		}

		internal void project_DiagramAdded(object sender, DiagramEventArgs e)
		{
			AddTab(e.Diagram);
			MainWindow.propertiesWindow.BindDiagram(ref MainWindow.dockManager);
		}
	}
}