using System.Diagnostics;
using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.Attribute, EEditType.Sedentary)]
    public abstract class AttributeChange : EvolutionChange
    {
        protected AttributeChange(PSMAttribute attribute)
        {
            Element = attribute;
        }

        public override EChangeScope Scope
        {
            get { return EChangeScope.Attribute; }
        }

        public PSMAttribute Attribute
        {
            get { return Element as PSMAttribute; }
        }

        public PSMAttribute AttributeOldVersion
        {
            get
            {
                return (PSMAttribute)Attribute.GetInVersion(OldVersion);
            }
        }

        public PSMClass PSMClass
        {
            get { return Attribute.Class; }
        }

        public PSMClass PSMClassOldVersion
        {
            get { return AttributeOldVersion.Class; }
        }

        public PSMAttributeContainer AttributeContainer
        {
            get { return Attribute.AttributeContainer; }
        }

        public PSMAttributeContainer AttributeContainerOldVersion
        {
            get
            {
                return AttributeOldVersion.AttributeContainer;
            }
        }

        public bool AttributeIsInClass
        {
            get { return AttributeContainer == null; }
        }

        public bool AttributeIsInAttributeContainer
        {
            get
            {
                return !AttributeIsInClass;
            }
        }

        public bool AttributeWasInClass
        {
            get { return AttributeContainerOldVersion == null; }
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(AttributeIsInAttributeContainer || AttributeIsInClass);
            Debug.Assert(AttributeContainerOldVersion != null || AttributeWasInClass);
        }
    }
}