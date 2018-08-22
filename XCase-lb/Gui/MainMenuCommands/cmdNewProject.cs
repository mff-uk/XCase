using System.Windows;
using System.Windows.Controls;
using XCase.Model;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Creates new project
	/// </summary>
	public class cmdNewProject : MainMenuCommandBase
	{
		public cmdNewProject(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{

		}

		public override void Execute(object parameter)
		{
            // Closes existing project
            if (CurrentProject != null)
            {
                // Save existing project if there are some unsaved changes
                if (MainWindow.HasUnsavedChanges)
                {
                    MessageBoxResult r =
                        XCaseYesNoBox.Show("Current project is not saved", "Do you want to save it?");
                    if (r == MessageBoxResult.Yes)
                        new cmdSaveProject(MainWindow, null).Execute();
                    else
                     if (r == MessageBoxResult.Cancel)
                        return;
                        
                }
                //new cmdCloseProject(MainWindow, null).Execute();
                MainWindow.CloseProject();
            }

            // Creates new XCase project but with no diagrams
			XmlDeserializator deserializator = new XmlDeserializator();
			Project p = deserializator.CreateEmptyProject();

            p.CreateModelController();
			// HACK (SEVERE): this should be somewhere else!
			p.GetModelController().getUndoStack().ItemsChanged += MainWindow.UndoStack_ItemsChanged;
            //It is this way so that when the CurrentProject is set, ModelController is present
            MainWindow.CurrentProject = p;
           
            MainWindow.projectsWindow.BindToProject(CurrentProject);
            MainWindow.navigatorWindow.BindToProject(CurrentProject);
            MainWindow.propertiesWindow.BindDiagram(ref MainWindow.dockManager);
            MainWindow.InitializeMainMenu();
			MainWindow.OpenProjectDiagrams();

            MainWindow.Title = "XCase editor - Project1";
            MainWindow.HasUnsavedChanges = false;        
		}

		public override bool CanExecute(object parameter)
		{
			return true;
		}
	}
}