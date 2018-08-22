using System.Collections.Generic;
using System.Windows;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller
{
	/// <summary>
	/// ViewController mehods alter <a href="../documentation/ViewHelpers.topic">View Helpers</a> 
	/// and thus alter diagram-dependant properties of diagram elemetns.
	/// </summary>
	public class ViewController
	{
		/// <summary>
		/// Move an element on the diagram
		/// </summary>
		/// <param name="X">new X coordinate</param>
		/// <param name="Y">new Y coordinate</param>
		/// <param name="viewHelper">viewHelper of the moved element</param>
		/// <param name="diagramController">diagram controller</param>
		public static void MoveElement(double X, double Y, PositionableElementViewHelper viewHelper, DiagramController diagramController)
		{
			MoveElementCommand moveElementCommand = CreateMoveCommand(X, Y, viewHelper, diagramController);
			moveElementCommand.Execute();
		}

		/// <summary>
		/// Creates <see cref="MoveElementCommand"/> that moves an element on the diagram
		/// </summary>
		/// <param name="X">new X coordinate</param>
		/// <param name="Y">new Y coordinate</param>
		/// <param name="viewHelper">viewHelper of the moved element</param>
		/// <param name="diagramController">diagram controller</param>
		/// <returns><see cref="MoveElementCommand"/> that moves an element on the diagram</returns>
		public static MoveElementCommand CreateMoveCommand(double? X, double? Y, PositionableElementViewHelper viewHelper, DiagramController diagramController)
		{
			MoveElementCommand moveElementCommand = (MoveElementCommand)MoveElementCommandFactory.Factory().Create(diagramController);
			moveElementCommand.X = X;
			moveElementCommand.Y = Y;
			moveElementCommand.ViewHelper = viewHelper;
			return moveElementCommand;
		}

		/// <summary>
		/// Alters size of an element on the diagram
		/// </summary>
		/// <param name="width">new width</param>
		/// <param name="height">new height</param>
		/// <param name="viewHelper">viewHelper of the moved element</param>
		/// <param name="diagramController">diagram controller</param>
		public static void ResizeElement(double width, double height, PositionableElementViewHelper viewHelper, DiagramController diagramController)
		{
			ResizeElementCommand resizeElementCommand = CreateResizeCommand(width, height, viewHelper, diagramController);
			resizeElementCommand.Execute();
		}

		/// <summary>
		/// Creates <see cref="ResizeElementCommand"/> that resizes an element on the diagram
		/// </summary>
		/// <param name="width">new width</param>
		/// <param name="height">new height</param>
		/// <param name="viewHelper">viewHelper of the moved element</param>
		/// <param name="diagramController">diagram controller</param>
		/// <returns>Creates <see cref="ResizeElementCommand"/> that resizes an element on the diagram</returns>
		public static ResizeElementCommand CreateResizeCommand(double width, double height, PositionableElementViewHelper viewHelper, DiagramController diagramController)
		{
			ResizeElementCommand resizeElementCommand = (ResizeElementCommand)ResizeElementCommandFactory.Factory().Create(diagramController);
			resizeElementCommand.Width = width;
			resizeElementCommand.Height = height;
			resizeElementCommand.ViewHelper = viewHelper;
			return resizeElementCommand;
		}

		public static JunctionPointCommand CreateBreakLineCommand(Point p, int orderInJunction, ObservablePointCollection pointCollection, DiagramController controller)
		{
			JunctionPointCommand junctionPointCommand =
											(JunctionPointCommand)JunctionPointCommandFactory.Factory().Create(controller);
			junctionPointCommand.Action = JunctionPointCommand.EJunctionPointAction.AddPoint;
			junctionPointCommand.NewPoint = p;
			junctionPointCommand.PointIndex = orderInJunction;
			junctionPointCommand.ViewHelperPointCollection = pointCollection;
			return junctionPointCommand;
		}

		public static void BreakLine(Point p, int orderInJunction, ObservablePointCollection pointCollection, DiagramController controller)
		{
			JunctionPointCommand pointCommand = CreateBreakLineCommand(p, orderInJunction, pointCollection, controller);
			pointCommand.Execute();
		}

		public static JunctionPointCommand CreateSraightenLineCommand(int orderInJunction, ObservablePointCollection viewHelperPointCollection, DiagramController controller)
		{
			JunctionPointCommand junctionPointCommand = (JunctionPointCommand)JunctionPointCommandFactory.Factory().Create(controller);
			junctionPointCommand.Action = JunctionPointCommand.EJunctionPointAction.RemovePoint;
			junctionPointCommand.PointIndex = orderInJunction;
			junctionPointCommand.ViewHelperPointCollection = viewHelperPointCollection;
			return junctionPointCommand;
		}

		public static void StraightenLine(int orderInJunction, ObservablePointCollection viewHelperPointCollection, DiagramController controller)
		{
			JunctionPointCommand junctionPointCommand = CreateSraightenLineCommand(orderInJunction, viewHelperPointCollection, controller);
			junctionPointCommand.Execute();
		}

		public static void ChangeSectionVisibility(ClassViewHelper classViewHelper, SectionVisibilityCommand.ESectionVisibilityAction action, DiagramController controller)
		{
			SectionVisibilityCommand command = (SectionVisibilityCommand)SectionVisibilityCommandFactory.Factory().Create(controller);
			command.Action = action;
			command.ViewHelper = classViewHelper;
			command.Execute();		
		}

        public static void ChangeElementNameLabelAlignment(ClassViewHelper classViewHelper, bool alignedRight, DiagramController controller)
        {
            ChangeElementNameLabelAlignmentCommand c = (ChangeElementNameLabelAlignmentCommand)ChangeElementNameLabelAlignmentCommandFactory.Factory().Create(controller);
            c.Set(classViewHelper, alignedRight);
            c.Execute();
        }

		public static void SwitchAssociationDiamond(AssociationViewHelper associationViewHelper, Association association, DiagramController controller)
		{
			SwitchAssociationDiamondCommand switchAssociationDiamondCommand =
				(SwitchAssociationDiamondCommand) SwitchAssociationDiamondCommandFactory.Factory().Create(controller);
			switchAssociationDiamondCommand.Association = association;
			switchAssociationDiamondCommand.AssociationViewHelper = associationViewHelper;
			switchAssociationDiamondCommand.Execute();
		}
	}
}