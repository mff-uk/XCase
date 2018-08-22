using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ProjectConverter;
using XCase.Model;
using Version=XCase.Model.Version;


namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Opens another project. 
	/// </summary>
	public class cmdOpenProject : MainMenuCommandBase
	{
		/// <summary>
		/// When set to true, no dialog windows requesting saving unsaved changes are displayed to the user.
		/// </summary>
		public bool NoDialogs { get; set; }
		
        public bool NoOpenFileDialog { get; set; }

		/// <summary>
		/// File containing saved project
		/// </summary>
		public string Filename { get; set; }



		public cmdOpenProject(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{

		}

		/// <summary>
		/// Opens a project from a <see cref="Filename">file</see>.
		/// </summary>
		/// <param name="parameter">ignored</param>
		/// <exception cref="Exception">When exception occurs, new project is created and exception is rethrown.</exception>
		public override void Execute(object parameter)
		{
			OpenFileDialog dlg = new OpenFileDialog
										{
											DefaultExt = ".XCase",
											Filter = "XCase files (*.XCase)|*.XCase|XML files (*.xml)|*.xml|All files (*.*)|*.*||"
										};

			bool? result = null;

            if (!NoDialogs && !NoOpenFileDialog)
			{
				result = dlg.ShowDialog();
			}
			else
			{
				dlg.FileName = Filename;
			}

            if (NoDialogs || NoOpenFileDialog || result == true)
			{
				String msg = string.Empty;

				XmlDeserializatorBase deserializator;
				if (XmlDeserializatorVersions.UsesVersions(dlg.FileName))
				{
					deserializator = new XmlDeserializatorVersions();
				}
				else
				{
					deserializator = new XmlDeserializator();
				}

				// First, validates if the file is a valid XCase XML file
				// TODO: it would be better to have two separate schemas rather than one choice schema 
				if (!deserializator.ValidateXML(dlg.FileName, ref msg))
				{
					Dialogs.ErrMsgBox.Show("File cannot be opened", "Not a valid XCase XML file");
					return;
				}

                // version check
			    string v1;
			    string v2;
			    if (!XmlDeserializatorBase.VersionsEqual(dlg.FileName, out v1, out v2))
			    {
                    fProjectConverter projectConverter = new fProjectConverter();
			        
			        if (projectConverter.CanConvert(v1, v2))
			        {
                        MessageBoxResult yn = XCaseYesNoBox.Show("Project is obsolete. ", "Project is obsolete and must be converted to a new version before opening. \r\nDo you want to convert it now? ");
                        if (yn == MessageBoxResult.Yes)
                        {
                            projectConverter.SetFile(dlg.FileName);
                            projectConverter.DialogMode = true;
                            projectConverter.ShowDialog();
                        }
			        }
                    else
			        {
                        Dialogs.ErrMsgBox.Show(string.Format("Can not open file {0}. Project version is {1}, needed version is {2}.", dlg.FileName, v1, v2), "");
			        }
                    if (!XmlDeserializatorBase.VersionsEqual(dlg.FileName, out v1, out v2))
                    {
                        Dialogs.ErrMsgBox.Show(string.Format("Can not open file {0}. Project version is {1}, needed version is {2}. \r\nUse project converter to convert the file to new version.",  dlg.FileName, v1, v2), "");
                        return;
                    }
			    }

				// Closes existing project
				if (CurrentProject != null)
				{
					// Save existing project if there are some unsaved changes
					if (MainWindow.HasUnsavedChanges && !NoDialogs)
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
				//try
				{
					BusyState.SetBusy();

					if (deserializator is XmlDeserializator)
					{
						Project p = deserializator.RestoreProject(dlg.FileName);
						p.FilePath = dlg.FileName;
						p.CreateModelController();
						// HACK (SEVERE) - this should be somewhere else ...
						p.GetModelController().getUndoStack().ItemsChanged += MainWindow.UndoStack_ItemsChanged;
						MainWindow.CurrentProject = p;
						MainWindow.Title = "XCase editor - " + p.FilePath;
						MainWindow.HasUnsavedChanges = false;


						//It is this way so that when the CurrentProject is set, ModelController is present
						MainWindow.projectsWindow.BindToProject(CurrentProject);
						MainWindow.navigatorWindow.BindToProject(CurrentProject);
						MainWindow.propertiesWindow.BindDiagram(ref MainWindow.dockManager);
						MainWindow.InitializeMainMenu();
						MainWindow.HasUnsavedChanges = false;
						MainWindow.OpenProjectDiagrams();
					}
					else
					{
						VersionManager versionManager = ((XmlDeserializatorVersions)deserializator).RestoreVersionedProject(dlg.FileName);
						versionManager.FilePath = dlg.FileName;
						foreach (Project project in versionManager.VersionedProjects.Values)
						{
							project.FilePath = dlg.FileName;
							project.CreateModelController();
						}
						MainWindow.projectsWindow.BindToVersionManager(versionManager);
					    Project latestProjectVersion = versionManager.LatestVersion;
					    MainWindow.projectsWindow.SwitchToVersion(latestProjectVersion.Version);
                        MainWindow.navigatorWindow.BindToProject(latestProjectVersion);
                        MainWindow.InitializeMainMenu();
                        MainWindow.OpenProjectDiagrams();
                        MainWindow.HasUnsavedChanges = false;
                        MainWindow.Title = "XCase editor - " + versionManager.FilePath;

                        #if DEBUG
					    foreach (KeyValuePair<Version, Project> kvp in versionManager.VersionedProjects)
					    {
					        Tests.ModelIntegrity.ModelConsistency.CheckEverything(kvp.Value);    
					    }
                        Tests.ModelIntegrity.VersionsConsistency.CheckVersionsConsistency(versionManager);
                        #endif
					}
				}
				//catch
				//{
				//    new cmdNewProject(MainWindow, null).Execute();
				//    throw;
				//}
				//finally
				//{
				//    BusyState.SetNormalState();
				//}
			}
		}

		public override bool CanExecute(object parameter)
		{
			return true;
		}
	}
}