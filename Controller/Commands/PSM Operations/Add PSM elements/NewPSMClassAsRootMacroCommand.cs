using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Adds new PSM Class as a root
    /// </summary>
    public class NewPSMClassAsRootMacroCommand : MacroCommand<DiagramController>
    {
        private ElementHolder<PSMClass> PSMClassHolder;
        
        public NewPSMClassAsRootMacroCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.ADD_PSM_DIAGRAM_ROOT;
        }

        public void Set(PIMClass RepresentedClass)
        {
            PSMClassHolder = new ElementHolder<PSMClass>();

            NewPSMClassCommand c1 = NewPSMClassCommandFactory.Factory().Create(Controller.ModelController) as NewPSMClassCommand;
            c1.RepresentedClass = RepresentedClass;
            c1.CreatedClass = PSMClassHolder;
            Commands.Add(c1);

            ElementToDiagramCommand<PSMClass, PSMElementViewHelper> c2 = 
                ElementToDiagramCommandFactory<PSMClass, PSMElementViewHelper>.Factory().Create(Controller) 
                as ElementToDiagramCommand<PSMClass, PSMElementViewHelper>;
            c2.IncludedElement = PSMClassHolder;
            Commands.Add(c2);

            AddPSMClassToRootsCommand c3 = AddPSMClassToRootsCommandFactory.Factory().Create(Controller) as AddPSMClassToRootsCommand;
            c3.Set(PSMClassHolder);
            Commands.Add(c3);
        }
    }

    #region NewPSMClassAsRootMacroCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="NewPSMClassAsRootMacroCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class NewPSMClassAsRootMacroCommandFactory : DiagramCommandFactory<NewPSMClassAsRootMacroCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private NewPSMClassAsRootMacroCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of NewPSMClassAsRootMacroCommand
        /// <param name="controller">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController controller)
        {
            return new NewPSMClassAsRootMacroCommand(controller) { };
        }
    }

    #endregion

}
