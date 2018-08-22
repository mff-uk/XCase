using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;
using XCase.Gui.MainMenuCommands;

namespace XCase.Gui
{
	public struct Shortcut
	{
		public Key Key { get; set; }
		public ModifierKeys Modifier { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public Shortcut(Key key, ModifierKeys modifier)
			: this()
		{
			Key = key;
			Modifier = modifier;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public Shortcut(Key key)
			: this()
		{
			Key = key;
			Modifier = ModifierKeys.None;
		}

		public bool SetBusyState { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is Shortcut)
			{
				Shortcut shortcut = (Shortcut)obj;
				return this.Key == shortcut.Key && this.Modifier == shortcut.Modifier;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return Key.GetHashCode() * Modifier.GetHashCode();
		}
	}


	public class ShortcutActionManager
	{
		public struct ShortcutAction
		{
			public Action directAction { get; set; }

			public ICommand command { get; set; }

			public void Perform()
			{
				if (!SetBusyState)
				{
					if (directAction != null)
					{
						directAction();
					}
					if (command != null && command.CanExecute(null))
					{
						command.Execute(null);
					}
				}
				else
				{
					try
					{
						BusyState.SetBusy();
						if (directAction != null)
						{
							directAction();
						}
						if (command != null && command.CanExecute(null))
						{
							command.Execute(null);
						}
					}
					finally
					{
						BusyState.SetNormalState();
					}
				}
			}

			public bool SetBusyState { get; set; }
		}

		private readonly Dictionary<Shortcut, ShortcutAction> actions = new Dictionary<Shortcut, ShortcutAction>();

		public Dictionary<Shortcut, ShortcutAction> Actions
		{
			get
			{
				return actions;
			}
		}

		public MainWindow Window { get; private set; }

		public ShortcutActionManager(MainWindow window)
		{
			Window = window;
		}

		public XCaseCanvas ActiveDiagram
		{
			get
			{
				return Window.ActiveDiagram;
			}
		}

		public void AboutAction()
		{
			AboutWindow a = new AboutWindow();
			a.ShowDialog();
		}

		public void DeleteAction()
		{
			if (ActiveDiagram.Diagram is PIMDiagram)
			{
				if (Window.bDeleteFromDiagram.Command.CanExecute(null)) Window.bDeleteFromDiagram.Command.Execute(null);
			}
			else if (ActiveDiagram.Diagram is PSMDiagram)
			{
				if (Window.bDeleteFromPSMDiagram.Command.CanExecute(null)) Window.bDeleteFromPSMDiagram.Command.Execute(null);
			}
		}

		public void ShiftDeleteAction()
		{
			if (Window.bDeleteFromModel.IsVisible && Window.bDeleteFromModel.IsEnabled)
			{
				if (Window.bDeleteFromModel.Command.CanExecute(null)) Window.bDeleteFromModel.Command.Execute(null);
			}
			else if (Window.bDeleteFromPSMDiagram.IsVisible && Window.bDeleteFromPSMDiagram.IsEnabled)
			{
				if (Window.bDeleteFromPSMDiagram.Command.CanExecute(null))
					Window.bDeleteFromPSMDiagram.Command.Execute(true);
			}
		}

		public void RenameAction()
		{
			if (ActiveDiagram != null &&
				ActiveDiagram.SelectedItems.Any(selected => selected is IF2Renamable))
				((IF2Renamable)ActiveDiagram.SelectedItems.First(selected => selected is IF2Renamable)).F2Rename();
		}

#if DEBUG
		private void DebugPrint(Key key)
		{

			string m = String.Empty;
			if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				m = m + "Ctrl + ";
			}
			if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
			{
				m = m + "Alt + ";
			}
			if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
			{
				m = m + "Shift + ";
			}
			if ((Keyboard.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
			{
				m = m + "Win + ";
			}
			m = m + key;
			System.Diagnostics.Debug.WriteLine(string.Format("KEY: {0}", m));

		}
#endif

		#region register action overloads
		private void RegisterAliases(IEnumerable<Shortcut> shortcuts, ShortcutAction action)
		{
			foreach (Shortcut shortcut in shortcuts)
			{
				RegisterAction(shortcut, action);
			}
		}

		public void RegisterAction(Shortcut shortcut, Button commandButton, params Shortcut[] aliasShortcuts)
		{
			ShortcutAction action = new ShortcutAction { command = commandButton.Command };
			RegisterAction(shortcut, action);
			RegisterAliases(aliasShortcuts, action);
		}

		public void RegisterAction(Shortcut shortcut, Action action, params Shortcut[] aliasShortcuts)
		{
			ShortcutAction _action = new ShortcutAction { directAction = action };
			RegisterAction(shortcut, _action);
			RegisterAliases(aliasShortcuts, _action);
		}

		public void RegisterAction(Shortcut shortcut, ICommand command, params Shortcut[] aliasShortcuts)
		{
			ShortcutAction _action = new ShortcutAction { command = command };
			RegisterAction(shortcut, _action);
			RegisterAliases(aliasShortcuts, _action);
		}

		public void RegisterAction(Shortcut shortcut, ShortcutAction action, params Shortcut[] aliasShortcuts)
		{
			action.SetBusyState = shortcut.SetBusyState;
			Actions[shortcut] = action;
			RegisterAliases(aliasShortcuts, action);
		}
		#endregion 

		public void PerformActions(KeyEventArgs keyEventArgs)
		{
			#if DEBUG
			DebugPrint(keyEventArgs.Key);
			#endif
			Shortcut c = new Shortcut(keyEventArgs.Key, Keyboard.Modifiers);
			ShortcutAction action;
			if (Actions.TryGetValue(c, out action))
			{
				action.Perform();
			    keyEventArgs.Handled = true;
			}
		}

		public void RegisterStandardShortcuts()
		{
			RegisterAction(new Shortcut(Key.F1), AboutAction);
			RegisterAction(new Shortcut(Key.F2), RenameAction);
			RegisterAction(new Shortcut(Key.Delete), DeleteAction);
			RegisterAction(new Shortcut(Key.Up), Window.cmdSelectParent);
			RegisterAction(new Shortcut(Key.Left), Window.cmdSelectLeftSibling);
			RegisterAction(new Shortcut(Key.Right), Window.cmdSelectRightSibling);
			RegisterAction(new Shortcut(Key.Down), Window.cmdSelectChild);

            RegisterAction(new Shortcut(Key.Up, ModifierKeys.Shift | ModifierKeys.Control), new cmdAlignOne(Window, null, EAlignment.Top));
            RegisterAction(new Shortcut(Key.Left, ModifierKeys.Shift | ModifierKeys.Control), new cmdAlignOne(Window, null, EAlignment.Left));
            RegisterAction(new Shortcut(Key.Right, ModifierKeys.Shift | ModifierKeys.Control), new cmdAlignOne(Window, null, EAlignment.Right));
            RegisterAction(new Shortcut(Key.Down, ModifierKeys.Shift | ModifierKeys.Control), new cmdAlignOne(Window, null, EAlignment.Bottom));

			RegisterAction(new Shortcut(Key.W, ModifierKeys.Control) { SetBusyState = true }, Window.DiagramTabManager.RemoveActiveTab, new Shortcut(Key.F4, ModifierKeys.Control) { SetBusyState = true });
			RegisterAction(new Shortcut(Key.Z, ModifierKeys.Control), Window.bUndo);
			RegisterAction(new Shortcut(Key.Y, ModifierKeys.Control), Window.bRedo);
			RegisterAction(new Shortcut(Key.O, ModifierKeys.Control), Window.bOpenProject);
			RegisterAction(new Shortcut(Key.S, ModifierKeys.Control), Window.bSaveProject);
			RegisterAction(new Shortcut(Key.N, ModifierKeys.Control), Window.bNewProject);
            RegisterAction(new Shortcut(Key.C, ModifierKeys.Control), delegate { if (ActiveDiagram != null) ClipboardManager.PutToClipboard(ActiveDiagram); });
            RegisterAction(new Shortcut(Key.V, ModifierKeys.Control) { SetBusyState = true }, delegate { if (ActiveDiagram != null) ClipboardManager.PasteContentToDiagram(ActiveDiagram, ActiveDiagram.Diagram is PIMDiagram ? MainWindow.PIMRepresentantsSet : MainWindow.PSMRepresentantsSet); });

			RegisterAction(new Shortcut(Key.Delete, ModifierKeys.Shift), ShiftDeleteAction);
	
			RegisterAction(new Shortcut(Key.S, ModifierKeys.Shift | ModifierKeys.Control), Window.bSaveProjectAs);

            RegisterAction(new Shortcut(Key.D1, ModifierKeys.Control), delegate { BringTabToForeground(0); });
            RegisterAction(new Shortcut(Key.NumPad1, ModifierKeys.Control), delegate { BringTabToForeground(0); });

            RegisterAction(new Shortcut(Key.D2, ModifierKeys.Control), delegate { BringTabToForeground(1); });
            RegisterAction(new Shortcut(Key.NumPad2, ModifierKeys.Control), delegate { BringTabToForeground(1); });

            RegisterAction(new Shortcut(Key.D3, ModifierKeys.Control), delegate { BringTabToForeground(2); });
            RegisterAction(new Shortcut(Key.NumPad3, ModifierKeys.Control), delegate { BringTabToForeground(2); });

            RegisterAction(new Shortcut(Key.D4, ModifierKeys.Control), delegate { BringTabToForeground(3); });
            RegisterAction(new Shortcut(Key.NumPad4, ModifierKeys.Control), delegate { BringTabToForeground(3); });
		}

        private void BringTabToForeground(int tabIndex)
        {
            List<TabItem> visibleItems = Window.Tabs.Items.Cast<TabItem>().Where(t => t.Visibility == Visibility.Visible).ToList();
            if (tabIndex < visibleItems.Count)
            {
                Window.Tabs.SelectedItem = visibleItems[tabIndex];
            }
        }
	}
}