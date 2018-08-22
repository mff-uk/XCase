using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    /// 
    ///<summary>
    ///  Represents change in
    ///  <see cref="PSMClass" />
    ///  .
    ///  <see cref="PSMClass.IsStructuralRepresentative" />
    ///  .
    ///</summary>
    public class ClassIsStructuralRepresentativeChange : ClassChange
    {
        public ClassIsStructuralRepresentativeChange(PSMClass psmClass)
            : base(psmClass)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public bool OldIsStructuralRepresentative
        {
            get { return PSMClassOldVersion.IsStructuralRepresentative; }
        }

        public bool NewIsStructuralRepresentative
        {
            get { return PSMClass.IsStructuralRepresentative; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider,
                                 "Class' {0:SN} is structural representative changed from {1:SN} to {2:SN}",
                                 PSMClass, OldIsStructuralRepresentative, NewIsStructuralRepresentative);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(NewIsStructuralRepresentative != OldIsStructuralRepresentative);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMClass psmClass)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            if (((PSMClass) psmClass.GetInVersion(v1)).IsStructuralRepresentative !=
                ((PSMClass) psmClass.GetInVersion(v2)).IsStructuralRepresentative)
            {
                ClassIsStructuralRepresentativeChange c = new ClassIsStructuralRepresentativeChange(psmClass) {OldVersion = v1, NewVersion = v2};
                result.Add(c);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get
            {
                if (PSMClass.IsStructuralRepresentative && !PSMClassOldVersion.IsStructuralRepresentative)
                {
                    return PSMClass.RepresentedPSMClass.EncompassesAttributesForParentSignificantNode() || PSMClass.RepresentedPSMClass.Attributes.Count > 0;
                }
                else
                {
                    Debug.Assert(!PSMClass.IsStructuralRepresentative && PSMClassOldVersion.IsStructuralRepresentative);
                    return PSMClassOldVersion.RepresentedPSMClass.EncompassesAttributesForParentSignificantNode() || PSMClassOldVersion.RepresentedPSMClass.Attributes.Count > 0;
                }
            }
        }

        public override bool InvalidatesContent
        {
            get
            {
                if (PSMClass.IsStructuralRepresentative && !PSMClassOldVersion.IsStructuralRepresentative)
                {
                    return PSMClass.RepresentedPSMClass.EncompassesContentForParentSignificantNode();
                }
                else
                {
                    Debug.Assert(!PSMClass.IsStructuralRepresentative && PSMClassOldVersion.IsStructuralRepresentative);
                    return PSMClassOldVersion.RepresentedPSMClass.EncompassesContentForParentSignificantNode();
                }
            }
        }
    }
}