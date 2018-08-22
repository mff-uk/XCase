using System;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.Attribute, EEditType.Migratory)]
    public class AttributeReturnedFromAtributeContainerChange : AttributeChange
    {
        public AttributeReturnedFromAtributeContainerChange(PSMAttribute attribute)
            : base(attribute)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Migratory; }
        }

        public PSMElement Parent
        {
            get { return PSMClass; }
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(AttributeContainer == null && AttributeContainerOldVersion != null);
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Attribute {0:SN} was moved from the attribute container {1:SN} back to class {2:SN}",
                                 Attribute.XPath, AttributeContainerOldVersion, PSMClass);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAttribute psmAttribute)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMAttribute attributeV1 = (PSMAttribute)psmAttribute.GetInVersion(v1);
            PSMAttribute attributeV2 = (PSMAttribute)psmAttribute.GetInVersion(v2);

            if (attributeV1.AttributeContainer != null && attributeV2.AttributeContainer == null) 
            {
                AttributeReturnedFromAtributeContainerChange c = new AttributeReturnedFromAtributeContainerChange(psmAttribute) { OldVersion = v1, NewVersion = v2 };
                result.Add(c);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return true; }
        }

        public override bool InvalidatesContent
        {
            get { return true; }
        }
    }
}