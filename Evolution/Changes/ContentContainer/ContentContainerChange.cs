using System;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using Version=XCase.Model.Version;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.ContentContainer, EEditType.Sedentary)]
    public abstract class ContentContainerChange : EvolutionChange
    {
        public PSMContentContainer ContentContainer
        {
            get
            {
                return (PSMContentContainer)Element;
            }
        }

        public PSMContentContainer ContentContainerOldVersion
        {
            get
            {
                return (PSMContentContainer)Element.GetInVersion(OldVersion);
            }
        }

        protected ContentContainerChange(PSMContentContainer contentContainer)
        {
            Element = contentContainer;
        }

        public override EChangeScope Scope
        {
            get { return EChangeScope.ContentContainer; }
        }
    }

    public class ContentContainerRenamedChange : ContentContainerChange, IDoubleTargetChange
    {
        public ContentContainerRenamedChange(PSMContentContainer contentContainer) : base(contentContainer)
        {

        }

        public string OldName
        {
            get
            {
                return ContentContainerOldVersion.Name;
            }
        }

        public string NewName
        {
            get
            {
                return ContentContainer.Name;
            }
        }

        public override EEditType EditType
        {
            get { return EEditType.Sedentary; }
        }

        public override bool InvalidatesAttributes
        {
            get { return false; }
        }

        public override bool InvalidatesContent
        {
            get { return true; }
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(NewName != OldName);
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Content container {0} renamed from {1:SN} to {2:SN}.", ContentContainer, OldName, NewName);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMContentContainer contentContainer)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            if (((PSMContentContainer)contentContainer.GetInVersion(v1)).Name != ((PSMContentContainer)contentContainer.GetInVersion(v2)).Name)
            {
                ContentContainerRenamedChange c = new ContentContainerRenamedChange(contentContainer) { OldVersion = v1, NewVersion = v2 };
                result.Add(c);
            }

            return result;
        }

        public PSMElement SecondaryTarget
        {
            get
            {
                return PSMTreeIterator.GetSignificantAncestorOrSelf(ContentContainer.Parent);
            }
        }
    }
}