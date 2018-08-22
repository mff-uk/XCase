using System;
using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.Subordinate, EEditType.Migratory)]
    public abstract class SubordinateChange : EvolutionChange
    {
        public PSMSubordinateComponent Subordinate
        {
            get
            {
                return (PSMSubordinateComponent)Element;
            }
        }

        public PSMSubordinateComponent SubordinateOldVersion
        {
            get
            {
                return (PSMSubordinateComponent)Element.GetInVersion(OldVersion);
            }
        }

        protected SubordinateChange(PSMSubordinateComponent subordinate)
        {
            Element = subordinate;
        }

        public override EChangeScope Scope
        {
            get { return EChangeScope.Subordinate; }
        }
    }
}