using System;
using System.Diagnostics;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using XCase.Controller.Dialogs;
using System.Collections;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace XCase.Controller.Commands
{
    public class PSMDiagramReferenceChangeNamespaceCommand : DiagramCommandBase
    {
        public PSMDiagramReferenceChangeNamespaceCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.CHANGE_REFERENCE_NAMESPACE;

        }

        [MandatoryArgument]
        public string Namespace { get; set; }

        [MandatoryArgument]
        public PSMDiagramReference DiagramReference { get; set; }

        private string OldNamespace { get; set; }

        public override bool CanExecute()
        {
            return true;
        }

        internal override void CommandOperation()
        {
            DiagramReference.Namespace = Namespace;
        }

        internal override OperationResult UndoOperation()
        {
            DiagramReference.Namespace = OldNamespace;
            return OperationResult.OK;
        }
    }

    #region PSMDiagramReferenceChangeNamespaceCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="PSMDiagramReferenceChangeNamespaceCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class PSMDiagramReferenceChangeNamespaceCommandFactory : DiagramCommandFactory<PSMDiagramReferenceChangeNamespaceCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private PSMDiagramReferenceChangeNamespaceCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of PSMDiagramReferenceChangeNamespaceCommand
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController diagramController)
        {
            return new PSMDiagramReferenceChangeNamespaceCommand(diagramController);
        }
    }

    #endregion
}