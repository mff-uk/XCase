using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.HasAttributes, EEditType.Removal)]
    public class HasAttributesAttributeRemovedChange : HasAttributesChange, ISubelementRemovalChange 
    {
        public PSMAttribute RemovedAttribute { get; private set; }

        public PSMElement ChangedSubelement
        {
            get { return RemovedAttribute; }
        }

        public HasAttributesAttributeRemovedChange(IHasPSMAttributes hasAttributes, PSMAttribute removedAttribute)
            : base(hasAttributes)
        {
            RemovedAttribute = removedAttribute;
        }

        public override EEditType EditType
        {
            get { return EEditType.Removal; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Attribute {0} removed in {1}.", RemovedAttribute.XPath, HasAttributes);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(!HasAttributes.PSMAttributes.Contains(RemovedAttribute));
            Debug.Assert(HasAttributesOldVersion.PSMAttributes.Contains(RemovedAttribute));
            Debug.Assert(RemovedAttribute.GetInVersion(NewVersion) == null);
            Debug.Assert(RemovedAttribute.GetInVersion(OldVersion) == RemovedAttribute);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, IHasPSMAttributes hasAttributes)
        {
            return (from psmAttribute in ((IHasPSMAttributes)hasAttributes.GetInVersion(v1)).PSMAttributes
                    where psmAttribute.GetInVersion(v1) != null && psmAttribute.GetInVersion(v2) == null
                    select new HasAttributesAttributeRemovedChange(hasAttributes, psmAttribute) { OldVersion = v1, NewVersion = v2 }).Cast<EvolutionChange>().ToList();
        }

        public override bool InvalidatesAttributes
        {
            get { return HasAttributes is PSMClass; }
        }

        public override bool InvalidatesContent
        {
            get { return HasAttributes is PSMAttributeContainer; }
        }
    }
}