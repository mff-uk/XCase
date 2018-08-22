using System;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    public class AttributeXSDImplementationChange : AttributeChange
    {
        public AttributeXSDImplementationChange(PSMAttribute attribute)
            : base(attribute)
        {

        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public string OldXSDImplementation
        {
            get
            {
                return AttributeOldVersion.XSDImplementation;
            }
        }

        public string NewXSDImplementation
        {
            get
            {
                return Attribute.XSDImplementation;
            }
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(OldXSDImplementation != NewXSDImplementation);
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Attribute's {0:SN} XSD implementatin changed. ", Attribute.XPath);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAttribute psmAttribute)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            if (((PSMAttribute)psmAttribute.GetInVersion(v1)).XSDImplementation != ((PSMAttribute)psmAttribute.GetInVersion(v2)).XSDImplementation)
            {
                AttributeXSDImplementationChange c = new AttributeXSDImplementationChange(psmAttribute) { OldVersion = v1, NewVersion = v2 };
                result.Add(c);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { throw new NotImplementedException("InvalidatesAttributes not implemented in AttributeXSDImplementationChange."); }
        }

        public override bool InvalidatesContent
        {
            get { throw new NotImplementedException("InvalidatesContent not implemented in AttributeXSDImplementationChange."); }
        }
    }
}