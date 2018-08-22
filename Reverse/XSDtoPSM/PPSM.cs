using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using System.Collections.ObjectModel;
using System.Xml;
using XCase.Model;

namespace XCase.Reverse
{
    public interface I_PSMHasParent
    {
        I_PSMHasChildren Parent { get; set; }
        List<I_PSMHasChildren> Ancestors { get; }
    }

    public interface I_PSMHasChildren
    {
        ObservableCollection<I_PSMHasParent> Children {get; }
        ObservableCollection<XmlQualifiedName> GroupRefs { get; }
    }

    public interface I_PSMHasAttributes
    {
        ObservableCollection<P_PSMAttribute> Attributes { get; }
        ObservableCollection<XmlQualifiedName> AttrGroupRefs { get; }
    }

    public class P_PSMDiagram : I_PSMHasChildren
    {
        public string TargetNamespace;
        
        ObservableCollection<I_PSMHasParent> children;
        ObservableCollection<XmlQualifiedName> groupRefs { get; set; }
        public ObservableCollection<XmlQualifiedName> GroupRefs { get { return groupRefs; } }
        public ObservableCollection<I_PSMHasParent> Children { get { return children; } } //roots
        /// <summary>
        /// Provides translation from the P_PSMClass name to the P_PSMClass
        /// </summary>
        public Dictionary<XmlQualifiedName, P_PSMClass> GlobalIDs;
        /// <summary>
        /// List of names that cannot be given to a new P_PSMClass
        /// </summary>
        public List<XmlQualifiedName> UsedNames;
        /// <summary>
        /// Provides translation from the XML Schema global element name to the P_PSMClass name representing its type
        /// The actual P_PSMClass can be then found in GlobalIDs
        /// </summary>
        public Dictionary<XmlQualifiedName, XmlQualifiedName> GlobalElementTypes;
        /// <summary>
        /// So far unused... should provide mapping between type names and simple types
        /// </summary>
        public Dictionary<XmlQualifiedName, XmlSchemaSimpleType> SimpleTypes;

        public Dictionary<XmlQualifiedName, List<P_PSMClass>> Extensions;

        public XmlSchemaSet SchemaSet;

        public P_PSMDiagram()
        {
            children = new ObservableCollection<I_PSMHasParent>();
            GlobalIDs = new Dictionary<XmlQualifiedName, P_PSMClass>();
            SimpleTypes = new Dictionary<XmlQualifiedName, XmlSchemaSimpleType>();
            groupRefs = new ObservableCollection<XmlQualifiedName>();
            UsedNames = new List<XmlQualifiedName>();
            GlobalElementTypes = new Dictionary<XmlQualifiedName, XmlQualifiedName>();
            Extensions = new Dictionary<XmlQualifiedName, List<P_PSMClass>>();
        }
    }
    
    public class P_PSMBase: I_PSMHasParent, I_PSMHasChildren
    {
        public PSMSuperordinateComponent Super;
        I_PSMHasChildren parent;
        ObservableCollection<I_PSMHasParent> children;
        ObservableCollection<XmlQualifiedName> groupRefs { get; set; }
        public ObservableCollection<XmlQualifiedName> GroupRefs { get { return groupRefs; } }
        public string AppInfo;
        public List<string> Documentation = new List<string>();
        public ObservableCollection<I_PSMHasParent> Children { get { return children; } }
        public I_PSMHasChildren Parent { get { return parent; } set { parent = value; } }

        public List<I_PSMHasChildren> Ancestors
        {
            get
            {
                List<I_PSMHasChildren> ancestors = new List<I_PSMHasChildren>();
                I_PSMHasChildren temp = this.Parent;
                while (temp != null)
                {
                    if (ancestors.Contains(temp)) break;
                    else ancestors.Add(temp);
                    if (temp is I_PSMHasParent) temp = (temp as I_PSMHasParent).Parent;
                    else break;
                }
                return ancestors;
            }
        }
        
        protected P_PSMBase()
        {
            children = new ObservableCollection<I_PSMHasParent>();
            groupRefs = new ObservableCollection<XmlQualifiedName>();
        }
    }
    
