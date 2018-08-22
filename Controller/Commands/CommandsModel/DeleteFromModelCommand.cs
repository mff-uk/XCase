using System;
using System.Collections.Generic;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Deletes elements from model.
	/// </summary>
	public class DeleteFromModelCommand : ModelCommandBase
	{
		/// <summary>
		/// Elements that should be deleted
		/// </summary>
		[MandatoryArgument]
		public IEnumerable<Element> DeletedElements { get; set; }

		public Diagram CallingDiagram { get; set; }

		public DeleteFromModelCommand(ModelController Controller)
			: base(Controller)
		{
			Description = CommandDescription.DELETE_FROM_MODEL;
		}

		public override bool CanExecute()
		{
			foreach (Element element in DeletedElements)
			{
				if (Controller.IsElementUsedInDiagrams(element, CallingDiagram))
				{
					ErrorDescription = CommandError.CMDERR_REMOVED_ELEMENT_BEING_USED;
					return false; 
				}
                if (Controller.HasElementPSMDependencies(element))
                {
                    if (element is PIMClass)
                    {
                        string Classes = Environment.NewLine;
                        foreach (PSMClass C in (element as PIMClass).DerivedPSMClasses)
                        {
                            Classes += C.Diagram.Caption + ": " + C.Name + Environment.NewLine;
                        }
                        Classes = Classes.Remove(Classes.Length - 1);
                        ErrorDescription = String.Format(CommandError.CMDERR_DELETE_PSM_DEPENDENT_CLASS, element, Classes);
                    }
                    else if (element is Association)
                    {
                        string NestingJoins = Environment.NewLine;
                        foreach (NestingJoin C in (element as Association).ReferencingNestingJoins)
                        {
                            NestingJoins += C.Association.Diagram.Caption + ": " + C.Association + Environment.NewLine;
                        }
                        NestingJoins = NestingJoins.Remove(NestingJoins.Length - 1);
                        ErrorDescription = String.Format(CommandError.CMDERR_DELETE_PSM_DEPENDENT_ASSOCIATION, element, NestingJoins);
                    }
                    else if (element is Generalization)
                    {
                        if ((element as Generalization).ReferencingPSMAttributes.Count > 0)
                        {
                            string attr = "Generalization \"{0}\" has PSM dependecies. These PSM Attributes were created using this generalization:" + Environment.NewLine + Environment.NewLine;
                            foreach (PSMAttribute A in (element as Generalization).ReferencingPSMAttributes)
                            {
                                attr += A.Class.Diagram.Caption + ": " + A.Class.Name + "." + A.Name + Environment.NewLine;
                            }
                            attr = attr.Remove(attr.Length - 1);
                            ErrorDescription = string.Format(attr, element);
                        }
                        else if ((element as Generalization).ReferencingPSMAssociations.Count > 0)
                        {
                            string assoc = "Generalization \"{0}\" has PSM dependecies. These PSM Associations were created using this generalization:" + Environment.NewLine + Environment.NewLine;
                            foreach (PSMAssociation A in (element as Generalization).ReferencingPSMAssociations)
                            {
                                assoc += A.Diagram.Caption + ": " + A + Environment.NewLine;
                            }
                            assoc = assoc.Remove(assoc.Length - 1);
                            ErrorDescription = string.Format(assoc, element);
                        }
                        else
                        {
                            ErrorDescription = "Unknown generalization PSM dependency";
                        }
                    }
					return false; 
                }
			}
			return true;
		}

		internal override void CommandOperation()
		{
			foreach (Element element in DeletedElements)
			{
				element.RemoveMeFromModel();
				AssociatedElements.Add(element);
			}
		}

		internal override OperationResult UndoOperation()
		{
			foreach (Element element in DeletedElements)
			{
				element.PutMeBackToModel();
			}
			return OperationResult.OK;
		}
	}

	#region DeleteFromModelCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="DeleteFromModelCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class DeleteFromModelCommandFactory : ModelCommandFactory<DeleteFromModelCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private DeleteFromModelCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of DeleteFromModelCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new DeleteFromModelCommand(modelController);
		}
	}

	#endregion
}