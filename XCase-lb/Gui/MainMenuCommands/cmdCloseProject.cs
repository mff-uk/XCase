using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Cloess current project
	/// </summary>
	public class cmdCloseProject : MainMenuCommandBase
	{
		public cmdCloseProject(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
		}

		public override void Execute(object parameter)
		{
          // Save existing project before closing (if there are some unsaved changes)
          if (MainWindow.HasUnsavedChanges)
          {
              MessageBoxResult r =
              XCaseYesNoBox.Show("Current project is not saved", "Do you want to save it?");

              if (r == MessageBoxResult.Yes)
                  new cmdSaveProject(MainWindow, null).Execute();
              else
                  if (r == MessageBoxResult.Cancel)
                  {
                      if (parameter is System.ComponentModel.CancelEventArgs)
                        ((System.ComponentModel.CancelEventArgs)parameter).Cancel = true;
                      return;
                  }
          }

			MainWindow.CloseProject();


		}

		public override bool CanExecute(object parameter)
		{
			return true;
		}
	}
}