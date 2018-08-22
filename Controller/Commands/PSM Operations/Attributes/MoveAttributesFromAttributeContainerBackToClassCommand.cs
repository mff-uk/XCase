using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Moves attributes from Attribute container to class.
	/// </summary>
	public class MoveAttributesFromAttributeContainerBackToClassCommand : DiagramCommandBase
	{
		/// <summary>
		/// Creates new instance of <see cref="MoveAttributesFromAttributeContainerBackToClassCommand">MoveAttributesFromAttributeContainerBackToClassCommand</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
		public MoveAttributesFromAttributeContainerBackToClassCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.PSM_MOVE_ATTRIBUTES_BACK_TO_CLASS;
			attributes = new List<PSMAttribute>();
		}

		private List<PSMAttribute> attributes;

		public List<PSMAttribute> Attributes
		{
			get
			{
				return attributes;
			}
		}

		[MandatoryArgument]
		public PSMAttributeContainer AttributeContainer { get; set; }

		public override bool CanExecute()
		{
			PSMClass c = null;
			if (attributes.Count > 0)
			{
				c = attributes.First().Class;
			}
            else
			    return false;
			if (attributes.Any(attribute => attribute.Class != c))
				return false;
			return true;
		}

		private PSMClass parentClass;
		private List<PSMAttribute> returnedAttributes;

		internal override void CommandOperation()
		{
			if (attributes.Count > 0)
			{
				parentClass = AttributeContainer.PSMClass;
                if (parentClass == null)
                    parentClass = Attributes.First().Class;
			    Debug.Assert(parentClass != null);
				returnedAttributes = new List<PSMAttribute>();
				foreach (PSMAttribute attribute in attributes)
				{
					AttributeContainer.PSMAttributes.Remove(attribute);
					//PSMAttribute createdAttribute = parentClass.AddAttribute(attribute.RepresentedAttribute);
                    parentClass.PSMAttributes.Add(attribute);
					returnedAttributes.Add(attribute);
				}
				AssociatedElements.Add(parentClass);
			}
		}

		internal override OperationResult UndoOperation()
		{
			if (parentClass != null)
			{
				//Attributes.Clear();
				foreach (PSMAttribute attribute in returnedAttributes)
				{
					parentClass.Attributes.Remove(attribute);
					//PSMAttribute createdAttribute = AttributeContainer.AddAttribute(attribute.RepresentedAttribute);
                    //Attributes.Add(attribute);
                    AttributeContainer.PSMAttributes.Add(attribute);
				}
				
			}
			return OperationResult.OK;
		}
	}

	#region MoveAttributesFromAttributeContainerBackToClassCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="MoveAttributesFromAttributeContainerBackToClassCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class MoveAttributesFromAttributeContainerBackToClassCommandFactory : DiagramCommandFactory<MoveAttributesFromAttributeContainerBackToClassCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private MoveAttributesFromAttributeContainerBackToClassCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of MoveAttributesFromAttributeContainerBackToClassCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new MoveAttributesFromAttributeContainerBackToClassCommand(diagramController);
		}
	}

	#endregion
}
