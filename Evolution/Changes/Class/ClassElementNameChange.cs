using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    /// 
    ///<summary>
    ///  Represents change in
    ///  <see cref="PSMClass" />.<see cref="PSMClass.ElementName" />.
    ///</summary>
    public class ClassElementNameChange : ClassChange, IDoubleTargetChange, IChangeWithEditTypeOverride
    {
        public ClassElementNameChange(PSMClass psmClass)
            : base(psmClass)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary;  }
        }

        public EEditType EditTypeOverride
        {
            get 
            { 
                if (ElementLabelAdded)
                    return EEditType.Addition;
                if (ElementLabelRemoved)
                    return EEditType.Removal;
                else
                    return EEditType.Sedentary;
            }
        }

        public bool ElementLabelRemoved
        {
            get { return !string.IsNullOrEmpty(OldElementName) && string.IsNullOrEmpty(NewElementName); }
        }

        public bool ElementLabelAdded
        {
            get { return string.IsNullOrEmpty(OldElementName) && !string.IsNullOrEmpty(NewElementName); }
        }

        public string OldElementName
        {
            get { return PSMClassOldVersion.ElementName; }
        }

        public string NewElementName
        {
            get { return PSMClass.ElementName; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Class' {0:SN} element name changed from '{1:SN}' to '{2:SN}'",
                                 PSMClass,
                                 OldElementName, NewElementName);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(NewElementName != OldElementName);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMClass psmClass)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            if (((PSMClass) psmClass.GetInVersion(v1)).ElementName != ((PSMClass) psmClass.GetInVersion(v2)).ElementName)
            {
                ClassElementNameChange c = new ClassElementNameChange(psmClass) {OldVersion = v1, NewVersion = v2};
                result.Add(c);
            }

            return result;
        }

        public PSMElement SecondaryTarget
        {
            get 
            {
                if (!PSMClassOldVersion.HasElementLabel)
                    return PSMTreeIterator.GetSignificantAncestorOrSelf((PSMElement)PSMClass.ParentUnion ?? PSMClass.ParentAssociation);
                else
                    return PSMTreeIterator.GetSignificantAncestorOrSelf(PSMClass);
            }
        }

        public override bool InvalidatesAttributes
        {
            get { return PSMClass.EncompassesAttributesForParentSignificantNode(); }
        }

        public override bool InvalidatesContent
        {
            get { return true; }
        }
    }
}