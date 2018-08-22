using System;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds an element to a diagram
	/// </summary>
	public class AddPSMSpecializationMacroCommand : MacroCommand<DiagramController>
	{
        /// <summary>
		/// Creates new instance of <see cref="AddPSMSpecializationCommand">AddPSMSpecificationCommand</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
        public AddPSMSpecializationMacroCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.PSM_ADD_SPECIFICATIONS;
		}

		[CommandResult]
		public ElementHolder<PSMClass> CreatedSpecificPSMClass { get; set; }

		//[CommandResult]
		public ElementHolder<Generalization> CreatedGeneralization { get; set;}

        public void Set(PSMClass generalPSMClass, PIMClass specificPIMClass)
        {
            if (CreatedSpecificPSMClass == null) CreatedSpecificPSMClass = new ElementHolder<PSMClass>();
            if (CreatedGeneralization == null) CreatedGeneralization = new ElementHolder<Generalization>();

            NewPSMClassCommand c1 = NewPSMClassCommandFactory.Factory().Create(Controller.ModelController) as NewPSMClassCommand;
            c1.RepresentedClass = specificPIMClass;
            c1.CreatedClass = CreatedSpecificPSMClass;
            Commands.Add(c1);

            NewPSMSpecializationCommand c2 = NewPSMSpecializationCommandFactory.Factory().Create(Controller.ModelController) as NewPSMSpecializationCommand;
            c2.GeneralPSMClass = new ElementHolder<PSMClass>() { Element = generalPSMClass };
            c2.SpecificPSMClass = CreatedSpecificPSMClass;
            c2.CreatedGeneralization = CreatedGeneralization;
            Commands.Add(c2);

            ElementToDiagramCommand<PSMClass, PSMElementViewHelper> c3 = ElementToDiagramCommandFactory<PSMClass, PSMElementViewHelper>.Factory().Create(Controller) as ElementToDiagramCommand<PSMClass, PSMElementViewHelper>;
            c3.IncludedElement = CreatedSpecificPSMClass;
            Commands.Add(c3);

            ElementToDiagramCommand<Generalization, GeneralizationViewHelper> c4 = ElementToDiagramCommandFactory<Generalization, GeneralizationViewHelper>.Factory().Create(Controller) as ElementToDiagramCommand<Generalization, GeneralizationViewHelper>;
            c4.IncludedElement = CreatedGeneralization;
            Commands.Add(c4);
        }
    }

    #region AddPSMSpecializationMacroCommandFactory

    /// <summary>
	/// Factory that creates instances of <see cref="AddPSMSpecializationMacroCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class AddPSMSpecializationMacroCommandFactory : DiagramCommandFactory<AddPSMSpecializationMacroCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
        private AddPSMSpecializationMacroCommandFactory()
		{
		}

		/// <summary>
        /// Creates new instance of AddPSMSpecializationMacroCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
            return new AddPSMSpecializationMacroCommand(diagramController);
		}
	}

	#endregion
}
