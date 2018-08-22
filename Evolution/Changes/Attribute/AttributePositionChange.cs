using System;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    /// <summary>
    /// Attribute remains in the same psm class/attribute container, but the position
    /// among the attributes is changed. 
    /// </summary>
    [ChangeProperties(EChangeScope.Attribute, EEditType.Migratory)]
    public class AttributePositionChange : AttributeChange, IDoubleTargetChange
    {
        public AttributePositionChange(PSMAttribute attribute)
            : base(attribute)
        {
        }

        public PSMElement Parent
        {
            get { return AttributeIsInClass ? (PSMElement)PSMClass : AttributeContainer; }
        }

        public override EEditType EditType
        {
            get { return EEditType.Migratory; }
        }

        public int OldPosition
        {
            get
            {
                if (AttributeIsInClass)
                {
                    Debug.Assert(PSMClass != null);
                    return PSMClassOldVersion.PSMAttributes.IndexOf(AttributeOldVersion);
                }
                else
                {
                    Debug.Assert(AttributeContainerOldVersion != null);
                    return AttributeContainerOldVersion.PSMAttributes.IndexOf(AttributeOldVersion);
                }
            }
        }

        public int NewPosition
        {
            get
            {
                if (AttributeIsInClass)
                {
                    Debug.Assert(PSMClass != null);
                    return PSMClass.PSMAttributes.IndexOf(Attribute);
                }
                else
                {
                    Debug.Assert(AttributeIsInAttributeContainer);
                    Debug.Assert(AttributeContainerOldVersion != null);
                    return AttributeContainer.PSMAttributes.IndexOf(Attribute);
                }
            }
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(NewPosition != OldPosition);
            Debug.Assert((PSMClassOldVersion != null && PSMClass != null) || (AttributeContainerOldVersion != null && AttributeIsInClass));
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Attribute's {0:SN} position changed from {1} to {2}. ", Attribute.XPath, OldPosition, NewPosition);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAttribute psmAttribute)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            bool inClass = psmAttribute.AttributeContainer == null;
            IHasPSMAttributes containerV1, containerV2;

            PSMAttribute attributeV1 = (PSMAttribute)psmAttribute.GetInVersion(v1);
            PSMAttribute attributeV2 = (PSMAttribute)psmAttribute.GetInVersion(v2);

            if (attributeV1.AttributeContainer != null && attributeV2.AttributeContainer == null ||
                attributeV1.AttributeContainer == null && attributeV2.AttributeContainer != null ||
                attributeV1.AttributeContainer != null && attributeV2.AttributeContainer != null && attributeV2.AttributeContainer.GetInVersion(v1) != attributeV1.AttributeContainer)
                // moved to different class/container
                return result;


            if (inClass)
            {
                Debug.Assert(psmAttribute.AttributeContainer == null);
                containerV1 = attributeV1.Class;
                containerV2 = attributeV2.Class;
            }
            else
            {
                containerV1 = attributeV1.AttributeContainer;
                containerV2 = attributeV2.AttributeContainer;
            }

            if (containerV1 != null && containerV2 != null &&
                containerV1.PSMAttributes.IndexOf(attributeV1) != containerV2.PSMAttributes.IndexOf(attributeV2))
            {
                AttributePositionChange c = new AttributePositionChange(psmAttribute) { OldVersion = v1, NewVersion = v2 };
                result.Add(c);
            }

            return result;
        }

        public PSMElement SecondaryTarget
        {
            get { return Parent; }
        }

        public override bool InvalidatesAttributes
        {
            get { return false; }
        }

        public override bool InvalidatesContent
        {
            get { return AttributeIsInAttributeContainer; }
        }
    }
}