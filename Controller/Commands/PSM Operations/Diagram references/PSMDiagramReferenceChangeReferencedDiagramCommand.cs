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
    public class PSMDiagramReferenceChangeReferencedDiagramCommand : DiagramCommandBase
    {
        public PSMDiagramReferenceChangeReferencedDiagramCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.CHANGE_REFERENCED_DIAGRAM;

        }

        [MandatoryArgument]
        public PSMDiagram ReferencedDiagram { get; set; }

        [MandatoryArgument]
        public PSMDiagramReference DiagramReference { get; set; }

        private PSMDiagram OldReferencedDiagram { get; set; }

        public override bool CanExecute()
        {
            return true;
        }

        internal override void CommandOperation()
        {
            DiagramReference.ReferencedDiagram = ReferencedDiagram;
        }

        internal override OperationResult UndoOperation()
        {
            DiagramReference.ReferencedDiagram = OldReferencedDiagram;
            return OperationResult.OK;
        }
    }

    #region PSMDiagramReferenceChangeReferencedDiagramCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="PSMDiagramReferenceChangeReferencedDiagramCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class PSMDiagramReferenceChangeReferencedDiagramCommandFactory : DiagramCommandFactory<PSMDiagramReferenceChangeReferencedDiagramCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private PSMDiagramReferenceChangeReferencedDiagramCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of PSMDiagramReferenceChangeReferencedDiagramCommand
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController diagramController)
        {
            return new PSMDiagramReferenceChangeReferencedDiagramCommand(diagramController);
        }
    }

    #endregion
}