    public class P_PSMClass: P_PSMBase, I_PSMHasAttributes
    {
        public Model.PSMClass PSMClass;
        public XmlQualifiedName Name;
        public string ElementLabel;
        public uint MinOccurs;
        public NUml.Uml2.UnlimitedNatural MaxOccurs;
        public XmlQualifiedName ExtensionOf;
        public XmlQualifiedName RestrictionOf;
        public bool CreatedAsExtensionSR = false;
        public XmlQualifiedName SRofType;
        public XmlQualifiedName SRofElemRef;
        ObservableCollection<P_PSMAttribute> attributes;
        ObservableCollection<XmlQualifiedName> attrGroupRefs;
        public ObservableCollection<XmlQualifiedName> AttrGroupRefs { get { return attrGroupRefs; } }
        public ObservableCollection<P_PSMAttribute> Attributes { get { return attributes; } }
        public ObservableCollection<P_PSMClass> SRepresentedBy = new ObservableCollection<P_PSMClass>();

        public P_PSMClass()
        {
            attributes = new ObservableCollection<P_PSMAttribute>();
            attrGroupRefs = new ObservableCollection<XmlQualifiedName>();
        }
    }

    public class P_PSMDummy : P_PSMBase
    { }

    public class P_PSMComment : I_PSMHasParent
    {
        I_PSMHasChildren parent;
        public I_PSMHasChildren Parent { get { return parent; } set { parent = value; } }

        public string text;

        public List<I_PSMHasChildren> Ancestors
        {
            get
            {
                List<I_PSMHasChildren> ancestors = new List<I_PSMHasChildren>();
                I_PSMHasChildren temp = this.Parent;
                while (temp != null)
                {
                    if (ancestors.Contains(temp)) break;
                    else ancestors.Add(temp);
                    if (temp is I_PSMHasParent) temp = (temp as I_PSMHasParent).Parent;
                    else break;
                }
                return ancestors;
            }
        }

    }

    public class P_PSMContentContainer : P_PSMBase
    {
        public string ElementLabel;
        public P_PSMClass P_PSMClass
        {
            get
            {
                I_PSMHasChildren tmpParent = Parent;
                while (!(tmpParent is P_PSMClass))
                    if (tmpParent is I_PSMHasParent)
                        tmpParent = (tmpParent as I_PSMHasParent).Parent;
                    else return null;
                return tmpParent as P_PSMClass;
            }
        }
    }
    
    public class P_PSMContentChoice : P_PSMBase
    {
        public P_PSMClass P_PSMClass
        {
            get
            {
                I_PSMHasChildren tmpParent = Parent;
                while (!(tmpParent is P_PSMClass))
                    if (tmpParent is I_PSMHasParent)
                        tmpParent = (tmpParent as I_PSMHasParent).Parent;
                    else return null;
                return tmpParent as P_PSMClass;
            }
        }
    }

    public class P_PSMAttributeContainer : I_PSMHasParent, I_PSMHasAttributes
    {
        I_PSMHasChildren parent;
        public I_PSMHasChildren Parent { get { return parent; } set { parent = value; } }

        ObservableCollection<P_PSMAttribute> attributes;
        ObservableCollection<XmlQualifiedName> attrGroupRefs;
        public ObservableCollection<XmlQualifiedName> AttrGroupRefs { get { return attrGroupRefs; } }

        public ObservableCollection<P_PSMAttribute> Attributes { get { return attributes; } }

        public P_PSMAttributeContainer()
        {
            attributes = new ObservableCollection<P_PSMAttribute>();
            attrGroupRefs = new ObservableCollection<XmlQualifiedName>();
        }

        public List<I_PSMHasChildren> Ancestors
        {
            get
            {
                List<I_PSMHasChildren> ancestors = new List<I_PSMHasChildren>();
                I_PSMHasChildren temp = this.Parent;
                while (temp != null)
                {
                    if (ancestors.Contains(temp)) break;
                    else ancestors.Add(temp);
                    if (temp is I_PSMHasParent) temp = (temp as I_PSMHasParent).Parent;
                    else break;
                }
                return ancestors;
            }
        }
    }

    public class P_PSMAttribute
    {
        public string Alias;
        public XmlQualifiedName type;
        public uint Lower;
        public NUml.Uml2.UnlimitedNatural Upper;
        public string FixedValue, DefaultValue;
        public XmlSchemaForm Form;

    }

    public class P_SimpleType
    {
        public string temp;
    }

}
