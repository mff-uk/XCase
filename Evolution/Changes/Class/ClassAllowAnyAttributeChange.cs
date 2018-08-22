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
    ///  <see cref="PSMClass.AllowAnyAttribute" />
    ///  .
    ///</summary>
    public class ClassAllowAnyAttributeChange : ClassChange
    {
        public ClassAllowAnyAttributeChange(PSMClass psmClass)
            : base(psmClass)
        {
        }

        public bool OldAllowAnyAttribute
        {
            get { return PSMClassOldVersion.AllowAnyAttribute; }
        }

        public bool NewAllowAnyAttribute
        {
            get { return PSMClass.AllowAnyAttribute; }
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider,
                                 "Class' {0:SN} allow any attribute changed from {1:SN} to {2:SN}",
                                 PSMClass, OldAllowAnyAttribute, NewAllowAnyAttribute);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(NewAllowAnyAttribute != OldAllowAnyAttribute);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMClass psmClass)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            if (((PSMClass) psmClass.GetInVersion(v1)).AllowAnyAttribute !=
                ((PSMClass) psmClass.GetInVersion(v2)).AllowAnyAttribute)
            {
                ClassAllowAnyAttributeChange c = new ClassAllowAnyAttributeChange(psmClass) {OldVersion = v1, NewVersion = v2};
                result.Add(c);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return OldAllowAnyAttribute && !NewAllowAnyAttribute; }
        }

        public override bool InvalidatesContent
        {
            get { return false; }
        }
    }
}