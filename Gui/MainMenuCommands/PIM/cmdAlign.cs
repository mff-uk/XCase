using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Linq;
using XCase.Controller;
using XCase.Model;
using XCase.Controller.Commands;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Variants of alignment
	/// </summary>
	public enum EAlignment
	{
		Top,
		Bottom,
		Right,
		Left,
		CenterV,
		CenterH,
		DistributeV,
		DistributeH
	}

	/// <summary>
	/// Alignes <see cref="IAlignable">alignable</see> controls on canvas
	/// </summary>
	public class cmdAlign : MainMenuCommandBase
	{
		public EAlignment Alignment { get; set; }

		public cmdAlign(MainWindow mainWindow, Control control, EAlignment alignment)
			: base(mainWindow, control)
		{
			Alignment = alignment;

			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
			CommandBase cmd = CreateMoveCommand(ActiveDiagramView.SelectedItems.OfType<IAlignable>(), Alignment, ActiveDiagramView);
			cmd.Execute();

			MainWindow.InvokeDiagramSelectionChanged(null, null);
		}

		public static CommandBase CreateMoveCommand(IEnumerable<IAlignable> affectedObjects, EAlignment alignment, XCaseCanvas diagramView)
		{
			MacroCommand<DiagramController> moveMacroCommand =
				MacroCommandFactory<DiagramController>.Factory().Create(diagramView.Controller);

			double minTop;
			double maxBottom;
			double distance;
			double offset;
			switch (alignment)
			{
				case EAlignment.Top:
					minTop = affectedObjects.Min(item => item.Top);
					foreach (IAlignable element in affectedObjects)
					{
						var cmd = ViewController.CreateMoveCommand(null, minTop, element.ViewHelper, diagramView.Controller);
						moveMacroCommand.Commands.Add(cmd);
					}
					break;
				case EAlignment.Bottom:
					maxBottom = affectedObjects.Max(item => item.Bottom);
					foreach (IAlignable element in affectedObjects)
					{
						var cmd = ViewController.CreateMoveCommand(null, maxBottom - (element.Bottom - element.Top), element.ViewHelper, diagramView.Controller);
						moveMacroCommand.Commands.Add(cmd);
					}
					break;
				case EAlignment.Left:
					double minLeft = affectedObjects.Min(item => item.Left);
					foreach (IAlignable element in affectedObjects)
					{
						var cmd = ViewController.CreateMoveCommand(minLeft, null, element.ViewHelper, diagramView.Controller);
						moveMacroCommand.Commands.Add(cmd);
					}
					break;
				case EAlignment.Right:
					double maxRight = affectedObjects.Max(item => item.Right);
					foreach (IAlignable element in affectedObjects)
					{
						var cmd = ViewController.CreateMoveCommand(maxRight - (element.Right - element.Left), null, element.ViewHelper, diagramView.Controller);
						moveMacroCommand.Commands.Add(cmd);
					}
					break;
				case EAlignment.CenterV:
					double centerH = Math.Round(affectedObjects.Average(item => item.Top + (item.Bottom - item.Top) / 2));
					foreach (IAlignable element in affectedObjects)
					{
						var cmd = ViewController.CreateMoveCommand(null, (centerH - (element.Bottom - element.Top) / 2), element.ViewHelper, diagramView.Controller);
						moveMacroCommand.Commands.Add(cmd);
					} 
					break;
				case EAlignment.CenterH:
					double centerV = Math.Round(affectedObjects.Average(item => item.Left + (item.Right - item.Left) / 2));
					foreach (IAlignable element in affectedObjects)
					{
						var cmd = ViewController.CreateMoveCommand(centerV - (element.Right - element.Left) / 2, null, element.ViewHelper, diagramView.Controller);
						moveMacroCommand.Commands.Add(cmd);
					}
					break;
				case EAlignment.DistributeV:
					minTop = affectedObjects.Min(item => item.Top);
					maxBottom = affectedObjects.Max(item => item.Bottom);
					double sumHeight = affectedObjects.Sum(item => item.Bottom - item.Top);

					distance = Math.Max(0, (maxBottom - minTop - sumHeight) / (affectedObjects.Count() - 1));
					offset = minTop;

					foreach (IAlignable element in affectedObjects.OrderBy(item => item.Top))
					{
						double delta = offset - element.Top;
						var cmd = ViewController.CreateMoveCommand(null, element.Top + delta, element.ViewHelper, diagramView.Controller);
						moveMacroCommand.Commands.Add(cmd);

						offset = offset + element.Bottom - element.Top + distance;
					}

					break;
				case EAlignment.DistributeH:
					minLeft = affectedObjects.Min(item => item.Left);
					maxRight = affectedObjects.Max(item => item.Right);
					double sumWidth = affectedObjects.Sum(item => item.Right - item.Left);

					distance = Math.Max(0, (maxRight - minLeft - sumWidth) / (affectedObjects.Count() - 1));
					offset = minLeft;

					foreach (IAlignable element in affectedObjects.OrderBy(item => item.Left))
					{
						double delta = offset - element.Left;
						var cmd = ViewController.CreateMoveCommand(element.Left + delta, null, element.ViewHelper, diagramView.Controller);
						moveMacroCommand.Commands.Add(cmd);

						offset = offset + element.Right - element.Left + distance;
					}

					break;
			}

			return moveMacroCommand;
		}

		public override bool CanExecute(object parameter)
		{
			if (ActiveDiagramView == null || ActiveDiagramView.SelectedItems.OfType<IAlignable>().Count() == 0)
				return false;
			IEnumerable<double> values = null;
			switch (Alignment)
			{
				case EAlignment.Top:
					values = from IAlignable thumb in ActiveDiagramView.SelectedItems.OfType<IAlignable>()
							 select thumb.Top;
					break;
				case EAlignment.Bottom:
					values = from IAlignable thumb in ActiveDiagramView.SelectedItems.OfType<IAlignable>()
							 select thumb.Bottom;
					break;
				case EAlignment.Left:
					values = from IAlignable thumb in ActiveDiagramView.SelectedItems.OfType<IAlignable>()
							 select thumb.Left;
					break;
				case EAlignment.Right:
					values = from IAlignable thumb in ActiveDiagramView.SelectedItems.OfType<IAlignable>()
							 select thumb.Right;
					break;
				case EAlignment.CenterH:
					values = from IAlignable thumb in ActiveDiagramView.SelectedItems.OfType<IAlignable>()
					         select Math.Round(thumb.Left + (thumb.Right - thumb.Left) / 2);
					break;
				case EAlignment.CenterV:
					values = from IAlignable thumb in ActiveDiagramView.SelectedItems.OfType<IAlignable>()
					         select Math.Round(thumb.Top + (thumb.Bottom - thumb.Top) / 2);
					break;
				case EAlignment.DistributeH:
				case EAlignment.DistributeV:
					return ActiveDiagramView.SelectedItems.OfType<IAlignable>().Count() > 2;
			}
			foreach (var v in values)
			{

			}

			return values.Distinct().Count() > 1;
		}

	}
}
	