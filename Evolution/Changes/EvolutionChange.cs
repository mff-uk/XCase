using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XCase.Model;
using Version = XCase.Model.Version;
using System;

namespace XCase.Evolution
{
    public abstract class EvolutionChange
    {
        protected internal static readonly DispNullFormatProvider DispNullFormatProvider = new DispNullFormatProvider();

        public Version OldVersion { get; set; }
        
        public Version NewVersion { get; set; }

        public abstract EChangeScope Scope { get; }

        public abstract EEditType EditType { get; }

        public PSMElement Element { get; protected set; }

        public override string ToString()
        {
            throw new InvalidOperationException(string.Format("Class {0} must override ToString!", base.ToString()));
        }

        public string ChangeType
        {
            get
            {
                return GetType().Name.Replace("Change", "");
            }
        }

        public virtual void Verify()
        {
            if (Scope != EChangeScope.Diagram)
            {
                Debug.Assert(Element.VersionManager != null && Element.Version != null && Element.FirstVersion != null);
            }

            IVersionedElement verifiedElement;

            if (this is ISubelementAditionChange)
            {
                verifiedElement = ((ISubelementAditionChange) this).ChangedSubelement;
            }
            else if (this is ISubelementRemovalChange)
            {
                verifiedElement = ((ISubelementRemovalChange) this).ChangedSubelement;
            }
            else 
            {
                verifiedElement = Element;
            }

            if (EditType.OneOf(EEditType.Addition, EEditType.Migratory, EEditType.Sedentary))
            {
                Debug.Assert(NewVersion != null);
                Debug.Assert(verifiedElement.GetInVersion(NewVersion) != null);
            }

            if (EditType.OneOf(EEditType.Removal, EEditType.Migratory, EEditType.Sedentary))
            {
                Debug.Assert(OldVersion != null);
                Debug.Assert(verifiedElement.ExistsInVersion(OldVersion));
            }

            if (EditType == EEditType.Addition)
            {
                Debug.Assert(NewVersion != null);
                Debug.Assert(verifiedElement.GetInVersion(OldVersion) == null);
            }

            if (EditType == EEditType.Removal)
            {
                Debug.Assert(OldVersion != null);
                Debug.Assert(verifiedElement.GetInVersion(NewVersion) == null);
            }
        }

        public abstract bool InvalidatesAttributes { get; }

        public abstract bool InvalidatesContent { get; }
    }
}   