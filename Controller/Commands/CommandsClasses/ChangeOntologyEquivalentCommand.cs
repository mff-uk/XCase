using System;
using NUml.Uml2;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using DataType = XCase.Model.DataType;
using Property = XCase.Model.Property;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Changes the ontology equivalent for a NamedElement
    /// </summary>
    public class ChangeOntologyEquivalentCommand : ModelCommandBase
    {
        /// <summary>
        /// Creates new instance of <see cref="ChangeOntologyEquivalentCommand" />. 
        /// </summary>
        /// <param name="controller">command controller</param>
        public ChangeOntologyEquivalentCommand(ModelController controller)
            : base(controller)
        {
        }

        string old;

        /// <summary>
        /// NamedElement for which the ontology equivalent is about to be changed
        /// </summary>
        [MandatoryArgument]
        public Model.NamedElement Element { get; set; }

        /// <summary>
        /// New ontologyEquivalent
        /// </summary>
        [MandatoryArgument]
        public string NewOntoEquiv { get; set; }

        public override bool CanExecute()
        {
            return true;
        }

        internal override void CommandOperation()
        {
            old = Element.OntologyEquivalent;
            Element.OntologyEquivalent = NewOntoEquiv;
        }

        internal override OperationResult UndoOperation()
        {
            Element.OntologyEquivalent = old;
            return OperationResult.OK;
        }

        internal override void RedoOperation()
        {
            Element.OntologyEquivalent = NewOntoEquiv;
        }
    }

    #region ChangeOntologyEquivalentCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="ChangeOntologyEquivalentCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class ChangeOntologyEquivalentCommandFactory : ModelCommandFactory<ChangeOntologyEquivalentCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private ChangeOntologyEquivalentCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of ChangeOntologyEquivalentCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new ChangeOntologyEquivalentCommand(modelController);
        }
    }

    #endregion
}