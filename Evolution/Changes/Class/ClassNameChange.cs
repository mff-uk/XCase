using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    /// 
    ///<summary>
    ///  Represents change in <see cref="PSMClass" />.<see cref="PSMClass.Name" />
    ///</summary>
    [ChangeProperties(EChangeScope.Class, EEditType.Sedentary, MayRequireRevalidation = false)]
    public class ClassNameChange : ClassChange
    {
        public ClassNameChange(PSMClass psmClass)
            : base(psmClass)
        {
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public string OldName
        {
            get { return PSMClassOldVersion.Name; }
        }

        public string NewName
        {
            get { return PSMClass.Name; }
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Class' {0:SN} name changed from '{1:SN}' to '{2:SN}'",
                                 PSMClass, OldName, NewName);
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(NewName != OldName);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMClass psmClass)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            if (((PSMClass) psmClass.GetInVersion(v1)).Name != ((PSMClass) psmClass.GetInVersion(v2)).Name)
            {
                ClassNameChange c = new ClassNameChange(psmClass) {OldVersion = v1, NewVersion = v2};
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