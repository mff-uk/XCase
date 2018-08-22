using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.Class, EEditType.Sedentary)]
    public abstract class ClassChange : EvolutionChange
    {
        protected ClassChange(PSMClass psmClass)
        {
            Element = psmClass;
        }

        public override EChangeScope Scope
        {
            get { return EChangeScope.Class; }
        }

        public PSMClass PSMClass
        {
            get { return Element as PSMClass; }
        }

        public PSMClass PSMClassOldVersion
        {
            get { return (PSMClass) PSMClass.GetInVersion(OldVersion); }
        }

        public override void Verify()
        {
            base.Verify();
        }
    }
}