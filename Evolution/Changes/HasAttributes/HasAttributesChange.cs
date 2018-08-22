using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.HasAttributes, EEditType.Sedentary)]
    public abstract class HasAttributesChange : EvolutionChange
    {
        protected HasAttributesChange(IHasPSMAttributes hasAttributes)
        {
            Element = hasAttributes;
        }

        public override EChangeScope Scope
        {
            get { return EChangeScope.HasAttributes; }
        }

        public IHasPSMAttributes HasAttributes
        {
            get { return Element as IHasPSMAttributes; }
        }

        public IHasPSMAttributes HasAttributesOldVersion
        {
            get { return (IHasPSMAttributes)HasAttributes.GetInVersion(OldVersion); }
        }
    }
}