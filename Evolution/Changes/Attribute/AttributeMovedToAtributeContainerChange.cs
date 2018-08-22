using System;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    /// <summary>
    /// Attribute was moved to an attribute container, it could have been 
    /// in a PSM class or in another attribute container. 
    /// </summary>
    /// <seealso cref="AttributeReturnedFromAtributeContainerChange"/>
    [ChangeProperties(EChangeScope.Attribute, EEditType.Migratory)]
    public class AttributeMovedToAtributeContainerChange : AttributeChange, IDoubleTargetChange
    {
        public AttributeMovedToAtributeContainerChange(PSMAttribute attribute)
            : base(attribute)
        {
        }

        public PSMElement Parent
        {
            get { return AttributeContainer; }
        }

        public PSMElement SecondaryTarget
        {
            get 
            {
                if (AttributeWasInClass)
                    return PSMClass;
                else
                    return AttributeContainer;
            }
        }

        public override EEditType EditType
        {
            get { return EEditType.Migratory; }
        }

        public bool MovedFromClass
        {
            get
            {
                return PSMClassOldVersion != null && AttributeContainerOldVersion == null;
            }
        }

        public bool MovedFromAnotherAttributeContainer
        {
            get
            {
                return AttributeContainerOldVersion != null;
            }

        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(AttributeIsInAttributeContainer);
            Debug.Assert(MovedFromAnotherAttributeContainer || MovedFromClass);
            Debug.Assert(!MovedFromAnotherAttributeContainer || !MovedFromClass);
        }

        public override string ToString()
        {
            if (MovedFromClass)
            {
                return string.Format(DispNullFormatProvider, "Attribute {0:SN} was moved from class {1:SN} to the attribute container {2:SN}",
                                     Attribute.XPath, PSMClassOldVersion, AttributeContainer);
            }
            else
            {
                Debug.Assert(MovedFromAnotherAttributeContainer);
                return string.Format(DispNullFormatProvider, "Attribute {0:SN} was moved from attribute container {1:SN} to the attribute container {2:SN}",
                                     Attribute.XPath, AttributeContainerOldVersion, AttributeContainer);
            }
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMAttribute psmAttribute)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMAttribute attributeV1 = (PSMAttribute)psmAttribute.GetInVersion(v1);
            PSMAttribute attributeV2 = (PSMAttribute)psmAttribute.GetInVersion(v2);

            if ((attributeV1.AttributeContainer == null && attributeV2.AttributeContainer != null) // moved from class
                || (attributeV1.AttributeContainer != null && attributeV2.AttributeContainer != null 
                    && attributeV1.AttributeContainer != attributeV2.AttributeContainer.GetInVersion(v1)))
                // moved from another AC
            {
                AttributeMovedToAtributeContainerChange c = new AttributeMovedToAtributeContainerChange(psmAttribute) { OldVersion = v1, NewVersion = v2 };
                result.Add(c);
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return AttributeWasInClass; }
        }

        public override bool InvalidatesContent
        {
            get { return true; }
        }
    }
}