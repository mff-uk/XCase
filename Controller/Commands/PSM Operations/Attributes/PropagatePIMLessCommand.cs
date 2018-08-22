using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Part of PropagatePIMLessMacroCommand, not to be used elsewhere
    /// </summary>
    internal class PropagatePIMLessCommand : ModelCommandBase
    {
        [MandatoryArgument]
        private ElementHolder<PSMAttribute> PIMLessAttributeHolder { get; set; }

        [MandatoryArgument]
        private ElementHolder<Property> PIMAttributeHolder { get; set; }
        
        public PropagatePIMLessCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.PROPAGATE_PIMLESS;
        }

        public override bool CanExecute()
        {
            return PIMAttributeHolder != null && PIMLessAttributeHolder != null;
        }

        public void Set(ElementHolder<PSMAttribute> pIMLessAttributeHolder, ElementHolder<Property> pIMAttributeHolder)
        {
            PIMLessAttributeHolder = pIMLessAttributeHolder;
            PIMAttributeHolder = pIMAttributeHolder;
        }
        
        internal override void CommandOperation()
        {
            PIMAttributeHolder.Element.DerivedPSMAttributes.Add(PIMLessAttributeHolder.Element);
            PIMLessAttributeHolder.Element.RepresentedAttribute = PIMAttributeHolder.Element;
        }

        internal override OperationResult UndoOperation()
        {
            PIMAttributeHolder.Element.DerivedPSMAttributes.Remove(PIMLessAttributeHolder.Element);
            PIMLessAttributeHolder.Element.RepresentedAttribute = null;
            return OperationResult.OK;
        }
    }

    #region PropagatePIMLessCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="PropagatePIMLessCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class PropagatePIMLessCommandFactory : ModelCommandFactory<PropagatePIMLessCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private PropagatePIMLessCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of PropagatePIMLessCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new PropagatePIMLessCommand(modelController);
        }
    }

    #endregion
}
