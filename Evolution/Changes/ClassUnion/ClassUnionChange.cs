using System;
using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.ClassUnion, EEditType.Migratory)]
    public abstract class ClassUnionChange : EvolutionChange
    {
        public PSMClassUnion ClassUnion
        {
            get
            {
                return (PSMClassUnion)Element;
            }
        }

        public PSMClassUnion ClassUnionOldVersion
        {
            get
            {
                return (PSMClassUnion)Element.GetInVersion(OldVersion);
            }
        }

        protected ClassUnionChange(PSMClassUnion classUnion)
        {
            Element = classUnion;
        }

        public override EChangeScope Scope
        {
            get { return EChangeScope.ClassUnion; }
        }
    }
}