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
    public class PSMDiagramReferenceChangeLocalCommand : DiagramCommandBase
    {
        public PSMDiagramReferenceChangeLocalCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.REFERENCE_LOCAL_EXTERNAL;

        }

        [MandatoryArgument]
        public bool Local { get; set; }

        [MandatoryArgument]
        public PSMDiagramReference DiagramReference { get; set; }

        private bool OldLocal { get; set; }

        public override bool CanExecute()
        {
            return true;
        }

        internal override void CommandOperation()
        {
            DiagramReference.Local = Local;
        }

        internal override OperationResult UndoOperation()
        {
            DiagramReference.Local = OldLocal;
            return OperationResult.OK;
        }
    }

    #region PSMDiagramReferenceChangeLocalCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="PSMDiagramReferenceChangeLocalCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class PSMDiagramReferenceChangeLocalCommandFactory : DiagramCommandFactory<PSMDiagramReferenceChangeLocalCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private PSMDiagramReferenceChangeLocalCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of PSMDiagramReferenceChangeLocalCommand
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController diagramController)
        {
            return new PSMDiagramReferenceChangeLocalCommand(diagramController);
        }
    }

    #endregion
}