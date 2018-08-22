using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Copies all the nesting joins of the source associations to the target association.
    /// </summary>
    public class CopyNestingJoinsCommand : ModelCommandBase
    {
        /// <summary>
        /// Gets or sets the list of associations that the nesting joins
        /// will be copied from.
        /// </summary>
        private List<PSMAssociation> SourceAssociations { get; set; }

        /// <summary>
        /// Gets or sets the PSM association that the nesting joins
        /// will be copied to.
        /// </summary>
        private Helpers.ElementHolder<PSMAssociation> TargetAssociation { get; set; }

        /// <summary>
        /// List of copied nesting joins (for UndoOperation)
        /// </summary>
        private List<NestingJoin> copiedJoins;

        /// <summary>
        /// Creates a new command instance.
        /// </summary>
        /// <param name="controller">Associated model controller</param>
        public CopyNestingJoinsCommand(ModelController controller)
            : base(controller)
        {
            copiedJoins = new List<NestingJoin>();
        }

        /// <summary>
        /// Sets this command for use.
        /// </summary>
        /// <param name="targetAssociation">References the target association</param>
        /// <param name="sourceAssociations">List of source associations</param>
        public void Set(PSMAssociation targetAssociation, List<PSMAssociation> sourceAssociations)
        {
            SourceAssociations = sourceAssociations;
            TargetAssociation = new Helpers.ElementHolder<PSMAssociation>();
            TargetAssociation.Element = targetAssociation;
        }

        /// <summary>
        /// Sets this command for use.
        /// </summary>
        /// <param name="targetAssociationHolder">References the holder of the target association</param>
        /// <param name="sourceAssociations">List of source associations</param>
        public void Set(Helpers.ElementHolder<PSMAssociation> targetAssociationHolder, List<PSMAssociation> sourceAssociations)
        {
            SourceAssociations = sourceAssociations;
            TargetAssociation = targetAssociationHolder;
        }

        public override bool CanExecute()
        {
            return SourceAssociations != null && TargetAssociation != null && TargetAssociation.Element != null;
        }

        internal override void CommandOperation()
        {
            foreach (PSMAssociation source in SourceAssociations)
            {
                foreach (NestingJoin njSource in source.NestingJoins)
                {
                    TargetAssociation.Element.NestingJoins.Add(njSource);
                    copiedJoins.Add(njSource);
                }
            }
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            Debug.Assert(TargetAssociation != null && TargetAssociation.Element != null);

            foreach (NestingJoin nj in copiedJoins)
                TargetAssociation.Element.NestingJoins.Remove(nj);

            copiedJoins.Clear();

            return OperationResult.OK;
        }
    }

    #region CopyNestingJoinsCommandFactory

    public class CopyNestingJoinsCommandFactory : ModelCommandFactory<CopyNestingJoinsCommandFactory>
    {
        public override IStackedCommand Create(ModelController controller)
        {
            return new CopyNestingJoinsCommand(controller);
        }
    }

    #endregion
}
