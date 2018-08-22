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
    /// Sets the RepresentedPSMClass property of representing PSM Class ot givent representative PSM Class
    /// </summary>
    public class SetRepresentedPSMClassCommand : DiagramCommandBase
    {
        /// <summary>
        /// A PSM class represented by the PSM StructuralRepresentative (PSMClass). Can be null (regular PSM Class).
        /// </summary>
        private PSMClass RepresentedClass;

        private PSMClass OriginalRepresentedClass;

        /// <summary>
		/// An elementHolder, where the reference to the representing class is stored
		/// </summary>
        private ElementHolder<PSMClass> Representative;

        public SetRepresentedPSMClassCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.SET_REPRESENTED_CLASS;
        }

        /// <summary>
        /// Prepares this command for execution, asks for the class to be represented
        /// </summary>
        /// <param name="representative">PSMClass to become PSMStructuralRepresentative</param>
        /// <param name="diagram">PSMDiagram where to look for possible represented classes</param>
        public void Set(PSMClass representative, PSMDiagram diagram)
        {
           List<PSMClass> Candidates = new List<PSMClass>();
           Candidates.AddRange(representative.RepresentedClass.DerivedPSMClasses.Where(PSMClass => PSMClass != representative && PSMClass.Diagram == diagram));

            if (Candidates.Count == 0)
                return;
            else if (Candidates.Count == 1)
                Set(Candidates[0], representative);
            else
            {
                SelectRepresentedClassDialog d = new SelectRepresentedClassDialog();
                d.cmbRepresentant.ItemsSource = Candidates;
                d.cmbRepresentant.SelectedValue = Candidates[0];
                d.ShowDialog();

                Set(d.cmbRepresentant.SelectedValue as PSMClass, representative);
            }
        }
        
        /// <summary>
        /// Prepares this command for execution.
        /// </summary>
        /// <param name="representedClass">Class to be represented</param>
        /// <param name="representativeHolder">ElementHolder containing the PSMClass to become PSMStructuralRepresentative</param>
        public void Set(PSMClass representedClass, ElementHolder<PSMClass> representativeHolder)
        {
            RepresentedClass = representedClass;
            Representative = representativeHolder;
        }

        /// <summary>
        /// Prepares this command for execution.
        /// </summary>
        /// <param name="representedClass">Class to be represented</param>
        /// <param name="representative">PSMClass to become PSMStructuralRepresentative</param>
        public void Set(PSMClass representedClass, PSMClass representative)
        {
            RepresentedClass = representedClass;
            Representative = new ElementHolder<PSMClass>() { Element = representative };
        }

        /// <summary>
		/// Checks whether Representative is not null.
        /// </summary>
    	public override bool CanExecute()
        {
            return Representative != null;
        }

    	internal override void CommandOperation()
        {
            OriginalRepresentedClass = Representative.Element.RepresentedPSMClass;
            Representative.Element.RepresentedPSMClass = RepresentedClass;
			AssociatedElements.Add(Representative.Element);
        }

    	internal override OperationResult UndoOperation()
        {
            Representative.Element.RepresentedPSMClass = OriginalRepresentedClass;
            return OperationResult.OK;
        }
    }

    #region SetRepresentedPSMClassCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="SetRepresentedPSMClassCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class SetRepresentedPSMClassCommandFactory : DiagramCommandFactory<SetRepresentedPSMClassCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private SetRepresentedPSMClassCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of SetRepresentedPSMClassCommand
        /// <param name="controller">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController controller)
        {
            return new SetRepresentedPSMClassCommand(controller);
        }
    }

    #endregion
}
