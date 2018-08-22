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
    public class ChangePSMDiagramTargetNamespaceCommand : DiagramCommandBase
    {
        public ChangePSMDiagramTargetNamespaceCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.CHANGE_DIAGRAM_TARGET_NAMESPACE;
            if (!(controller.Diagram is PSMDiagram))
            {
                throw new ArgumentException(CommandError.NOT_PSM_DIAGRAM);
            }
        }

        public override bool CanExecute()
        {
            return true;
        }

        public string TargetNamespace { get; set; }

        public string OldTargetNamespace { get; set; }

        public PSMDiagram PSMDiagram
        {
            get { return (PSMDiagram)Diagram; }
        }

        internal override void CommandOperation()
        {
            if (TargetNamespace == String.Empty)
            {
                TargetNamespace = null;
            }
            OldTargetNamespace = PSMDiagram.TargetNamespace;
            PSMDiagram.TargetNamespace = TargetNamespace;
        }

        internal override OperationResult UndoOperation()
        {
            PSMDiagram.TargetNamespace = OldTargetNamespace;
            return OperationResult.OK;
        }
    }

    #region ChangePSMDiagramTargetNamespaceCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="ChangePSMDiagramTargetNamespaceCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class ChangePSMDiagramTargetNamespaceCommandFactory : DiagramCommandFactory<ChangePSMDiagramTargetNamespaceCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private ChangePSMDiagramTargetNamespaceCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of ChangePSMDiagramTargetNamespaceCommand
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController diagramController)
        {
            return new ChangePSMDiagramTargetNamespaceCommand(diagramController);
        }
    }

    #endregion
}