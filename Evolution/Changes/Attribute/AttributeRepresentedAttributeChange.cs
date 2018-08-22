using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.Attribute, EEditType.Sedentary, MayRequireRevalidation = false)]
    public class AttributeRepresentedAttributeChange : AttributeChange
    {
        public AttributeRepresentedAttributeChange(PSMAttribute attribute)
            : base(attribute)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public Property OldRepresentedAttribute
        {
            get
            {
                return AttributeOldVersion.RepresentedAttribute;
            }
        }

        public Property NewRepresentedAttribute
        {
            get
            {
                return Attribute.RepresentedAttribute;
            }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Attribute's {0:SN} represented attribute changed from {1:SN} to {2:SN}", Attribute, OldRepresentedAttribute, NewRepresentedAttribute);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAttribute psmAttribute)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMAttribute psmAttributeOldVersion = (PSMAttribute)psmAttribute.GetInVersion(v1);
            PSMAttribute psmAttributeNewVersion = (PSMAttribute)psmAttribute.GetInVersion(v2);
            Debug.Assert(psmAttributeNewVersion == psmAttribute);
            if (
                (psmAttributeOldVersion.RepresentedAttribute == null && psmAttributeNewVersion.RepresentedAttribute != null)
                || (psmAttributeOldVersion.RepresentedAttribute != null && psmAttributeNewVersion.RepresentedAttribute == null)
                || (
                       psmAttributeOldVersion.RepresentedAttribute != null 
                       && psmAttributeNewVersion.RepresentedAttribute != null
                       && psmAttributeOldVersion.RepresentedAttribute != psmAttributeNewVersion.RepresentedAttribute.GetInVersion(v1)
                   )
                )
            {
                AttributeRepresentedAttributeChange c = new AttributeRepresentedAttributeChange(psmAttribute) { OldVersion = v1, NewVersion = v2 };
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