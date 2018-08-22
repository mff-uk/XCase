using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds an element to a diagram
	/// </summary>
	public class LeaveOutSuperordinateComponent : DiagramCommandBase
	{
		/// <summary>
		/// Creates new instance of <see cref="LeaveOutSuperordinateComponent">LeaveOutSuperordinateComponent</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
		public LeaveOutSuperordinateComponent(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.PSM_LEAVE_OUT_SUPERORDINATE_COMPONENT;
		}

		[MandatoryArgument]
		public PSMSuperordinateComponent ComponentLeftOut
		{
			get;
			set;
		}

		private PSMSuperordinateComponent parent;

		private PSMSubordinateComponent[] movedComponents;

		private ViewHelper viewHelper;

		public override bool CanExecute()
		{
			return (ComponentLeftOut != null && ComponentLeftOut is PSMSubordinateComponent);
		}

		internal override void CommandOperation()
		{
			parent = ((PSMSubordinateComponent)ComponentLeftOut).Parent;
			movedComponents = new PSMSubordinateComponent[ComponentLeftOut.Components.Count];
			ComponentLeftOut.Components.CopyTo(movedComponents, 0);
            int componentIndex = parent.Components.IndexOf(ComponentLeftOut as PSMSubordinateComponent);
			foreach (PSMSubordinateComponent component in movedComponents)
			{
				ComponentLeftOut.Components.Remove(component);
                parent.Components.Insert(componentIndex, component);
                ++componentIndex;
			}

			viewHelper = Diagram.DiagramElements[ComponentLeftOut];
			Diagram.RemoveModelElement(ComponentLeftOut);
			ComponentLeftOut.RemoveMeFromModel();
			AssociatedElements.Add(ComponentLeftOut);
		}

		internal override OperationResult UndoOperation()
		{
			ComponentLeftOut.PutMeBackToModel();
			foreach (PSMSubordinateComponent component in movedComponents)
			{
				parent.Components.Remove(component);
				ComponentLeftOut.Components.Add(component);
			}
			Diagram.AddModelElement(ComponentLeftOut, viewHelper);
			return OperationResult.OK;
		}
	}

	#region LeaveOutSuperordinateComponentFactory

	/// <summary>
	/// Factory that creates instances of <see cref="LeaveOutSuperordinateComponent"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class LeaveOutSuperordinateComponentFactory : DiagramCommandFactory<LeaveOutSuperordinateComponentFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private LeaveOutSuperordinateComponentFactory()
		{
		}

		/// <summary>
		/// Creates new instance of LeaveOutSuperordinateComponent
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new LeaveOutSuperordinateComponent(diagramController);
		}
	}

	#endregion
}
