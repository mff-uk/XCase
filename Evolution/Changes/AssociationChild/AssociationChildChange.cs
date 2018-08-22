using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.AssociationChild, EEditType.Sedentary)]
    public abstract class AssociationChildChange : EvolutionChange
    {
        public PSMAssociationChild AssociationChild
        {
            get
            {
                return (PSMAssociationChild)Element;
            }
        }

        public PSMAssociationChild AssociationChildOldVersion
        {
            get
            {
                return (PSMAssociationChild)Element.GetInVersion(OldVersion);
            }
        }

        protected AssociationChildChange(PSMAssociationChild associationChild)
        {
            Element = associationChild;
        }

        public override EChangeScope Scope
        {
            get { return EChangeScope.AssociationChild; }
        }
    }
}