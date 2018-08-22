using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
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
	/// Alignes <see cref="IAlignable">alignable</see> controls on canvas
	/// </summary>
	public class cmdAlignOne : MainMenuCommandBase
	{
		public EAlignment Alignment { get; set; }

        public cmdAlignOne(MainWindow mainWindow, Control control, EAlignment alignment)
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

        private static Type[] ignoredTypes = new Type[] { typeof(AssociationLabel) };

		public static CommandBase CreateMoveCommand(IEnumerable<IAlignable> affectedObjects, EAlignment alignment, XCaseCanvas diagramView)
		{
			MacroCommand<DiagramController> moveMacroCommand =
				MacroCommandFactory<DiagramController>.Factory().Create(diagramView.Controller);

		    List<Point> aidsPointsForDiagram = VisualAidsAdorner.GetVisualAidsPointsForDiagram(diagramView, ignoredTypes);

		    double minTop, nextTop;
			double maxBottom, nextBottom;
            double minLeft, nextLeft;
            double maxRight, nextRight;
		    double delta;

			switch (alignment)
			{
				case EAlignment.Top:
					minTop = affectedObjects.Min(item => item.Top);
                    if (minTop > 0)
                    {
                        nextTop = aidsPointsForDiagram.Where(point => point.Y < minTop).Max(point => point.Y);
                        delta = nextTop - minTop;
                        foreach (IAlignable element in affectedObjects)
                        {
                            var cmd = ViewController.CreateMoveCommand(null, element.Top + delta, element.ViewHelper,
                                                                       diagramView.Controller);
                            moveMacroCommand.Commands.Add(cmd);
                        }
                    }
			        break;
				case EAlignment.Bottom:
					maxBottom = affectedObjects.Max(item => item.Bottom);
			        IEnumerable<Point> candidatesBottom = aidsPointsForDiagram.Where(point => point.Y > maxBottom);
                    if (candidatesBottom.Count() > 0)
                    {
                        nextBottom = candidatesBottom.Min(point => point.Y);
                        delta = nextBottom - maxBottom;
                        foreach (IAlignable element in affectedObjects)
                        {
                            var cmd = ViewController.CreateMoveCommand(null, element.Top + delta, element.ViewHelper, diagramView.Controller);
                            moveMacroCommand.Commands.Add(cmd);
                        }
                    }
			        break;
				case EAlignment.Left:
			        minLeft = affectedObjects.Min(item => item.Left);
                    if (minLeft > 0)
                    {
                        nextLeft = aidsPointsForDiagram.Where(point => point.X < minLeft).Max(point => point.X);
                        delta = nextLeft - minLeft;
                        foreach (IAlignable element in affectedObjects)
                        {
                            var cmd = ViewController.CreateMoveCommand(element.Left + delta, null, element.ViewHelper, diagramView.Controller);
                            moveMacroCommand.Commands.Add(cmd);
                        }
                    }
					break;
				case EAlignment.Right:
                    maxRight = affectedObjects.Max(item => item.Right);
                    IEnumerable<Point> candidatesRight = aidsPointsForDiagram.Where(point => point.X > maxRight);
                    if (candidatesRight.Count() > 0)
                    {
                        nextRight = candidatesRight.Min(point => point.X);
                        delta = nextRight - maxRight;
                        foreach (IAlignable element in affectedObjects)
                        {
                            var cmd = ViewController.CreateMoveCommand(element.Left + delta, null, element.ViewHelper, diagramView.Controller);
                            moveMacroCommand.Commands.Add(cmd);
                        }
                    }
					break;
				case EAlignment.CenterV:
                case EAlignment.CenterH:
				case EAlignment.DistributeV:
				case EAlignment.DistributeH:
				    throw new ArgumentException("Not valid for this command: " + alignment);
			}

			return moveMacroCommand;
		}

		public override bool CanExecute(object parameter)
		{
		    return ActiveDiagramView.SelectedItems.Count > 0;
		}

	}
}
	