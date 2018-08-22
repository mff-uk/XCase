using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    public class AttributeAliasChange : AttributeChange
    {
        public AttributeAliasChange(PSMAttribute attribute)
            : base(attribute)
        {
        }

        public string OldAlias
        {
            get
            {
                return AttributeOldVersion.Alias;
            }
        }

        public string NewAlias
        {
            get
            {
                return Attribute.Alias;
            }
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Attribute's {0:SN} alias changed from {1:SN} to {2:SN}", Attribute.XPath, OldAlias, NewAlias);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(NewAlias != OldAlias);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAttribute psmAttribute)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            if (((PSMAttribute)psmAttribute.GetInVersion(v1)).Alias != ((PSMAttribute)psmAttribute.GetInVersion(v2)).Alias)
            {
                AttributeAliasChange c = new AttributeAliasChange(psmAttribute) { OldVersion = v1, NewVersion = v2 };
                result.Add(c);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return AttributeIsInClass; }
        }

        public override bool InvalidatesContent
        {
            get { return AttributeIsInAttributeContainer; }
        }
    }
}