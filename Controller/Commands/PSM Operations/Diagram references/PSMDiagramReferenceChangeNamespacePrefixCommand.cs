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
    public class PSMDiagramReferenceChangeNamespacePrefixCommand : DiagramCommandBase
    {
        public PSMDiagramReferenceChangeNamespacePrefixCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.CHANGE_REFERENCE_NAMESPACE_PREFIX;

        }

        [MandatoryArgument]
        public string NamespacePrefix { get; set; }

        [MandatoryArgument]
        public PSMDiagramReference DiagramReference { get; set; }

        private string OldNamespacePrefix { get; set; }

        public override bool CanExecute()
        {
            return true;
        }

        internal override void CommandOperation()
        {
            DiagramReference.NamespacePrefix = NamespacePrefix;
        }

        internal override OperationResult UndoOperation()
        {
            DiagramReference.NamespacePrefix = OldNamespacePrefix;
            return OperationResult.OK;
        }
    }

    #region PSMDiagramReferenceChangeNamespacePrefixCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="PSMDiagramReferenceChangeNamespacePrefixCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class PSMDiagramReferenceChangeNamespacePrefixCommandFactory : DiagramCommandFactory<PSMDiagramReferenceChangeNamespacePrefixCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private PSMDiagramReferenceChangeNamespacePrefixCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of PSMDiagramReferenceChangeNamespacePrefixCommand
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController diagramController)
        {
            return new PSMDiagramReferenceChangeNamespacePrefixCommand(diagramController);
        }
    }

    #endregion
}