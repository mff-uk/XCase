using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XCase.Controller;
using XCase.View.Controls;
using XCase.Model;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Parent class for toolbar commands.
	/// </summary>
	public abstract class MainMenuCommandBase : ICommand
	{
		/// <summary>
		/// Control associated with the command
		/// </summary>
		/// <value><see cref="Control"/></value>
		public Control Control { get; set; }

		/// <summary>
		/// Reference to the main window. 
		/// </summary>
		/// <value><see cref="MainWindow"/></value>
		public MainWindow MainWindow { get; private set; }

		/// <summary>
		/// Reference to the active panel window.
		/// </summary>
		/// <value><see cref="PanelWindow"/></value>
		public PanelWindow ActivePanelWindow
		{
			get
			{
				return (PanelWindow)MainWindow.dockManager.ActiveDocument;
			}
		}

		/// <summary>
		/// Reference to the active diagram. 
		/// </summary>
		/// <value><see cref="XCaseCanvas"/></value>
		public XCaseCanvas ActiveDiagramView
		{
			get
			{
				return MainWindow.ActiveDiagram;
			}
		}

		/// <summary>
		/// Reference to the current project.
		/// </summary>
		/// <value><see cref="Project"/></value>
		public Project CurrentProject
		{
			get
			{
				return MainWindow.CurrentProject;
			}
		}

		/// <summary>
		/// Reference to the current project's model controller. 
		/// </summary>
		/// <value></value>
		public ModelController ModelController
		{
			get
			{
				return CurrentProject.GetModelController();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MainMenuCommandBase"/> class.
		/// </summary>
		/// <param name="mainWindow">The main window.</param>
		/// <param name="control">The control that launches this command.</param>
		protected MainMenuCommandBase(MainWindow mainWindow, Control control)
		{
			MainWindow = mainWindow;
			Control = control;

			Button b = control as Button;
			if (b != null)
			{
				b.Click += delegate { MainWindow.OnMenuButtonClick(b); };
			}
		}

		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		public abstract void Execute(object parameter);

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		public virtual void Execute()
		{
			Execute(null);
		}

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		/// <returns>
		/// true if this command can be executed; otherwise, false.
		/// </returns>
		public abstract bool CanExecute(object parameter);

		/// <summary>
		/// Raises the <see cref="CanExecuteChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		public virtual void OnCanExecuteChanged(EventArgs e)
		{
			EventHandler canExecuteChangedHandler = CanExecuteChanged;
			if (canExecuteChangedHandler != null)
			{
				canExecuteChangedHandler(this, e);
			}
		}
	}
}