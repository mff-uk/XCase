using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;
using XCase.Controller.Commands.Helpers;
using System.Diagnostics;

namespace XCase.Controller.Commands
{
    /// <summary>   
    /// Creates a new PSM Association.
    /// </summary>
    public class NewPSMAssociationCommand : ModelCommandBase
    {
        /// <summary>
        /// Holder for the parent element of the association
        /// </summary>
        [MandatoryArgument]
        private ElementHolder<PSMSuperordinateComponent> ParentHolder { get; set; }

        /// <summary>
        /// Holder for the child element of the association
        /// </summary>
        [MandatoryArgument]
        private ElementHolder<PSMAssociationChild> ChildHolder { get; set; }

        public NUml.Uml2.UnlimitedNatural Upper = 1;
        public uint? Lower = 1;
        public IEnumerable<Generalization> UsedGeneralizations;

        /// <summary>
        /// Index at which to insert the association into parents components
        /// </summary>
        private int? Index;
        
        /// <summary>
        /// An elementHolder, where the reference to the newly created PSM association can be stored.
        /// </summary>
        [CommandResult]
        public ElementHolder<PSMAssociation> CreatedAssociation { get; set; }

        public NewPSMAssociationCommand(ModelController modelController)
            : base(modelController)
        {
            Description = CommandDescription.ADD_PSM_ASSOCIATION;
        }

        public override bool CanExecute()
        {
            return (ParentHolder != null && ChildHolder != null && CreatedAssociation != null);
        }

        /// <summary>
        /// Sets this command for execution
        /// </summary>
        /// <param name="parentHolder">Holder containing the parent element of the association</param>
        /// <param name="childHolder">Holder containing the child element of the association</param>
        /// <param name="createdAssociation">Holder that will contain the newly created association - can be null</param>
        /// <param name="index">Index at which to insert the association into parents components (null = default)</param>
        public void Set(ElementHolder<PSMSuperordinateComponent> parentHolder, ElementHolder<PSMAssociationChild> childHolder, ElementHolder<PSMAssociation> createdAssociation, int? index)
        {
            ParentHolder = parentHolder;
            ChildHolder = childHolder;
            Index = index;

            if (createdAssociation == null) CreatedAssociation = new ElementHolder<PSMAssociation>();
            else CreatedAssociation = createdAssociation;
        }

        /// <summary>
        /// Sets this command for execution
        /// </summary>
        /// <param name="Parent">Parent element of the association</param>
        /// <param name="Child">Child element of the association</param>
        /// <param name="createdAssociation">Holder that will contain the newly created association - can be null</param>
        /// <param name="index">Index at which to insert the association into parents components (null = default)</param>
        public void Set(PSMSuperordinateComponent Parent, PSMAssociationChild Child, ElementHolder<PSMAssociation> createdAssociation, int? index)
        {
            ParentHolder = new ElementHolder<PSMSuperordinateComponent>() { Element = Parent };
            ChildHolder = new ElementHolder<PSMAssociationChild>() { Element = Child };
            Index = index;

            if (createdAssociation == null) CreatedAssociation = new ElementHolder<PSMAssociation>();
            else CreatedAssociation = createdAssociation;
        }
        
        internal override void CommandOperation()
        {
            if (Index == null)
                CreatedAssociation.Element = (PSMAssociation)ParentHolder.Element.AddComponent(PSMAssociationFactory.Instance);
			else CreatedAssociation.Element = (PSMAssociation)ParentHolder.Element.AddComponent(PSMAssociationFactory.Instance, Index.Value);
            CreatedAssociation.Element.Child = ChildHolder.Element;
            CreatedAssociation.Element.Upper = Upper;
            CreatedAssociation.Element.Lower = Lower;
            if (UsedGeneralizations != null)
                foreach (Generalization G in UsedGeneralizations)
                {
                    CreatedAssociation.Element.UsedGeneralizations.Add(G);
                    G.ReferencingPSMAssociations.Add(CreatedAssociation.Element);
                }
			AssociatedElements.Add(CreatedAssociation.Element);
        }

        internal override OperationResult UndoOperation()
        {
            CreatedAssociation.Element.RemoveMeFromModel();
            return OperationResult.OK;
        }

        internal override void RedoOperation()
        {
            CreatedAssociation.Element.PutMeBackToModel();
        }
    }

    #region NewPSMAssociationCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="NewPSMAssociationCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class NewPSMAssociationCommandFactory : ModelCommandFactory<NewPSMAssociationCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private NewPSMAssociationCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of NewPSMAssociationCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new NewPSMAssociationCommand(modelController);
        }
    }

    #endregion
}
