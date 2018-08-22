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
    /// <summary>
    /// Adds a reference of another diagram into a diagram. 
    /// </summary>
    public class AddPSMDiagramReferenceCommand : DiagramCommandBase
    {
        public AddPSMDiagramReferenceCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.PSM_ADD_DIAGRAM_REFERENCE;

            ViewHelper = new PSMElementViewHelper(Diagram);
        }

        public override bool CanExecute()
        {
            return Diagram is PSMDiagram;
        }


        public PSMElementViewHelper ViewHelper { get; set; }

        [MandatoryArgument]
        public PSMDiagram ReferencedDiagram { get; set; }

        public ElementHolder<PSMDiagramReference> CreatedDiagramReference;

        internal override void CommandOperation()
        {
            if (CreatedDiagramReference == null)
            {
                CreatedDiagramReference = new ElementHolder<PSMDiagramReference>();
            }

            PSMDiagramReference reference = new PSMDiagramReference()
            {
                ReferencedDiagram = ReferencedDiagram,
                ReferencingDiagram = (PSMDiagram) this.Diagram,
                Name = ReferencedDiagram.Caption,
                Local = true,
                SchemaLocation = ReferencedDiagram.Caption + ".xsd"
            };

            CreatedDiagramReference.Element = reference;

            Debug.Assert(CreatedDiagramReference.HasValue);
            Diagram.AddModelElement(reference, ViewHelper);
            AssociatedElements.Add(reference);
        }

        internal override OperationResult UndoOperation()
        {
            Debug.Assert(CreatedDiagramReference.HasValue);
            Diagram.RemoveModelElement(CreatedDiagramReference.Element);
            CreatedDiagramReference.Element.RemoveMeFromModel();
            return OperationResult.OK;
        }

        internal override void RedoOperation()
        {
            CreatedDiagramReference.Element.PutMeBackToModel();
            Diagram.AddModelElement(CreatedDiagramReference.Element, ViewHelper);
        }
    }

    #region AddPSMDiagramReferenceCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="AddPSMDiagramReferenceCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class AddPSMDiagramReferenceCommandFactory : DiagramCommandFactory<AddPSMDiagramReferenceCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private AddPSMDiagramReferenceCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of AddPSMDiagramReferenceCommand
        /// <param name="controller">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController controller)
        {
            return new AddPSMDiagramReferenceCommand(controller);
        }
    }

    #endregion
}
