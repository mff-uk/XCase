using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Simple command which removes a PSMSuperordinateComponent without dependencies
    /// </summary>
    public class RemovePSMSuperordinateComponentCommand : ModelCommandBase
    {
        /// <summary>
        /// Gets or sets a reference to the component being removed.
        /// </summary>
        public PSMSuperordinateComponent Component { get; set; }

        public RemovePSMSuperordinateComponentCommand(ModelController modelController)
            : base(modelController)
        {
            Description = CommandDescription.REMOVE_PSM_SUPERORDINATE;
        }

        public override bool CanExecute()
        {
            return Component != null;
        }

        internal override void CommandOperation()
        {
            Component.RemoveMeFromModel();
        }

        internal override OperationResult UndoOperation()
        {
            Component.PutMeBackToModel();
            return OperationResult.OK;
        }
    }
}
