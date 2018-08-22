using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.Attribute, EEditType.Sedentary, MayRequireRevalidation = false)]
    public class AttributeDefaultChange : AttributeChange
    {
        public AttributeDefaultChange(PSMAttribute attribute)
            : base(attribute)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public string OldDefault
        {
            get
            {
                return AttributeOldVersion.Default;
            }
        }

        public string NewDefault
        {
            get
            {
                return Attribute.Default;
            }
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(OldDefault != NewDefault);
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Attribute's {0:SN} default value changed from {1:SN} to {2:SN}", Attribute.XPath, OldDefault, NewDefault);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAttribute psmAttribute)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            if (((PSMAttribute)psmAttribute.GetInVersion(v1)).Default != ((PSMAttribute)psmAttribute.GetInVersion(v2)).Default)
            {
                AttributeDefaultChange c = new AttributeDefaultChange(psmAttribute) { OldVersion = v1, NewVersion = v2 };
                result.Add(c);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return false; }
        }

        public override bool InvalidatesContent
        {
            get { return false; }
        }
    }
}