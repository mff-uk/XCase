using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using Microsoft.Win32;
using XCase.Model.Serialization;
using XCase.View.Controls;
using XCase.View.Interfaces;
using System.Windows.Input;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Saves current project
	/// </summary>
    public class cmdSaveProject : MainMenuCommandBase
    {
        public cmdSaveProject(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
        {

        }

        public override void Execute(object parameter)
        {
                if (CurrentProject.FilePath.Equals(""))
                {
                    new cmdSaveProjectAs(MainWindow, null).Execute();
                }
                else 
                {
                    // Save only if there are some new unsaved changes
                    if (MainWindow.HasUnsavedChanges)
                    {
                        XmlSerializator serializator = CurrentProject.VersionManager != null ?
							new XmlSerializator(CurrentProject.VersionManager) :
							new XmlSerializator(CurrentProject);

                    	serializator.SerilizeTo(CurrentProject.FilePath);
						MainWindow.HasUnsavedChanges = false;
                    }
                }
            
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }
    }
}