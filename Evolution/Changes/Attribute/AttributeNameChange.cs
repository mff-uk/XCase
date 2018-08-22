using System;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    public class AttributeNameChange : AttributeChange
    {
        public AttributeNameChange(PSMAttribute attribute)
            : base(attribute)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public string OldName
        {
            get
            {
                return AttributeOldVersion.Name;
            }
        }

        public string NewName
        {
            get
            {
                return Attribute.Name;
            }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Attribute's {0:SN} name changed from {1:SN} to {2:SN}", Attribute.XPath, OldName, NewName);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(NewName != OldName);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAttribute psmAttribute)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            if (((PSMAttribute)psmAttribute.GetInVersion(v1)).Name != ((PSMAttribute)psmAttribute.GetInVersion(v2)).Name)
            {
                AttributeNameChange c = new AttributeNameChange(psmAttribute) { OldVersion = v1, NewVersion = v2 };
                result.Add(c);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return AttributeIsInClass && String.IsNullOrEmpty(Attribute.Alias); }
        }

        public override bool InvalidatesContent
        {
            get { return AttributeIsInAttributeContainer && String.IsNullOrEmpty(Attribute.Alias); }
        }
    }
}