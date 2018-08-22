using System;
using System.Collections.Generic;
using XCase.Controller.Commands;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller
{
    /// <summary>
    /// Controller for a diagram. Diagram visualizes selected part of the model. 
    /// </summary>
    public class DiagramController : CommandControllerBase
    {
        /// <summary>
        /// Returns the Diagram model class controlled by this DiagramController
        /// </summary>
        public Diagram Diagram { get; private set; }

        /// <summary>
        /// Returns current Project model class
        /// </summary>
        public Project Project { get { return ModelController.Project; } }

    	/// <summary>
    	/// Accesses the ModelController of current project
    	/// </summary>
        public ModelController ModelController { get; private set; }

        /// <summary>
        /// This is the accessor of the UndoStack, which is actually located in the ModelController.
        /// </summary>
        /// <returns>The Undo Stack</returns>
        public override CommandStack getUndoStack()
        {
            return ModelController.getUndoStack();
        }

        /// <summary>
        /// This is the accessor of the RedoStack, which is actually located in the ModelController.
        /// </summary>
        /// <returns>The Redo Stack</returns>
        public override CommandStack getRedoStack()
        {
            return ModelController.getRedoStack();
        }

    	/// <summary>
        /// Creates controller for a diagram
        /// </summary>
        public DiagramController(Diagram diagram, ModelController modelController)
        {
        	Diagram = diagram;
        	ModelController = modelController;
        }

        /// <summary>
        /// All commands executed after this call will be stored in a queue and executed when CommitMacro is called
        /// </summary>
        /// <returns>The MacroCommand created</returns>
        public override IMacroCommand BeginMacro()
		{
			CreatedMacro = MacroCommandFactory<DiagramController>.Factory().Create(this);
			return CreatedMacro;
		}

		/// <summary>
		/// Adds class to model and diagram
		/// </summary>
		/// <param name="className">name of the new class (<see cref="NamedElement.Name"/>)</param>
		/// <param name="x">x coordinate of the new class on the diagram</param>
		/// <param name="y">y coordinate of the new class on the diagram</param>
		/// <returns>result of creation (element in model and its viewhelper)</returns>
        public CreationResult<Class, ClassViewHelper> NewClass(string className, double x, double y)
		{
			CreationResult<Class, ClassViewHelper> result = new CreationResult<Class, ClassViewHelper>();

			NewModelClassToDiagramCommand command = (NewModelClassToDiagramCommand)NewModelClassToDiagramCommandFactory.Factory().Create(this);
			command.ClassName = className;
			command.X = x;
			command.Y = y;
			command.Set(ModelController, ModelController.Model);
			command.Execute();

			result.ModelElement = command.CreatedClass.Element;
			result.ViewHelper = (ClassViewHelper)command.ViewHelper;
			return result;
		}

        /// <summary>
		/// Adds association to model and diagram
		/// </summary>
		/// <param name="name">name of the new association (optional, set to null or empty string when not naming
		/// the association explicitly)</param>
		/// <param name="classes">associated classes</param>
		/// <returns>result of creation (element in model and its viewhelper)</returns>
		public CreationResult<Association, AssociationViewHelper> NewAssociation(string name, params Class[] classes)
		{
			return NewAssociation(name, (IEnumerable <Class>)classes);	
		}

		/// <summary>
		/// Adds association to model and diagram
		/// </summary>
		/// <param name="name">name of the new association (optional, set to null or empty string when not naming
		/// the association explicitly)</param>
		/// <param name="classes">associated classes</param>
		/// <returns>result of creation (element in model and its viewhelper)</returns>
		public CreationResult<Association, AssociationViewHelper> NewAssociation(string name, IEnumerable<Class> classes)
		{
			CreationResult<Association, AssociationViewHelper> result = new CreationResult<Association, AssociationViewHelper>();
			NewModelAssociationToDiagramCommand associationCommand = (NewModelAssociationToDiagramCommand)NewModelAssociationToDiagramCommandFactory.Factory().Create(this);
			if (!string.IsNullOrEmpty(name))
				associationCommand.Name = name;
			associationCommand.Set(ModelController, ModelController.Model, classes);
			
			associationCommand.Execute();

			result.ModelElement = associationCommand.CreatedAssociation;
			result.ViewHelper = associationCommand.ViewHelper;

			return result;
		}

		public CreationResult<Generalization, GeneralizationViewHelper> NewGeneralization(Class general, Class specific)
    	{
			CreationResult<Generalization, GeneralizationViewHelper> result = new CreationResult<Generalization, GeneralizationViewHelper>();
			NewModelGeneralizationToDiagramCommand generalizationCommand = (NewModelGeneralizationToDiagramCommand)NewModelGeneralizationToDiagramCommandFactory.Factory().Create(this);
			generalizationCommand.Set(ModelController, ModelController.Model, general, specific);
			generalizationCommand.Execute();
			result.ModelElement = generalizationCommand.CreatedGeneralization;
			result.ViewHelper = generalizationCommand.ViewHelper;
			return result;
    	}

		/// <summary>
		/// Adds association class of a given name to model. 
		/// </summary>
		/// <param name="name">name of the association class</param>
		/// <param name="classes">associated classes</param>
		/// <returns>created association classs and its view helper</returns>
		public CreationResult<AssociationClass, AssociationClassViewHelper> NewAssociationClass(string name, Class[] classes)
		{
			return NewAssociationClass(name, classes, double.NaN, double.NaN);	
		}

		/// <summary>
		/// Adds association class of a given name to model on a specific location in the diagram.
		/// </summary>
		/// <param name="name">name of the association class</param>
		/// <param name="classes">associated classes</param>
		/// <param name="x">x coordinate of the association class</param>
		/// <param name="y">y coordinate of the association class</param>
		/// <returns>created association classs and its view helper</returns>
		public CreationResult<AssociationClass, AssociationClassViewHelper> NewAssociationClass(string name, Class[] classes, double x, double y)
    	{
			CreationResult<AssociationClass, AssociationClassViewHelper> result = new CreationResult<AssociationClass, AssociationClassViewHelper>();
			NewModelAssociationClassToDiagramCommand AssociationClassCommand = (NewModelAssociationClassToDiagramCommand)NewModelAssociationClassToDiagramCommandFactory.Factory().Create(this);
			if (!string.IsNullOrEmpty(name))
				AssociationClassCommand.Name = name;
			AssociationClassCommand.X = x;
			AssociationClassCommand.Y = y;
			AssociationClassCommand.Set(ModelController, ModelController.Model, classes);
			
			AssociationClassCommand.Execute();

			result.ModelElement = AssociationClassCommand.CreatedAssociationClass;
			result.ViewHelper = AssociationClassCommand.ViewHelper;

			return result;
    	}

		/// <summary>
		/// Removes the PSM subtree under element <paramref name="e"/>.
		/// </summary>
		/// <param name="e">element under which the subtree is deleted</param>
		public void RemovePSMSubtree(Element e)
		{
			if (!(Diagram is PSMDiagram))
			{
				throw new InvalidOperationException("Method can be called only for PSM diagrams");
			}
			DeleteFromPSMDiagramConsideringRepresentativesMacroCommand c = (DeleteFromPSMDiagramConsideringRepresentativesMacroCommand)DeleteFromPSMDiagramConsideringRepresentativesMacroCommandFactory.Factory().Create(this);
			if (c.InitializeCommand(null, new [] {e}))
				c.Execute();
		}

		public void AddElementToDiagram<ElementType, ViewHelperType>(ElementType element)
			where ElementType : class, Element
			where ViewHelperType : ViewHelper, new()
		{
			ElementToDiagramCommand<ElementType, ViewHelperType> command =
						(ElementToDiagramCommand<ElementType, ViewHelperType>) (ElementToDiagramCommandFactory<ElementType, ViewHelperType>.Factory().Create(this));

			command.IncludedElement = new ElementHolder<ElementType> { Element = element };
			command.Execute();
		}

        public void ChangeTargetNamespace(string targetNamespace)
        {
            ChangePSMDiagramTargetNamespaceCommand command = (ChangePSMDiagramTargetNamespaceCommand) ChangePSMDiagramTargetNamespaceCommandFactory.Factory().Create(this);
            command.TargetNamespace = targetNamespace;
            command.Execute();
        }
    }
}
