using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Macrocommand for deriving a PSM Class to an existing diagram
    /// </summary>
    public class DerivePSMClassToDiagramCommand : MacroCommand<ModelController>
    {
        [MandatoryArgument]
        private PIMClass Class {get; set;}

        public ElementHolder<PSMClass> pSMClassHolder = null;

        public DerivePSMClassToDiagramCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.DERIVE_PSM_CLASS_DIAGRAM;
        }

        /// <summary>
        /// Sets this command for execution
        /// </summary>
        /// <param name="c">PIMClass to be derived from</param>
        /// <param name="d">Diagram to which to add the PSM Class as one of roots</param>
        public void Set(PIMClass c, PSMDiagram d)
        {
            Set(c, d, null);
        }
        /// <summary>
        /// Sets this command for execution
        /// </summary>
        /// <param name="c">PIMClass to be derived from</param>
        /// <param name="d">Diagram to which to add the PSM Class as one of roots</param>
        /// <param name="h">Optional Element holder where the final PSMClass will be placed</param>
        public void Set(PIMClass c, PSMDiagram d, ElementHolder<PSMClass> h)
        {
            Class = c;
            HolderBase<PSMDiagram> DiagramHolder = new HolderBase<PSMDiagram>() { Element = d };
            if (h != null) pSMClassHolder = h;
            else pSMClassHolder = new ElementHolder<PSMClass>();

            NewPSMClassCommand c1 = NewPSMClassCommandFactory.Factory().Create(Controller) as NewPSMClassCommand;
            c1.RepresentedClass = Class;
            c1.CreatedClass = pSMClassHolder;
            Commands.Add(c1);

            PSMClassToDiagram_ModelCommand c3 = PSMClassToDiagram_ModelCommandFactory.Factory().Create(Controller) as PSMClassToDiagram_ModelCommand;
            c3.Set(pSMClassHolder, DiagramHolder);
            Commands.Add(c3);

            AddPSMClassToRoots_ModelCommand c4 = AddPSMClassToRoots_ModelCommandFactory.Factory().Create(Controller) as AddPSMClassToRoots_ModelCommand;
            c4.Set(pSMClassHolder, DiagramHolder);
            Commands.Add(c4);

            ActivateDiagramCommand c5 = ActivateDiagramCommandFactory.Factory().Create(Controller) as ActivateDiagramCommand;
            c5.Set(d);
            Commands.Add(c5);
        }

    }
    #region DerivePSMClassToDiagramCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="DerivePSMClassToNewDiagramCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class DerivePSMClassToDiagramCommandFactory : ModelCommandFactory<DerivePSMClassToDiagramCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private DerivePSMClassToDiagramCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of DerivePSMClassToDiagramCommand
		/// <param name="modelController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new DerivePSMClassToDiagramCommand(modelController);
        }
    }
    #endregion
}
