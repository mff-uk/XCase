using System;
using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.Superordinate, EEditType.Sedentary)]
    public abstract class SuperordinateChange : EvolutionChange
    {
        public PSMSuperordinateComponent Superordinate
        {
            get
            {
                return (PSMSuperordinateComponent)Element;
            }
        }

        public PSMSuperordinateComponent SuperordinateOldVersion
        {
            get
            {
                return (PSMSuperordinateComponent)Element.GetInVersion(OldVersion);
            }
        }

        protected SuperordinateChange(PSMSuperordinateComponent superordinate)
        {
            Element = superordinate;
        }

        public override EChangeScope Scope
        {
            get { return EChangeScope.Superordinate; }
        }
    }
}