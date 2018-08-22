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
    public class PSMDiagramReferenceChangeSchemaLocationCommand:DiagramCommandBase
    {
        public PSMDiagramReferenceChangeSchemaLocationCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.CHANGE_SCHEMA_LOCATION;
        }

        [MandatoryArgument]
        public string SchemaLocation { get; set; }

        [MandatoryArgument]
        public PSMDiagramReference DiagramReference { get; set; }

        private string OldSchemaLocation { get; set; }

        public override bool CanExecute()
        {
            return true;
        }

        internal override void CommandOperation()
        {
            DiagramReference.SchemaLocation = SchemaLocation;
        }

        internal override OperationResult UndoOperation()
        {
            DiagramReference.SchemaLocation = OldSchemaLocation;
            return OperationResult.OK;
        }
    }

    #region PSMDiagramReferenceChangeSchemaLocationCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="PSMDiagramReferenceChangeSchemaLocationCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class PSMDiagramReferenceChangeSchemaLocationCommandFactory : DiagramCommandFactory<PSMDiagramReferenceChangeSchemaLocationCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private PSMDiagramReferenceChangeSchemaLocationCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of PSMDiagramReferenceChangeSchemaLocationCommand
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController diagramController)
        {
            return new PSMDiagramReferenceChangeSchemaLocationCommand(diagramController);
        }
    }

    #endregion
}