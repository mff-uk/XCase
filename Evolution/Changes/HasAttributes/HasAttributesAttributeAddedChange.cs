using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.HasAttributes, EEditType.Addition)]
    public class HasAttributesAttributeAddedChange: HasAttributesChange, ISubelementAditionChange
    {
        public PSMAttribute NewAttribute { get; private set; }

        public PSMElement ChangedSubelement
        {   
            get { return NewAttribute; }
        }

        public HasAttributesAttributeAddedChange(IHasPSMAttributes hasAttributes, PSMAttribute newAttribute) : base(hasAttributes)
        {
            NewAttribute = newAttribute;
        }

        public override EEditType EditType
        {
            get { return EEditType.Addition; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Attribute {0} added in {1}.", NewAttribute.XPath, HasAttributes);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(NewAttribute.GetInVersion(OldVersion) == null && NewAttribute.GetInVersion(NewVersion) == NewAttribute);
            Debug.Assert(HasAttributes.PSMAttributes.Contains(NewAttribute));
            Debug.Assert((HasAttributes.IsFirstVersion && HasAttributesOldVersion == null) ||
                         !HasAttributesOldVersion.PSMAttributes.Contains(NewAttribute));
            Debug.Assert(NewAttribute.Version == NewVersion);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, IHasPSMAttributes hasAttributes)
        {
            return (from psmAttribute in hasAttributes.PSMAttributes
                    where psmAttribute.GetInVersion(v1) == null && psmAttribute.GetInVersion(v2) != null
                    select new HasAttributesAttributeAddedChange(hasAttributes, psmAttribute) { OldVersion = v1, NewVersion = v2 } ).Cast<EvolutionChange>().ToList();
        }

        public override bool InvalidatesAttributes
        {
            get { return HasAttributes is PSMClass && NewAttribute.Lower > 0; }
        }

        public override bool InvalidatesContent
        {
            get { return HasAttributes is PSMAttributeContainer && NewAttribute.Lower > 0; }
        }
    }
}