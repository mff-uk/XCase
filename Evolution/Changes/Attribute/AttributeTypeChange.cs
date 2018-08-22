using System;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    public class AttributeTypeChange : AttributeChange
    {
        public AttributeTypeChange(PSMAttribute attribute)
            : base(attribute)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public DataType OldType
        {
            get
            {
                return AttributeOldVersion.Type;
            }
        }

        public DataType NewType
        {
            get
            {
                return Attribute.Type;
            }
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(OldType != NewType);
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Attribute's {0:SN} type changed from {1:SN} to {2:SN}", Attribute, OldType, NewType);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAttribute psmAttribute)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMAttribute psmAttributeOldVersion = (PSMAttribute)psmAttribute.GetInVersion(v1);
            PSMAttribute psmAttributeNewVersion = ((PSMAttribute)psmAttribute.GetInVersion(v2));

                      

            if ((psmAttributeOldVersion.Type != null && psmAttributeNewVersion.Type == null) ||
                (psmAttributeOldVersion.Type == null && psmAttributeNewVersion.Type != null) ||
                (psmAttributeNewVersion.Type != null && psmAttributeOldVersion.Type != null &&
                 psmAttributeOldVersion.Type != psmAttributeNewVersion.Type.GetInVersion(v1)))
            {
                AttributeTypeChange c = new AttributeTypeChange(psmAttribute) { OldVersion = v1, NewVersion = v2 };
                result.Add(c);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { throw new NotImplementedException("InvalidatesAttributes not implemented in AttributeTypeChange."); }
        }

        public override bool InvalidatesContent
        {
            get { throw new NotImplementedException("InvalidatesContent not implemented in AttributeTypeChange."); }
        }   
    }
}