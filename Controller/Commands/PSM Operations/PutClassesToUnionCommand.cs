using System.Collections.Generic;
using XCase.Model;

namespace XCase.Controller.Commands
{
    public class PutClassesToUnionCommand : ModelCommandBase
    {
        public List<PSMAssociation> Associations { get; set; }

        private Helpers.ElementHolder<PSMClassUnion> Union { get; set; }

        private readonly List<PSMClass> movedClasses;

        public PutClassesToUnionCommand(ModelController controller) : base(controller)
        {
            movedClasses = new List<PSMClass>();
        }
        
        public override bool CanExecute()
        {
            return Associations != null && Associations.Count > 0 && Union != null && Union.HasValue;
        }

        public void Set(IEnumerable<PSMAssociation> associations, Helpers.ElementHolder<PSMClassUnion> union)
        {
            Union = union;

            Associations = new List<PSMAssociation>(associations);
        }

        internal override void CommandOperation()
        {
            foreach (PSMAssociation assoc in Associations)
            {
                PSMClass cls = assoc.Child as PSMClass;
                if (cls != null)
                {
                    Union.Element.Components.Add(cls);
                    movedClasses.Add(cls);
                }
            }
        }

        internal override OperationResult UndoOperation()
        {
            foreach (PSMClass cls in movedClasses)
            {
                Union.Element.Components.Remove(cls);
            }
            movedClasses.Clear();

            return OperationResult.OK;
        }
    }

    #region PutClassesToUnionCommandFactory

    public class PutClassesToUnionCommandFactory : ModelCommandFactory<PutClassesToUnionCommandFactory>
    {
        public override IStackedCommand Create(ModelController controller)
        {
            return new PutClassesToUnionCommand(controller);
        }
    }

    #endregion
}