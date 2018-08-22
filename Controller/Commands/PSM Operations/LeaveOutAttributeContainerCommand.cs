using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Removes attribute container from the diagram, 
	/// all attributes in the container are copied back to the PSM Class (if they are not already there).
	/// </summary>
	public class LeaveOutAttributeContainerCommand : DiagramCommandBase
	{
		[MandatoryArgument]
		public PSMAttributeContainer AttributeContainerLeftOut
		{
			get;
			set;
		}

		/// <summary>
		/// Creates new instance of <see cref="LeaveOutAttributeContainerCommand">LeaveOutAttributeContainerCommand</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
		public LeaveOutAttributeContainerCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.PSM_LEAVE_OUT_ATTRIBUTE_CONTAINER;
		}

		private ViewHelper viewHelper;
		private List<PSMAttribute> returnedAttributes;
		private PSMClass parentClass; 

		public override bool CanExecute()
		{
			return true;
		}

		internal override void CommandOperation()
		{
			PSMSuperordinateComponent p = AttributeContainerLeftOut.Parent;
			while (!(p is PSMClass) && p is PSMSubordinateComponent)
				p = ((PSMSubordinateComponent)p).Parent;
			parentClass = p as PSMClass;
			if (parentClass != null)
			{
				returnedAttributes = new List<PSMAttribute>();
				foreach (PSMAttribute attribute in AttributeContainerLeftOut.PSMAttributes)
				{
					if (parentClass.PSMAttributes.Any(a => a.RepresentedAttribute == attribute.RepresentedAttribute))
						continue;
					PSMAttribute createdAttribute = parentClass.AddAttribute(attribute.RepresentedAttribute);
					returnedAttributes.Add(createdAttribute);
					if (!String.IsNullOrEmpty(attribute.Alias))
					{
						createdAttribute.Alias = attribute.Alias;
					}
				}
				AttributeContainerLeftOut.RemoveMeFromModel();
				viewHelper = Diagram.DiagramElements[AttributeContainerLeftOut];
				Diagram.RemoveModelElement(AttributeContainerLeftOut);

			}
		}

		internal override OperationResult UndoOperation()
		{
			if (parentClass != null)
			{	
				foreach (PSMAttribute attribute in returnedAttributes)
				{
					parentClass.Attributes.Remove(attribute);
				}	
			}
			AttributeContainerLeftOut.PutMeBackToModel();
			Diagram.AddModelElement(AttributeContainerLeftOut, viewHelper);
			return OperationResult.OK;
		}
	}

	#region LeaveOutAttributeContainerCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="LeaveOutAttributeContainerCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class LeaveOutAttributeContainerCommandFactory : DiagramCommandFactory<LeaveOutAttributeContainerCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private LeaveOutAttributeContainerCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of LeaveOutAttributeContainerCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new LeaveOutAttributeContainerCommand(diagramController);
		}
	}

	#endregion
}
