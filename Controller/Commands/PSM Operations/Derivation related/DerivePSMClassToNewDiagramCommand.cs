using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Macrocommand for deriving a PSM Class to a new PSM diagram
    /// </summary>
    public class DerivePSMClassToNewDiagramCommand : MacroCommand<ModelController>
    {
        [MandatoryArgument]
        private PIMClass Class {get; set;}

        public ElementHolder<PSMClass> pSMClassHolder = null;

		public HolderBase<PSMDiagram> DiagramHolder = null;

        public DerivePSMClassToNewDiagramCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.DERIVE_PSM_CLASS_NEW_DIAGRAM;
        }

        /// <summary>
        /// Sets this command for execution
        /// </summary>
        /// <param name="c">PIMClass to be derived from</param>
        public void Set(PIMClass c)
        {
            Set(c, null);
        }
        /// <summary>
        /// Sets this command for execution
        /// </summary>
        /// <param name="c">PIMClass to be derived from</param>
        /// <param name="h">Optional Element holder where the final PSMClass will be placed</param>
        public void Set(PIMClass c, ElementHolder<PSMClass> h)
        {
            Class = c;
            if (h != null) pSMClassHolder = h;
            else pSMClassHolder = new ElementHolder<PSMClass>();

            NewPSMClassCommand c1 = NewPSMClassCommandFactory.Factory().Create(Controller) as NewPSMClassCommand;
            c1.RepresentedClass = Class;
            c1.CreatedClass = pSMClassHolder;
            Commands.Add(c1);

            AddPSMDiagramCommand c2 = AddPSMDiagramCommandFactory.Factory().Create(Controller) as AddPSMDiagramCommand;
            
			if (DiagramHolder == null) DiagramHolder = new HolderBase<PSMDiagram>();
        	c2.Set(Controller.Project, DiagramHolder);
            Commands.Add(c2);

            PSMClassToDiagram_ModelCommand c3 = PSMClassToDiagram_ModelCommandFactory.Factory().Create(Controller) as PSMClassToDiagram_ModelCommand;
            c3.Set(pSMClassHolder, DiagramHolder);
            Commands.Add(c3);

            AddPSMClassToRoots_ModelCommand c4 = AddPSMClassToRoots_ModelCommandFactory.Factory().Create(Controller) as AddPSMClassToRoots_ModelCommand;
            c4.Set(pSMClassHolder, DiagramHolder);
            Commands.Add(c4);

        }

		public override void  CommandsExecuted()
		{
			 base.CommandsExecuted();
			 AssociatedElements.Add(pSMClassHolder.Element);
		}
    }

    #region DerivePSMClassToNewDiagramCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="DerivePSMClassToNewDiagramCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class DerivePSMClassToNewDiagramCommandFactory : ModelCommandFactory<DerivePSMClassToNewDiagramCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private DerivePSMClassToNewDiagramCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of DerivePSMClassToNewDiagramCommand
		/// <param name="modelController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new DerivePSMClassToNewDiagramCommand(modelController);
        }
    }
    #endregion
}
