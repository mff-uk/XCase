using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using NUml.Uml2;
using System.Text;
using XCase.Model;

namespace XCase.Translation.XmlSchema
{
	/// <summary>
	/// Translates PSM Diagram into XML schema in XML Schema language. 
	/// The algorithm of translation is covered in detail in the 
	/// documentation to <c>XCase</c>. 
	/// </summary>
	public partial class XmlSchemaTranslator: DiagramTranslator<XmlSchemaTranslator.Context, string>
	{
		/// <summary>
		/// Data for classes that were or are about to be translated.
		/// </summary>
		private Dictionary<PSMClass, ClassTranslationData> classesTranslationData; 

		/// <summary>
		/// <see cref="NamingSupport"/> class is used to translate type names and attribute 
		/// aliases in distinct names to avoid ambiguous references. 
		/// </summary>
		private readonly NamingSupport namingSupport;
        private readonly Configuration config;
        private Dictionary<string, XsdDocument> documents;  // indexed by name of XCase package
        private XsdDocument defaultDoc;
        public Dictionary<XsdDocument, string> locations;
        private string projectName;

		/// <summary>
		/// Holds amount of currently opened &lt;xs:choice.. declarations. 
		/// </summary>
		/// <seealso cref="inChoice"/>
		private int choiceCounter = 0;

		/// <summary>
		/// Gets a value indicating whether there is currently an opened 
		/// &lt;xs:choice.. declaration. When inChoice is <c>true</c>, translation 
		/// of attribute groups writes "opt groups" instead normal groups. 
		/// This topic is covered in detail in the documentation for <c>XCase</c>
		/// </summary>
		private bool inChoice
		{
			get { return choiceCounter > 0; }
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSchemaTranslator"/> class.
        /// </summary>
        public XmlSchemaTranslator(Configuration config, string projectName)
        {
            this.projectName = projectName;
            this.config = config;

            classesTranslationData = new Dictionary<PSMClass, ClassTranslationData>();
            documents = new Dictionary<string, XsdDocument>();
            locations = new Dictionary<XsdDocument, string>();
            namingSupport = new NamingSupport { Log = Log };
            namingSupport.Initialize();
            Log.Clear();

            string targetNamespace = namingSupport.GetNamespaceURIForPackage("default", config.getNamespacePrefix(), (config.useProjectNameInNamespace() ? projectName : ""));
            defaultDoc = new XsdDocument(targetNamespace, config) { Log = Log };
            XmlElement currentElement = defaultDoc.getSchemaElement();
            currentElement.SetAttribute("xmlns", targetNamespace);
            currentElement.SetAttribute("elementFormDefault", config.getElementFormDefault());

            string xsdFileName = namingSupport.GetXsdFileNameForPackage("default", projectName);
            locations[defaultDoc] = xsdFileName;
            documents.Add("default", defaultDoc);
        }

        private XsdDocument getDefaultDocument()
        {
            return this.documents["default"];
        }
        private XsdDocument getAppropriateDocument(PSMClass psmClass)
        {
            string packName = null;
            if (psmClass.Package.NestingPackage == null)
                packName = "default";
            else
                packName = (config.isUsingNamespaces() ? psmClass.Package.QualifiedName : "default");
            if (!documents.Keys.Contains(packName))
            {
                // there is no XSD for that package yet, let's create one
                string targetNamespace = namingSupport.GetNamespaceURIForPackage(packName, config.getNamespacePrefix(), (config.useProjectNameInNamespace() ? projectName : ""));

                XsdDocument newXsd = new XsdDocument(targetNamespace, config) { Log = Log };
                XmlElement currentElement = newXsd.getSchemaElement();
                currentElement.SetAttribute("elementFormDefault", config.getElementFormDefault());
                currentElement.SetAttribute("xmlns", targetNamespace);

                // store new XSD document permanently
                this.documents[packName] = newXsd;

                string xsdFileName = namingSupport.GetXsdFileNameForPackage(packName, (config.useProjectNameInNamespace() ? projectName : ""));
                this.locations[newXsd] = xsdFileName;
            }
            return this.documents[packName];
        }
        public Dictionary<string,string> getResults()
        {
            Dictionary<string, string> schemas = new Dictionary<string, string>();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.NewLineOnAttributes = true;
            
            foreach (XsdDocument dok in documents.Values)
            {
                if (dok.isSchemaEmpty())
                    continue;

                // write with indentation
                StringBuilder sb = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(sb, settings);

                dok.indent();
                dok.Save(writer);
                writer.Close();
                string str = sb.ToString();
                // todo: workaround
                str = str.Replace("xmlns", "\r\n     xmlns");
                schemas[locations[dok]] = str;
            }

            //===============
            if (config.isEliminateRedundantAttrDeclsEnabled() || (config.isEliminateRedundantAPsEnabled()))
                schemas = test(schemas);
            //===============

            return schemas;
        }

        public string import(XsdDocument parent, XsdDocument child)
        {
            string parentNamespace = parent.getTargetNamespace();
            string childNamespace = child.getTargetNamespace();

            if (parentNamespace == childNamespace)
                return "";         // no need to import namespace to the same namespace
                        
            string childNamespacePrefix = namingSupport.GetPrefixForNamespaceURI(childNamespace);
            string schemaLocation = locations[child];

            parent.import(childNamespace, childNamespacePrefix, schemaLocation);
            return childNamespacePrefix + ":";            
        }

		public override string Translate(PSMDiagram diagram)
		{
            Diagram = diagram;

            foreach (PSMClass root in diagram.Roots)
            {
                if (!root.CanBeDeclaredAsElement())
                {
                    if (!classesTranslationData.ContainsKey(root))
                        TranslateClass(root, new Context());
                    continue;
                }
                           
                // create <element> in appropriate context
                Context ctx = new Context(getAppropriateDocument(root));
                string elementName = namingSupport.GetNameForGlobalElement(root);
                ctx.element(elementName);
                ctx.down();

                // add reference to element declaration into the default document
                Context defCtx = new Context(getDefaultDocument());
                if (ctx.Document != defCtx.Document)
                {
                    string prefix = import(defCtx.Document, ctx.Document);
                    defCtx.elementRef(prefix + elementName);
                }

                // translate the class itself (recursive)
                string typeName = null;
                if (!classesTranslationData.ContainsKey(root))
                    typeName = TranslateClass(root, new Context(ctx));

                // add reference to complexType (if any) into "type" attribute
                if ((typeName != null) && (config.allComplexTypesGlobal()))
                {
                    ctx.typeAttribute(typeName);                      
                }        
            }

            // redundancy elimination
            eliminateRedundancy();
            return "ok";
		}
        
        /// Rozvetveni pro 1,2! Vrati nazev CT, pokud nìjaký vznikl.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="psmClass"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
		protected string TranslateClass(PSMClass psmClass, Context ctx)
		{
			string typeName;
			if (psmClass.HasElementLabel)
				typeName = TranslateClassWithLabel(psmClass, ctx);
			else
			{
				TranslateClassContentToGroups(psmClass, ctx);
				typeName = null;
			}

			// if there are specializations, translate them also
			TranslateSpecializations(psmClass, typeName);
			return typeName;
		}

        /// 1! Vrati nazev vznikleho CT.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="psmClass"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
		private string TranslateClassWithLabel(PSMClass psmClass, Context ctx)
		{
			// if this is used as a representedPSMClass from another class, content of the class is translated 
			// to model group and attribute group and these are then included in a complex type
			if (Diagram.DiagramElements.Keys.OfType<PSMClass>().Any(rClass => rClass.RepresentedPSMClass == psmClass))
				return TranslateClassWithLabelAsGroups(psmClass, ctx);

            // class with label is translated to a complex type
            if (config.allComplexTypesGlobal() || psmClass.IsAbstract)
            {
                // find out to which document complexType should be written
                ctx.Document = getAppropriateDocument(psmClass);
                ctx.Element = ctx.Document.getSchemaElement();
            }

			if (!classesTranslationData.ContainsKey(psmClass))
				classesTranslationData[psmClass] = new ClassTranslationData();
			ClassTranslationData translationData = classesTranslationData[psmClass];
			if (translationData.ComplexTypeName != null) //already translated
				return translationData.ComplexTypeName;

			if (translationData.NameBase == null)
				translationData.NameBase = namingSupport.GetNameForComplexType(psmClass);
			translationData.ComplexTypeName = translationData.NameBase;
            translationData.ParentDocument = ctx.Document;

			int curChoiceCounter = choiceCounter;
			choiceCounter = 0;

            // create a complexType node
            if (config.allComplexTypesGlobal() || psmClass.IsAbstract)
                ctx.complexType(translationData.ComplexTypeName);
            else
                ctx.complexType();    //local complex type cannot have "name" attribute

            ctx.down();

            if (psmClass.IsAbstract)
                ctx.setAbstract();

            // translate class content
            TranslateClassContentWithoutGroups(psmClass, new Context(ctx));

            if (psmClass.AllowAnyAttribute)
                ctx.anyAttribute();

            if (!ctx.Element.HasChildNodes)
                ctx.Element.IsEmpty = true;   // empty complex type

			choiceCounter = curChoiceCounter;
			return translationData.NameBase;
		}

        /// 2!
        /// <summary>
        /// 
        /// </summary>
        /// <param name="psmClass"></param>
        /// <param name="ctx"></param>
        private void TranslateClassContentToGroups(PSMClass psmClass, Context ctx)
        {
            ClassTranslationData reprTranslationData;
            bool isRepresentative = PrepareRepresentative(psmClass, out reprTranslationData);

            /* type name will be used as a name for model and attribute groups */
            string typeName;
            ClassTranslationData translationData;
            bool contentGroupAlreadyTranslated = false;
            bool attributeGroupAlreadyTranslated = false;
            bool needsOptGroup = inChoice;
            if (classesTranslationData.ContainsKey(psmClass))
            {
                /* the class was already translated (it was requested from a structural 
                 * representative), typeName is known */
                translationData = classesTranslationData[psmClass];
                typeName = translationData.NameBase;
                if (translationData.IsModelGroup)
                {
                    contentGroupAlreadyTranslated = true;
                }
                if ((needsOptGroup && translationData.IsAttributeGroupDeclaredOptional) ||
                    (!needsOptGroup && translationData.IsAttributeGroupDeclaredNormal))
                {
                    attributeGroupAlreadyTranslated = true;
                }
            }
            else
            {
                /* the class was not already translated, get the typeName */
                typeName = namingSupport.TranslateTypeName(psmClass);
                translationData = new ClassTranslationData();
                classesTranslationData[psmClass] = translationData;
            }

            // find a context where to place result
            ctx.Document = getAppropriateDocument(psmClass);
            ctx.Element = ctx.Document.getSchemaElement();

            translationData.AttributeGroupUnknown = true;
            translationData.ModelGroupUnknown = true;
            translationData.ParentDocument = ctx.Document;
            if (translationData.AttributeGroupNameSuggestion == null)
                translationData.AttributeGroupNameSuggestion = config.getAttrGroupNameMask().Replace("%", typeName);
            if (needsOptGroup)
            {
                translationData.AttributeGroupNameSuggestion += "-opt";
                if (inChoice) Log.AddWarning(LogMessages.XS_ATTRIBUTES_IN_CHOICE);
            }
            if (translationData.ModelGroupNameSuggestion == null)
                translationData.ModelGroupNameSuggestion = config.getGroupNameMask().Replace("%", typeName);
            
            List<ComposedAttrGroup> composedAttributes = new List<ComposedAttrGroup>();
            if (!contentGroupAlreadyTranslated || !attributeGroupAlreadyTranslated)
            {
                XmlElement elemGroup = null;
                Context context = new Context(ctx);
                context.ComposedContent.Clear();
                context.ReferencedAttributeGroups = null;
                context.silence = true;

                if (!contentGroupAlreadyTranslated)
                {
                    context.silence = false;
                    translationData.ModelGroupName = translationData.ModelGroupNameSuggestion;
                    context.group(translationData.ModelGroupName);
                    elemGroup = context.last();
                    context.down();                    
                }
                
                // translate components
                TranslateComponentsIncludingRepresentative(psmClass, context);
                composedAttributes = composedAttributes.Concat(context.ComposedAttributes).ToList();
                context.silence = false;

                if (!contentGroupAlreadyTranslated)
                {
                    // if ComposedContent list is passed, put a reference to the created
				    // model group in the ComposedContent list
                    if ((ctx.ComposedContent != null) && (elemGroup.HasChildNodes))
                    {
                        XmlElement elemGroupRef = defaultDoc.CreateElement("xs", "group", "http://www.w3.org/2001/XMLSchema");
                        elemGroupRef.SetAttribute("ref", translationData.ModelGroupName);

                        if (ctx.LeadingAssociation != null)
                        {
                            uint? lower = (uint?) ctx.LeadingAssociation.Lower;
                            uint? upper = (uint?) ctx.LeadingAssociation.Upper;

                            if (!lower.HasValue || lower.Value != 1)
                                elemGroupRef.SetAttribute("minOccures", lower.HasValue ? lower.Value.ToString() : "0");

                            if (upper != null && upper.Value != 1)
                            {
                                if (upper.Value == UnlimitedNatural.Infinity)
                                    elemGroupRef.SetAttribute("maxOccurs", "unbounded");
                                else
                                    elemGroupRef.SetAttribute("maxOccurs", upper.ToString());
                            }
                        }
                        elemGroupRef.IsEmpty = true;
                        ctx.ComposedContent.Add(elemGroupRef);
                    }
                }

                // remove <group> if empty
                if (elemGroup != null)
                {
                    removeElementIfEmpty(elemGroup);
                    translationData.ModelGroupName = null;
                }
            }

            #region translate attributes
            if (psmClass.Attributes.Count > 0
                || (isRepresentative && reprTranslationData.IsAttributeGroup)
                || (composedAttributes.Count > 0)
                || ctx.ReferencedAttributeGroups != null
                || translationData.MustCreateAttributeGroup
                || attributeGroupAlreadyTranslated)
            {
                #region write the attribute group

                if (!attributeGroupAlreadyTranslated)
                {
                    string attributeGroupName = translationData.AttributeGroupNameSuggestion ?? config.getAttrGroupNameMask().Replace("%", typeName);
                    translationData.AttributeGroupName = attributeGroupName;
                    if (needsOptGroup)
                        translationData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Optional;
                    else
                        translationData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Normal;

                    ctx.attributeGroup(attributeGroupName);
                    ctx.down();

                    Context context = new Context(ctx);
                    context.ComposedAttributes = composedAttributes;
                    context.ReferencedModelGroups = null;

                    //List<int> refAPECs = new List<int>();
                    TranslateAttributesIncludingRepresentative(psmClass, context/*, refAPECs*/);

                    if (config.isEliminateRedundantAttrDeclsEnabled() || (config.isEliminateRedundantAPsEnabled()))
                    {
                        // vyrobime a zaevidujeme atributovou cast pro cerstve vytvorenou attributeGroup
                        AttrPart newAP = new AttrPart(ctx.Element, ADList);
                        int ixAPEC = addAttrPartToAPECList(newAP, ARHeap);
                        if (ixAPEC != -1)
                        {
                            translationData.attrPartEquivalenceClass = ixAPEC;
                        }
                        ADList.Clear();
                    }
                }
                #endregion

                /* if ComposedAttributes writer is passed, put a reference to the created
				 * attribute group in the ComposedAttributes writer */
                if (ctx.ComposedAttributes != null)
                {
                    XmlElement el = ctx.Document.CreateElement("xs", "attributeGroup", "http://www.w3.org/2001/XMLSchema");
                    el.SetAttribute("ref", translationData.AttributeGroupName);
                    ctx.ComposedAttributes.Add(new ComposedAttrGroup(el,translationData.attrPartEquivalenceClass));
                }
            }
            #endregion

            translationData.AttributeGroupUnknown = false;
            translationData.ModelGroupUnknown = false;
        }

        // ctx.currentElement je <complexType> nebo <attributeGroup>
        private void TranslateAttributesIncludingRepresentative(PSMClass psmClass, Context ctx/*, List<int> refAPECs = null*/)
        {
            ClassTranslationData myReprTranslationData;
            bool isRepresentative = PrepareRepresentative(psmClass, out myReprTranslationData);

            #region write reference to represented class group
            // append attributes of the represented class as an attribute group
            if (isRepresentative && myReprTranslationData.IsAttributeGroup)
            {
                if (inChoice)
                {
                    if (!myReprTranslationData.IsAttributeGroupDeclaredOptional
                        && !(myReprTranslationData.AttributeGroupUnknown && myReprTranslationData.AttributeGroupUsage == ClassTranslationData.EAGsage.None))
                    {
                        TranslateAttributeGroupsAgain(psmClass.RepresentedPSMClass, myReprTranslationData.AttributeGroupName);
                        myReprTranslationData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Optional;
                    }
                    if (myReprTranslationData.IsAttributeGroupDeclaredOptional || myReprTranslationData.AttributeGroupUnknown)
                    {
                        ctx.attributeGroupRef(myReprTranslationData.GetOptionalAttributeGroupName);
                        if (config.isEliminateRedundantAttrDeclsEnabled() || (config.isEliminateRedundantAPsEnabled()))
                            APECList[myReprTranslationData.attrPartEquivalenceClass].addRefElement(ctx.last());
                        myReprTranslationData.MustCreateAttributeGroup = true;
                        myReprTranslationData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Optional;
                    }
                }
                else
                {
                    if (!myReprTranslationData.IsAttributeGroupDeclaredNormal
                        && !(myReprTranslationData.AttributeGroupUnknown && myReprTranslationData.AttributeGroupUsage == ClassTranslationData.EAGsage.None))
                    {
                        TranslateAttributeGroupsAgain(psmClass.RepresentedPSMClass, myReprTranslationData.AttributeGroupName);
                        myReprTranslationData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Normal;
                    }
                    if (myReprTranslationData.IsAttributeGroupDeclaredNormal || myReprTranslationData.AttributeGroupUnknown)
                    {
                        ctx.attributeGroupRef(myReprTranslationData.GetNormalAttributeGroupName);
                        if (config.isEliminateRedundantAttrDeclsEnabled() || (config.isEliminateRedundantAPsEnabled()))
                            APECList[myReprTranslationData.attrPartEquivalenceClass].addRefElement(ctx.last());
                        myReprTranslationData.MustCreateAttributeGroup = true;
                        myReprTranslationData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Normal;
                    }
                }

                if (config.isEliminateRedundantAttrDeclsEnabled() || (config.isEliminateRedundantAPsEnabled()))
                {
                    BinaryHeap<int> heap = APECList[myReprTranslationData.attrPartEquivalenceClass].ARHeap.Copy();
                    if ((ARHeap != null) && (heap != null))
                    {
                        while (heap.Count > 0)
                            ARHeap.Add(heap.RemovePeek());
                    }
                }
            }
            #endregion

            #region write references to referenced groups
            if (ctx.ReferencedAttributeGroups != null)
            {
                foreach (ClassTranslationData referencedGroup in ctx.ReferencedAttributeGroups.Where(g => g.IsAttributeGroup))
                {
                    ctx.attributeGroupRef(referencedGroup.AttributeGroupName);
                }
                ctx.ReferencedAttributeGroups = null;
            }
            #endregion

            #region append attributes of the class itself
            /* append attributes of the class itself */
            foreach (PSMAttribute psmAttribute in psmClass.PSMAttributes)
            {
                ctx.attribute(namingSupport.NormalizeTypeName(psmAttribute, a => a.AliasOrName), psmAttribute, inChoice);

                if (config.isEliminateRedundantAttrDeclsEnabled() || (config.isEliminateRedundantAPsEnabled()))
                {
                    // vlozime atr. deklaraci do seznamu trid ekviv. atr. deklaraci, a take do AR haldy aktualniho kontextu
                    int ix = addAttrDeclarationToAECList(ctx.last());
                    if (ix > -1)
                    {
                        ARHeap.Add(ix);
                        ADList.Add(ix);
                    }
                }
            }
            #endregion

            #region append composed attributes
            if (ctx.ComposedAttributes != null)
            {
                foreach (ComposedAttrGroup attr in ctx.ComposedAttributes)
                {                    
                    XmlNode nod = ctx.Document.ImportNode(attr.attrGroup, true);
                    if ((nod.NodeType == XmlNodeType.Element) && (nod.Name == "xs:attributeGroup"))
                    {
                        string value = nod.Attributes["ref"].Value;
                        XsdDocument parentDocument = (XsdDocument) attr.attrGroup.OwnerDocument;
                        string prefix = import(ctx.Document, parentDocument);
                        nod.Attributes["ref"].Value = prefix + value;
                    }
                    nod = ctx.Element.AppendChild(nod);

                    if (config.isEliminateRedundantAttrDeclsEnabled() || (config.isEliminateRedundantAPsEnabled()))
                        APECList[attr.ixAPEC].addRefElement((XmlElement)nod);
                }
            }
            #endregion
        }

        // ctx.currentElement je <complexType>
        private void TranslateClassContentWithoutGroups(PSMClass psmClass, Context ctx)
        {
            Context context = new Context(ctx);
            TranslateComponentsIncludingRepresentative(psmClass, context);
            context.changePosition(ctx);

            //List<int> refAPECs = new List<int>();
            TranslateAttributesIncludingRepresentative(psmClass, context/*, refAPECs*/);

            if (config.isEliminateRedundantAttrDeclsEnabled() || (config.isEliminateRedundantAPsEnabled()))
            {
                // vytvorime a zaevidujeme atrib. cast pro novy complexType
                AttrPart newAP = new AttrPart(ctx.Element, ADList);
                int ixAPEC = addAttrPartToAPECList(newAP, ARHeap);
                ARHeap.Clear();
                ADList.Clear();
            }
        }

        /// Opetovny preklad attribute group.
        /// <summary>
        /// Translates the attribute groups again. This method is called when both 
        /// "opt group" and normal attribute groups need to be included in the 
        /// schema. See documentation to <c>XCase</c> for explanation of opt groups. 
        /// </summary>
        /// <param name="psmClass">The PSM class translated.</param>
        /// <param name="attributeGroupName">Name to which the attribute group was
        /// already translated.</param>
        private void TranslateAttributeGroupsAgain(PSMClass psmClass, string attributeGroupName)
        {
            if (inChoice && !attributeGroupName.EndsWith("-opt"))
            {
                attributeGroupName += "-opt";
            }
            else if (!inChoice && attributeGroupName.EndsWith("-opt"))
            {
                attributeGroupName = attributeGroupName.Substring(0, attributeGroupName.LastIndexOf("-opt"));
            }

            Context ctx = new Context(getAppropriateDocument(psmClass));
            ctx.attributeGroup(attributeGroupName);
            ctx.down();

            TranslateAttributesIncludingRepresentative(psmClass, ctx);
            if (config.isEliminateRedundantAttrDeclsEnabled() || (config.isEliminateRedundantAPsEnabled()))
                ADList.Clear();
        }

        /// 9!
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentContainer"></param>
        /// <param name="ctx"></param>
        protected override void TranslateContentContainer(PSMContentContainer contentContainer, Context ctx)
        {
            if (String.IsNullOrEmpty(contentContainer.Name))
            {
                Log.AddError(string.Format(LogMessages.XS_ELEMENT_NAME_MISSING, contentContainer));
                return;
            }

            int curChoiceCounter = choiceCounter;
            choiceCounter = 0;
            
            string elementName = namingSupport.NormalizeTypeName(contentContainer, c => c.Name);

            if (config.allElementsGlobal())
            {
                ctx.elementRef(elementName);
                ctx.Element = ctx.Document.getSchemaElement();
                if (config.allComplexTypesGlobal())
                    ctx.element(elementName);
                else
                {
                    ctx.element(elementName);
                    ctx.down();
                }

                ctx.complexType("ContentContainer");   // todo
                ctx.down();
            }
            else // local element
            {
                ctx.element(elementName);
                ctx.down();
                if (config.allComplexTypesGlobal())
                {
                    ctx.Element = ctx.Document.getSchemaElement();
                    ctx.complexType("ContentContainer");   // todo
                    ctx.down();
                }
                else
                {
                    ctx.complexType("ContentContainer");   // todo
                    ctx.down();
                }
            }
            XmlElement compType = ctx.Element;
            
            ctx.sequence();
            ctx.down();

            TranslateComponents(contentContainer, new Context(ctx));

            // remove sequence if it is empty
            ctx.Element = removeElementIfEmpty(ctx.Element);

            if (config.isEliminateRedundantAttrDeclsEnabled() || (config.isEliminateRedundantAPsEnabled()))
            {
                // vytvorime a zaevidujeme atrib. cast pro novy complexType
                AttrPart newAP = new AttrPart(ctx.Element, null);
                addAttrPartToAPECList(newAP, ARHeap);
                ARHeap.Clear();
            }

            choiceCounter = curChoiceCounter;
        }

        /// Pomocna.
        /// <summary>
        /// ctx.currentElement je <sequence>, superord.Component je psmClass
        /// </summary>
        /// <param name="superordinateComponent"></param>
        /// <param name="ctx"></param>
        private void TranslateComponents(PSMSuperordinateComponent superordinateComponent, Context ctx)
        {
            XsdDocument doc = ctx.Document;
            XmlElement elem = ctx.Element;

            List<ComposedAttrGroup> composedAttributes = new List<ComposedAttrGroup>();
            List<XmlNode> composedContent = new List<XmlNode>();
            BinaryHeap<int> heap = ARHeap.Copy();

            foreach (PSMSubordinateComponent c in superordinateComponent.Components)
            {
                Context context = new Context(ctx);
                TranslateSubordinateComponent(c, context);
                if (context.ComposedAttributes != null)
                    composedAttributes = composedAttributes.Concat(context.ComposedAttributes).ToList();
                if (context.ComposedContent != null)
                    composedContent = composedContent.Concat(context.ComposedContent).ToList();

                while (ARHeap.Count > 0)
                {
                    heap.Add(ARHeap.RemovePeek());
                }
            }
            if (composedAttributes != null)
                ctx.ComposedAttributes = ctx.ComposedAttributes.Concat(composedAttributes).ToList();
            if (composedContent != null)
                ctx.ComposedContent = ctx.ComposedContent.Concat(composedContent).ToList();
            ARHeap = heap.Copy();
        }

        /// Pomocna.
        /// <summary>
        /// ctx.currentElement je <complexType> nebo <group>
        /// </summary>
        /// <param name="psmClass"></param>
        /// <param name="ctx"></param> 
        private void TranslateComponentsIncludingRepresentative(PSMClass psmClass, Context ctx)
        {
            ClassTranslationData myReprTranslationData;
            bool isRepresentative = PrepareRepresentative(psmClass, out myReprTranslationData);

            ctx.sequence();
            ctx.down();
            XmlElement seq = ctx.Element;

            if (ctx.ReferencedModelGroups != null)
            {
                foreach (ClassTranslationData referencedData in ctx.ReferencedModelGroups)
                {
                    ctx.groupRef(referencedData.ModelGroupName);
                }
                ctx.ReferencedModelGroups = null;
            }

            // reference represented class' model group 
            if (isRepresentative && myReprTranslationData.IsModelGroup)
                // currentElement se nemeni
                ctx.groupRef(myReprTranslationData.ModelGroupName);

            TranslateComponents(psmClass, ctx);

            // remove sequence if it is empty
            removeElementIfEmpty(seq);
        }

        /// 5,6! Pro tridy se specializacemi.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="associationChild"></param>
        /// <param name="psmClass"></param>
        /// <param name="ctx"></param>
        private void TranslateEdgeGoingToSpecializedNode(PSMAssociationChild associationChild, PSMClass psmClass, Context ctx)
        {
            string typeName;
            // if under association, choice is written, otherwise under
            // class union choice was already written in during class union translation
            if (associationChild.ParentAssociation != null)
            {
                ctx.choice();
                ctx.down();
                ctx.multiplicityAttributes(associationChild.ParentAssociation.Lower, associationChild.ParentAssociation.Upper);
            }

            // prepare new context
            Context context = new Context();
            context.changePosition(ctx);
            context.ComposedContent = psmClass.IsAbstract ? null : ctx.ComposedContent;
            context.ComposedAttributes = psmClass.IsAbstract ? null : ctx.ComposedAttributes;

            typeName = TranslateClass(psmClass, context);

            ctx.ComposedAttributes = context.ComposedAttributes;
            ctx.ComposedContent = context.ComposedContent;

            if (psmClass.HasElementLabel && psmClass.CanBeDeclaredAsElement())
            {
                XmlElement elem = ctx.Element;
                ctx.element(namingSupport.NormalizeTypeName(psmClass, c => c.ElementName));
                ctx.down();
                ctx.typeAttribute(typeName);
                ctx.Element = elem;
            }
            else
            {
                if (psmClass.HasElementLabel && !psmClass.IsAbstract)
                {
                    Log.AddWarning(String.Format(LogMessages.XS_NON_ABSTRACT_CLASS, psmClass));
                }
            }

            foreach (XCase.Model.Generalization specialization in psmClass.Specifications)
            {
                PSMClass specialized = (PSMClass)specialization.Specific;
                TranslateSubstitutions(specialized, new Context(ctx));
            }
        }

        /// 5,6! Pro tridy bez specializaci.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="associationChild"></param>
        /// <param name="psmClass"></param>
        /// <param name="ctx"></param>
        private void TranslateEdgeWithNoSpecializedNode(PSMAssociationChild associationChild, PSMClass psmClass, Context ctx)
        {
            if (associationChild.ParentAssociation != null)
            {
                ctx.LeadingAssociation = associationChild.ParentAssociation;
                if (ctx.LeadingAssociation.Lower == 0)
                    choiceCounter++;
            }

            // 5!
            if (psmClass.HasElementLabel)
            {
                Context ctxLocal = new Context(ctx);
                Context ctxGlobal = new Context(getAppropriateDocument(psmClass));

                string typeName = null;
                string prefix = null;
                string elementName = namingSupport.NormalizeTypeName(psmClass, c => c.ElementName);

                if (ctxLocal.Document != ctxGlobal.Document)
                    prefix = import(ctxLocal.Document, ctxGlobal.Document);

                if ((config.allElementsGlobal()) || (ctxLocal.Document != ctxGlobal.Document))
                {
                    ctxLocal.elementRef(prefix+elementName);
                    ctx.changePosition(ctxGlobal);
                }
                else
                {
                    ctx.changePosition(ctxLocal);
                }

                ctx.element(elementName);
                ctx.down();

                Context context = new Context();
                context.changePosition(ctx);
                context.ComposedContent = ctx.ComposedContent;
                context.ComposedAttributes = ctx.ComposedAttributes;

                // translate the class itself (recursive process)
                typeName = TranslateClass(psmClass, context);

                ctx.ComposedAttributes = context.ComposedAttributes;
                ctx.ComposedContent = context.ComposedContent;

                if ((typeName != null) && (config.allComplexTypesGlobal()))
                {
                    // add reference to complexType into "type" attribute
                    ctx.typeAttribute(typeName);
                }

                // todo: pres context fce
                if (associationChild.ParentAssociation != null)
                    ctx.multiplicityAttributes(associationChild.ParentAssociation.Lower, associationChild.ParentAssociation.Upper);
                else
                {
                    PSMClassUnion union = associationChild.ParentUnion;
                    if (union != null && union.ParentAssociation != null)
                    {
                        ctx.multiplicityAttributes(union.ParentAssociation.Lower, union.ParentAssociation.Upper);
                    }
                }
            }
            else // 6!
            {
                // translate the class itself (recursive process)
                TranslateClass(psmClass, ctx);

                if (associationChild.ParentAssociation != null && classesTranslationData[psmClass].IsAttributeGroup
                    && associationChild.ParentAssociation.Upper > 1)
                    Log.AddWarning(string.Format(LogMessages.XS_ASSOCIATION_MULTIPLICITY_LOST, associationChild.ParentAssociation, associationChild.ParentAssociation.MultiplicityString, psmClass));
            }

            if (associationChild.ParentAssociation != null && associationChild.ParentAssociation.Lower == 0)
                choiceCounter--;

            if (psmClass.IsAbstract)
                Log.AddError(string.Format(LogMessages.XS_ABSTRACT_NOT_SPECIALIZED, psmClass));
        }

        /// Rozvetveni prekladu hran k tridam bez specializaci a se specializacemi.
        /// <summary>
        /// ctx.currentElement je <complexType>
        /// </summary>
        /// <typeparam name="complexType"></typeparam>
        /// <param name="?"></param>
        /// <returns></returns>
        private void TranslateAssociationChildAsPSMClass(PSMAssociationChild associationChild, PSMClass psmClass, Context ctx)
        {
            if (psmClass.Specifications.Count > 0)
            {
                TranslateEdgeGoingToSpecializedNode(associationChild, psmClass, ctx);
            }
            else
            {
                TranslateEdgeWithNoSpecializedNode(associationChild, psmClass, ctx);

            }
        }

        /// 7,8!
        /// <summary>
        /// 
        /// </summary>
        /// <param name="associationChild"></param>
        /// <param name="classUnion"></param>
        /// <param name="ctx"></param>
        private void TranslateAssociationChildAsPSMClassUnion(PSMAssociationChild associationChild, PSMClassUnion classUnion, Context ctx)
        {
            choiceCounter++;

            ctx.choice();
            ctx.down();
            XmlElement elemChoice = ctx.Element;

            if (associationChild.ParentAssociation != null)
                ctx.multiplicityAttributes(associationChild.ParentAssociation.Lower, associationChild.ParentAssociation.Upper);

            List<ComposedAttrGroup> composedAttributes = new List<ComposedAttrGroup>();
            List<XmlNode> composedContent = new List<XmlNode>();
            foreach (PSMAssociationChild psmAssociationChild in classUnion.Components)
            {
                Context context = new Context(ctx);
                TranslateAssociationChild(psmAssociationChild, context);
                composedAttributes = composedAttributes.Concat(context.ComposedAttributes).ToList();
                composedContent = composedContent.Concat(context.ComposedContent).ToList();
            }

            if (composedContent != null && composedContent.Count>0)
            {
                foreach (XmlNode node in composedContent)
                {
                    XmlNode nod = ctx.Document.ImportNode(node, true);
                    ctx.Element.AppendChild(nod);
                }
            }

            if (composedAttributes.Count>0 && ctx.ComposedAttributes != null)
            {
                ctx.ComposedAttributes = ctx.ComposedAttributes.Concat(composedAttributes).ToList();
                Log.AddWarning(string.Format(LogMessages.XS_ATTRIBUTES_IN_CLASS_UNION, classUnion));
            }

            removeElementIfEmpty(elemChoice);
            choiceCounter--;
        }

        /// Rozvetveni prekladu hran k psmClass (5,6) a k psmClassUnionu (7,8).
        /// <summary>
        /// ctx.currentElement je <complexType>
        /// </summary>
        /// <param name="associationChild"></param>
        /// <param name="ctx"></param>
        protected override void TranslateAssociationChild(PSMAssociationChild associationChild, Context ctx)
        {
            PSMClass psmClass = associationChild as PSMClass;
            if (psmClass != null)
            {
                TranslateAssociationChildAsPSMClass(associationChild, psmClass, ctx);
            }
            PSMClassUnion classUnion = associationChild as PSMClassUnion;
            if (classUnion != null)
            {
                TranslateAssociationChildAsPSMClassUnion(associationChild, classUnion, ctx);
            }
        }

        /// 10!
        /// <summary>
        /// Translates the attribute container. The attributes in the <paramref name="attributeContainer"/> 
        /// are translated into element declarations.
        /// </summary>
        /// <param name="attributeContainer">The attribute container.</param>
        /// <param name="translationContext">The translation context.</param>
        protected override void TranslateAttributeContainer(PSMAttributeContainer attributeContainer, Context ctx)
        {
            foreach (PSMAttribute psmAttribute in attributeContainer.PSMAttributes)
            {
                ctx.attributeAsElement(namingSupport.NormalizeTypeName(psmAttribute, a => a.AliasOrName), psmAttribute);
            }
        }

        /// 8!
        /// <summary>
        /// ctx.currentElement je <sequence> nebo <choice>
        /// </summary>
        /// <param name="contentChoice"></param>
        /// <param name="ctx"></param> 
        protected override void TranslateContentChoice(PSMContentChoice contentChoice, Context ctx)
        {
            choiceCounter++;

            // create a <choice> node in current context
            ctx.choice();
            ctx.down();

            foreach (PSMSubordinateComponent component in contentChoice.Components)
            {
                Context context = new Context(ctx);
                context.ReferencedAttributeGroups = null;
                context.ReferencedModelGroups = null;
                context.LeadingAssociation = null;

                TranslateSubordinateComponent(component, context);
                ctx.ComposedAttributes = context.ComposedAttributes;
                //if (context.ComposedAttributes != null) ctx.ComposedAttributes = ctx.ComposedAttributes.Concat(context.ComposedAttributes).ToList();
                //if (context.ComposedContent != null) ctx.ComposedContent = ctx.ComposedContent.Concat(context.ComposedContent).ToList();
                //if (context.ReferencedAttributeGroups != null) ctx.ReferencedAttributeGroups = ctx.ReferencedAttributeGroups.Concat(context.ReferencedAttributeGroups).ToArray();
                //if (context.ReferencedModelGroups != null) ctx.ReferencedModelGroups = ctx.ReferencedModelGroups.Concat(context.ReferencedModelGroups).ToArray();
            }
            choiceCounter--;
        }

        /// Trida jako CT i Groups, at se na ni muze odkazat strukt.repr. (12,13).
        /// <summary>
        /// Attributes and content of the class is translated into model and attribute group. 
        /// These groups are then referenced in the complexType element. This approach is used 
        /// for those types that are referenced from structural representatives. 
        /// </summary>
        /// <param name="psmClass">translated class</param>
        /// <param name="translationContext">translation context, complex type is created
        /// int <paramref name="translationContext"/>'s <see cref="TranslationContext.TreeDeclarations"/>writer</param>
        /// <returns>name of the created complex type</returns>
        /// ctx.currentElement je <schema> element
        private string TranslateClassWithLabelAsGroups(PSMClass psmClass, Context ctx)
        {
            if (!classesTranslationData.ContainsKey(psmClass))
                classesTranslationData[psmClass] = new ClassTranslationData();

            ClassTranslationData myTranslationData = classesTranslationData[psmClass];
            if (myTranslationData.ComplexTypeName != null) //already translated
                return myTranslationData.ComplexTypeName;

            if (myTranslationData.NameBase == null)
                myTranslationData.NameBase = namingSupport.GetNameForComplexType(psmClass);
            myTranslationData.ComplexTypeName = myTranslationData.NameBase;
            int curChoiceCounter = choiceCounter;
            choiceCounter = 0;

            // translate class to model and attribute group 
            TranslateClassContentToGroups(psmClass, new Context(ctx));

            // create complex type with references to the groups
            ctx.Element = ctx.Document.complexType(ctx.Element, myTranslationData.ComplexTypeName);
            if (psmClass.IsAbstract)
                ctx.Document.setAbstract(ctx.Element);

            if (myTranslationData.IsModelGroup)
            {
                ctx.Element = ctx.Document.sequence(ctx.Element);
                ctx.Document.groupRef(ctx.Element, myTranslationData.ModelGroupName);
            }
            if (myTranslationData.IsAttributeGroup)
            {
                ctx.Document.attributeGroupRef(ctx.Element, myTranslationData.AttributeGroupName);
            }
            if (psmClass.AllowAnyAttribute)
                ctx.Document.anyAttribute(ctx.Element);

            choiceCounter = curChoiceCounter;
            return myTranslationData.NameBase;
        }

        /// Preklad tridy pri prekladu reprezentantu.
        /// <summary>
        /// Looks up <see cref="ClassTranslationData"/> for <paramref name="psmClass"/>' structural representative.
        /// If the representative was not yet translated, it is translated immediately. If class is
        /// not structural representative, <c>false</c> is return.
        /// </summary>
        /// <param name="psmClass">The PSM class for which structural representative is searched</param>
        /// <param name="representativeTranslationData">The structural representative's translation data.</param>
        /// <returns><code>true</code> if class is structural representative, <code>false</code> otherwise</returns>
        private bool PrepareRepresentative(PSMClass psmClass, out ClassTranslationData representativeTranslationData)
        {
            representativeTranslationData = null;
            bool isRepresentative = psmClass.IsStructuralRepresentative;

            /* find (or translate, if it was not already translated) represented class */
            if (isRepresentative)
                representativeTranslationData = TranslateClassNow(psmClass.RepresentedPSMClass);
            return isRepresentative;
        }

        /// Preklad tridy pri prekladu reprezentantu.
        /// <summary>
        /// Translates the class even when it is in another subtree. This call is
        /// used when a referenced class must be translated before the translation 
        /// can continue. 
        /// </summary>
        /// <param name="translatedClass">The translated class.</param>
        /// <returns>result of the translation of <paramref name="translatedClass"/></returns>
        private ClassTranslationData TranslateClassNow(PSMClass translatedClass)
        {
            if (classesTranslationData.ContainsKey(translatedClass))
                return classesTranslationData[translatedClass];          // already translated

            string reprTypeName = namingSupport.GetNameForComplexType(translatedClass);
            ClassTranslationData myReprTranslationData = new ClassTranslationData { NameBase = reprTypeName };
            classesTranslationData.Add(translatedClass, myReprTranslationData);

            TranslateClass(translatedClass, new Context());

            return myReprTranslationData;
        }

        /// 17! GroupRef a Elementy pro specializace.
        /// <summary>
        /// </summary>
        /// <param name="specialization"></param>
        /// <param name="ctx"></param>
        private void TranslateSubstitutions(PSMClass specialization, Context ctx)
        {
            if (!classesTranslationData.ContainsKey(specialization)) // this should never happen, but to be sure
                return;

            ClassTranslationData specializedData = classesTranslationData[specialization];

            if (specializedData.IsModelGroup)
            {
                ctx.Document.groupRef(ctx.Element, specializedData.ModelGroupName);
            }
            if (specializedData.IsAttributeGroup)
            {
                Log.AddWarning(String.Format(LogMessages.XS_SPECIALIZED_ATTRIBUTE_GROUP, specialization, specializedData.AttributeGroupName));
            }
            if (specialization.HasElementLabel)
            {
                if (!specialization.IsAbstract)
                {
                    Context ctxLocal = new Context(ctx);
                    Context ctxGlobal = new Context(getAppropriateDocument(specialization));

                    string prefix = null;
                    string elementName = namingSupport.NormalizeTypeName(specialization, c => c.ElementName);

                    if (ctxLocal.Document != ctxGlobal.Document)
                        prefix = import(ctxLocal.Document, ctxGlobal.Document);

                    if ((config.allElementsGlobal()) || (ctxLocal.Document != ctxGlobal.Document))
                    {
                        ctxLocal.elementRef(prefix + elementName);
                        ctx.changePosition(ctxGlobal);
                    }
                    else
                    {
                        ctx.changePosition(ctxLocal);
                    }

                    ctx.element(elementName);
                    ctx.down();
                    ctx.typeAttribute(specializedData.ComplexTypeName);
                }
            }
            foreach (XCase.Model.Generalization specification in specialization.Specifications)
            {
                TranslateSubstitutions((PSMClass)specification.Specific, new Context(ctx));
            }
        }

        /// Rozvetveni pro 14,15,16!
        /// <summary> 
        /// Translates the <paramref name="specialization"/>. One of the methods 
		/// <see cref="TranslateSpecializationGeneralIsComplexType"/>,
		/// <see cref="TranslateSpecializationGeneralNotComplexTypeSpecificWithLabel"/> or
		/// <see cref="TranslateSpecializationGeneralNotComplexTypeSpecificWithoutLabel"/>
		/// is chosen according to whether the general class was translated into 
		/// a complex type and whether the specific class has or has not 
		/// the element label 
		/// </summary>
		/// <param name="specialization">The translated specialization.</param>
		/// <param name="generalTypeName">Identifier of the general type.</param>
		protected override void TranslateSpecialization(XCase.Model.Generalization specialization, string generalTypeName)
		{
			PSMClass general = (PSMClass)specialization.General;
			PSMClass specific = (PSMClass)specialization.Specific;

            Context ctx = new Context(getAppropriateDocument(specific));
			ClassTranslationData generalData = classesTranslationData[general];
			if (general.HasElementLabel || generalData.IsComplexType)
			{
                TranslateSpecializationGeneralIsComplexType(specific, general, generalTypeName, ctx);
			}
			else
			{
				if (specific.HasElementLabel)
                    TranslateSpecializationGeneralNotComplexTypeSpecificWithLabel(specific, general, ctx);
				else
                    TranslateSpecializationGeneralNotComplexTypeSpecificWithoutLabel(specific, general, ctx);
			}
		}

        /// 14!
        /// <summary>
		/// Translates specific class into a complex type that is 
		/// an extension of the type to which <paramref name="general"/>
		/// was translated to. 
		/// </summary>
		/// <param name="specific">The specific class.</param>
		/// <param name="general">The general class.</param>
		/// <param name="generalTypeName">Name of the complex type to which general class was translated to.</param>
		/// <param name="translationContext">The translation context.</param>
		private void TranslateSpecializationGeneralIsComplexType(PSMClass specific, PSMClass general, string generalTypeName, Context ctx)
		{
			string specificTypeName = namingSupport.GetNameForComplexType(specific);

			ClassTranslationData myRepresentativeTranslationData = new ClassTranslationData { NameBase = specificTypeName };
			classesTranslationData[specific] = myRepresentativeTranslationData;
			myRepresentativeTranslationData.ComplexTypeName = specificTypeName;

			int curChoiceCounter = choiceCounter;
			choiceCounter = 0;
            
            ctx.complexType(specificTypeName);
            ctx.down();
            if (specific.IsAbstract)
                ctx.setAbstract();

            ctx.complexContent();
            ctx.down();
            ctx.extension(generalTypeName);
            ctx.down();

			TranslateClassContentWithoutGroups(specific, new Context(ctx));

            if (specific.AllowAnyAttribute)
                ctx.anyAttribute();

            if (!ctx.Element.HasChildNodes)
                ctx.Element.IsEmpty = true;   // empty extension construct

            // change context document/current element
            XsdDocument doc = getAppropriateDocument(specific);
            ctx.changePosition(doc, doc.getSchemaElement());

			PSMClass root;
			if (specific.HasElementLabel 
				&& specific.IsClassSpecializedRoot(Diagram, false, out root)
				&& (!specific.IsAbstract || specific.NonAbstractWithoutLabelRecursive())
				&& specific.ElementName != root.ElementName)
			{
				string elementName = namingSupport.GetNameForGlobalElement(specific);
                string typeName = namingSupport.NormalizeTypeName(specific, c => c.Name);
                ctx.element(elementName);
                ctx.down();
                ctx.typeAttribute(typeName);

                // add reference to element declaration into the default document
                Context defCtx = new Context(getDefaultDocument());
                if (ctx.Document != defCtx.Document)
                {
                    string prefix = import(defCtx.Document, ctx.Document);
                    defCtx.elementRef(prefix + elementName);
                }
			}

			// if there are specifications, translate them also
			TranslateSpecializations(specific, specificTypeName);

			choiceCounter = curChoiceCounter;
		}

        /// 15!
        /// <summary>
		/// Translates the specific class into a complex type referencing 
		/// model and attribute group to which the general class was translated to. 
		/// </summary>
		/// <param name="specific">The specific class.</param>
		/// <param name="general">The general class.</param>
		/// <param name="translationContext">The translation context.</param>
		private void TranslateSpecializationGeneralNotComplexTypeSpecificWithLabel(PSMClass specific, PSMClass general, Context ctx)
		{
			ClassTranslationData generalData = classesTranslationData[general];
			string specificTypeName = namingSupport.GetNameForComplexType(specific);

			ClassTranslationData myRepresentativeTranslationData = new ClassTranslationData { NameBase = specificTypeName };
			classesTranslationData[specific] = myRepresentativeTranslationData;
			myRepresentativeTranslationData.ComplexTypeName = specificTypeName;

			int curChoiceCounter = choiceCounter;
			choiceCounter = 0;

            ctx.Element = ctx.Document.complexType(ctx.Element, specificTypeName);
            if (specific.IsAbstract)
                ctx.Document.setAbstract(ctx.Element);

            Context context = new Context(ctx);
            if (generalData.IsModelGroup)
                context.ReferencedModelGroups = new [] { generalData };
            if (generalData.IsAttributeGroup)
                context.ReferencedAttributeGroups = new[] { generalData };

			TranslateComponentsIncludingRepresentative(specific, context);
            context.changePosition(ctx);
			TranslateAttributesIncludingRepresentative(specific, context);

            if (config.isEliminateRedundantAttrDeclsEnabled() || (config.isEliminateRedundantAPsEnabled()))
                ADList.Clear();

			if (specific.AllowAnyAttribute)
                ctx.Document.anyAttribute(ctx.Element);

			PSMClass root;
			if (specific.HasElementLabel
				&& specific.IsClassSpecializedRoot(Diagram, false, out root)
				&& (!specific.IsAbstract || specific.NonAbstractWithoutLabelRecursive())
				&& specific.ElementName != root.ElementName)
			{
				string elementName = namingSupport.GetNameForGlobalElement(specific);
                string typeName = namingSupport.NormalizeTypeName(specific, c => c.Name);

                XmlElement elem = defaultDoc.element(defaultDoc.getSchemaElement(), elementName);
                defaultDoc.typeAttribute(elem, typeName);
			}

			// if there are specifications, translate them also 
			TranslateSpecializations(specific, specificTypeName);

			choiceCounter = curChoiceCounter;
		}

		/// 16!
        /// <summary>
		/// Translates the specific class into model and attribute groups that 
		/// reference the groups to which the general class was translated to. 
		/// </summary>
		/// <param name="specific">The specific class.</param>
		/// <param name="general">The general class.</param>
		/// <param name="translationContext">The translation context.</param>
		private void TranslateSpecializationGeneralNotComplexTypeSpecificWithoutLabel(PSMClass specific, PSMClass general, Context ctx)
		{
			string specificTypeName = namingSupport.TranslateTypeName(specific);
			
			ClassTranslationData generalData = classesTranslationData[general];
			ClassTranslationData myTranslationData = new ClassTranslationData { NameBase = specificTypeName };
			classesTranslationData[specific] = myTranslationData;

			if (generalData.IsModelGroup)
				ctx.ReferencedModelGroups = new[] { generalData };
			if (generalData.IsAttributeGroup)
				ctx.ReferencedAttributeGroups = new[] { generalData };

			TranslateClassContentToGroups(specific, ctx);
			
			/* if there are specifications, translate them also */
			TranslateSpecializations(specific, specificTypeName);
		}

        private XmlElement removeElementIfEmpty(XmlElement elem)
        {
            if (!elem.HasChildNodes)
            {
                XmlNode node = (XmlNode)elem;
                elem = (XmlElement)node.ParentNode;
                if (elem != null)
                    elem.RemoveChild(node);
            }
            return elem;
        }

        // ==========================================================================================
        #region REDUNDANCY ELIMINATION

        private void eliminateRedundancy()
        {
            if (config.isEliminateRedundantAttrDeclsEnabled())
                eliminateRedundantAttrDeclarations();
            if (config.isEliminateRedundantAPsEnabled())
                eliminateRedundantAPs();
            //if (config.isEliminateRedundancyInNestingsEnabled())
                //eliminateRedundantNestings();
        }

        // pusti algoritmus odstraneni redundantnich APs
        private void eliminateRedundantAPs()
        {
            foreach (AttrPartEquivClass apec in APECList)
            {
                if (apec.APList.Count > 1)
                    processAPEC(apec);                
            }            
        }

        // odstrani redundance v ramci jedne tridy ekvivalence APs
        private void processAPEC(AttrPartEquivClass apec)
        {
            //if (apec.APRefList.Contains(apec.repr))
            bool contains = false;
            foreach (XmlElement elem in apec.refElementList)
            {
                if (elem.ParentNode == apec.repr.getParent())
                    contains = true;
            }

            //if (apec.refElementList.Contains(apec.repr.getParent()))
            if (contains)
            {
                //takovy repr neni vhodny, najdeme noveho
                apec.chooseRepresentative();
            }

            XmlElement reprParent = apec.repr.getParent();
            XmlDocument reprDoc = reprParent.OwnerDocument;
            //reprParent.SetAttribute("ORIGREPR", "true"); // todo            

            // pokud to neni attribute group
            if (reprParent.LocalName != "attributeGroup")
            {
                // musime si ji vytvorit
                XmlElement elemAttrGroup = reprDoc.CreateElement("xs", "attributeGroup", "http://www.w3.org/2001/XMLSchema");
                elemAttrGroup.SetAttribute("id", namingSupport.GetUniqueIDForAttributeGroup());

                // dat ji stejny obsah, jako ma puvodni repr
                foreach (XmlNode child in reprParent.ChildNodes)
                {
                    elemAttrGroup.AppendChild(child.Clone());
                }

                // dale zavesit novou attr. group do dokumentu
                reprDoc.DocumentElement.AppendChild(elemAttrGroup);

                // a presmerovat na ni repr ukazatele
                apec.repr = new AttrPart(elemAttrGroup, apec.repr.ADList);
                reprParent = elemAttrGroup;
            }

            // ted mame zaruceno, ze repr ma za parenta attribute group, vyrobime si odkaz na tuto attr. group
            XmlElement elemAttrGroupRef = reprDoc.CreateElement("xs", "attributeGroup", "http://www.w3.org/2001/XMLSchema");
            elemAttrGroupRef.SetAttribute("ref", reprParent.GetAttribute("id"));
            elemAttrGroupRef.IsEmpty = true;

            foreach (XmlElement refElem in apec.refElementList)
            {
                if (refElem.ParentNode != null)
                {
                    refElem.ParentNode.ReplaceChild(elemAttrGroupRef.Clone(), refElem);
                }
            }

            foreach (AttrPart ap in apec.APList)
            {
                XmlElement parent = ap.getParent();
                if (parent.LocalName == "attributeGroup")
                {
                    if (apec.repr != ap)
                        parent.ParentNode.RemoveChild(parent);
                    //parent.SetAttribute("removed", "true"); // todo
                }
                else
                {
                    //parent.SetAttribute("CONTENT_REMOVED", "true"); // todo
                    while (parent.HasChildNodes)
                    {
                        parent.RemoveChild(parent.FirstChild);
                    }
                    parent.AppendChild(elemAttrGroupRef.Clone());
                }
            }
        }

        string s2 = null;
        private void eliminateRedundantAttrDeclarations()
        {
            #region collect intersections
            BinaryHeap<APIntersection> S = new BinaryHeap<APIntersection>();
            int i = 0;
            foreach (AttrPartEquivClass apec in APECList) {
                i++;
                int m = 0;
                foreach (AttrPart ap in apec.APList) {
                    m++;
                    int j = 0;
                    foreach (AttrPartEquivClass apec2 in APECList) {
                        j++;
                        int n = 0;
                        foreach (AttrPart ap2 in apec2.APList) {
                            n++;
                            if (j < i) continue;
                            if ((j == i) && (n <= m)) continue;
                            APIntersection qqq = new APIntersection(ap, ap2);
                            if (qqq.intersection.Count>0)
                                S.Add(qqq);
                        }
                    }
                }
            }
            #endregion

            #region testovaci vypis
            string ret = null;
            BinaryHeap<APIntersection> aaa = S.Copy();
            while (aaa.Count > 0)
                ret += "\n" + aaa.RemovePeek().info();
            s2 = ret + "\n";
            #endregion

            // jdeme resit ulohu
            while (S.Count > 0)
            {
                APIntersection inter = S.RemovePeek();
                s2 += "\n\nprocessing " + inter.info();

                // todo: special dokument pro vyclenovani!!!
                XmlDocument specialDoc = inter.refAP1.getParent().OwnerDocument;   

                XmlElement elemAttrGroup = specialDoc.CreateElement("xs", "attributeGroup", "http://www.w3.org/2001/XMLSchema");
                elemAttrGroup.SetAttribute("id", namingSupport.GetUniqueIDForAttributeGroup());

                // dame nove attribute groupe obsah z pruniku
                ARHeap.Clear();
                ADList.Clear();
                foreach (int ix in inter.intersection)
                {
                    XmlElement attr = (XmlElement) AECList[ix].getAttributes()[0].Clone();
                    elemAttrGroup.AppendChild(attr);
                    ARHeap.Add(ix);
                    ADList.Add(ix);
                }

                // dale zavesit novou attr. group do dokumentu
                specialDoc.DocumentElement.AppendChild(elemAttrGroup);

                // a vytvorit novou AttrPart
                AttrPart ap = new AttrPart(elemAttrGroup, ADList);
                addAttrPartToAPECList(ap, ARHeap);

                // vyrobime si odkaz na tuto attr. group
                XmlElement elemAttrGroupRef = specialDoc.CreateElement("xs", "attributeGroup", "http://www.w3.org/2001/XMLSchema");
                elemAttrGroupRef.SetAttribute("ref", elemAttrGroup.GetAttribute("id"));
                elemAttrGroupRef.IsEmpty = true;

                // upravime AD listy a DOM strukturu participantu pruseku
                removeAttrDeclsFromAP(inter.refAP1, inter.intersection);
                removeAttrDeclsFromAP(inter.refAP2, inter.intersection);
                
                // a pridame do participantu referenci na novou attr. group
                inter.refAP1.getParent().AppendChild(elemAttrGroupRef.Clone());
                inter.refAP2.getParent().AppendChild(elemAttrGroupRef.Clone());

                // todo: zaevidovat nove zavislosti mezi APs

                S = repair(S, inter, ap);

                foreach (AttrPartEquivClass apec in APECList)
                {
                    foreach (AttrPart ap2 in apec.APList)
                    {
                        if (ap == ap2)
                            continue;
                        APIntersection newInter = new APIntersection(ap, ap2);
                        if (newInter.intersection.Count > 0)
                            S.Add(newInter);
                    }
                }
            }
        }

        public BinaryHeap<APIntersection> repair(BinaryHeap<APIntersection> pS, APIntersection pInter, AttrPart pAP)
        {
            BinaryHeap<APIntersection> S = new BinaryHeap<APIntersection>();
            while (pS.Count > 0)
            {
                APIntersection inter = pS.RemovePeek();
                s2 += "\n    "+inter.info()+" is";
                if (!inter.shareParticipant(pInter))
                {
                    s2 += " irrelevant";
                    continue;
                }

                IntersectionColor color = inter.sayColor(pInter);
                switch (color)
                {
                    case IntersectionColor.Blue:
                        s2 += " blue";
                        continue;

                    case IntersectionColor.Red:
                        s2 += " red";
                        continue;

                    case IntersectionColor.Yellow:
                        inter.intersection = inter.intersection.Except<int>(pInter.intersection).ToList<int>();
                        S.Add(inter);
                        s2 += " yellow";
                        continue;

                    case IntersectionColor.Violet:
                        S.Add(inter);
                        s2 += " violet";
                        continue;
                }
            }
            return S;
        }
    
        public void removeAttrDeclsFromAP(AttrPart pAP, List<int> pAECList)
        {
            // upravim AD list
            pAP.ADList = pAP.ADList.Except<int>(pAECList).ToList<int>();

            // upravim DOM strukturu
            foreach (int ix in pAECList)
            {
                string attrName = AECList[ix].getAttributes()[0].GetAttribute("name");

                // Create an XmlNamespaceManager to resolve the default namespace.
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(pAP.getParent().OwnerDocument.NameTable);
                nsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

                XmlNode elem = pAP.getParent().SelectSingleNode("descendant::xs:attribute[@name='" + attrName + "']", nsmgr);
                if (elem != null)
                    pAP.getParent().RemoveChild(elem);
            }
        }
    

        public class APIntersection : IComparable<APIntersection>
        {
            public AttrPart refAP1;
            public AttrPart refAP2;
            public List<int> intersection;

            public APIntersection(AttrPart ap1, AttrPart ap2)
            {
                refAP1 = ap1;
                refAP2 = ap2;
                intersection = ap1.ADList.Intersect<int>(ap2.ADList).ToList<int>();
            }

            public int CompareTo(APIntersection inter)
            {
                return inter.intersection.Count - intersection.Count;
                // todo
            }

            public bool shareParticipant(APIntersection inter)
            {
                if ((inter.refAP1 == refAP1) || (inter.refAP1 == refAP2))
                    return true;
                if ((inter.refAP2 == refAP1) || (inter.refAP2 == refAP2))
                    return true;
                return false;
            }

            public IntersectionColor sayColor(APIntersection inter)
            {
                List<int> ListA = intersection;
                List<int> X = inter.intersection;
                List<int> myInter = ListA.Intersect<int>(X).ToList<int>();

                if (myInter.IsEmpty())
                    return IntersectionColor.Violet;

                if (myInter.Count == X.Count)
                    return IntersectionColor.Red;

                if (myInter.Count < ListA.Count && myInter.Count < X.Count)
                {
                    return IntersectionColor.Yellow;
                }
                return IntersectionColor.Blue;
            }

            public string info()
            {
                string ret = null;
                ret += "(" + refAP1.getParent().GetAttribute("name") + "," + refAP2.getParent().GetAttribute("name") + ")";
                foreach (int m in this.intersection)
                {
                    ret += " " + m;
                }
                return ret;
            }
        }

        public enum IntersectionColor { Blue, Red, Violet, Yellow };


        // vytvori hash z haldy trid ekvivalence atributovych deklaraci
        private string AECIndexesHashCode(BinaryHeap<int> heap)
        {
            if (heap.Count < 1)
                return null;

            string repr = null;
            while (heap.Count > 0)
            {
                repr += heap.RemovePeek().ToString() + " ";
            }

            string hash = Convert.ToBase64String(
                new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(
                    System.Text.Encoding.Default.GetBytes(repr))
            );
            return hash;
        }

        private string attrDeclarationHashCode(XmlElement attr)
        {
            string repr = attr.OwnerDocument.DocumentElement.GetAttribute("targetNamespace") + ":" + attr.GetAttribute("name") + " " + attr.GetAttribute("type") + " "
                + attr.GetAttribute("use") + " " + attr.GetAttribute("default");
            string hash = Convert.ToBase64String(
                new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(
                    System.Text.Encoding.Default.GetBytes(repr))
            );
            return hash;
        }

        private int addAttrPartToAPECList(AttrPart attrPart, BinaryHeap<int> ARHeap)
        {
            BinaryHeap<int> heapCopy = ARHeap.Copy();
            string hash = AECIndexesHashCode(heapCopy);
            if (hash == null)
                return -1;

            if (!APECHashDict.ContainsKey(hash))
            {
                // vytvorime novou equiv. class
                AttrPartEquivClass apec = new AttrPartEquivClass(attrPart, ARHeap);
                APECList.Add(apec);
                APECHashDict[hash] = new List<int>();
                APECHashDict[hash].Add(APECList.Count-1);
                return APECList.Count - 1;
            }

            foreach (int ix in APECHashDict[hash])
            {
                if (APECList[ix].checkCandidate(ARHeap))
                {
                    APECList[ix].add(attrPart);
                    return ix;
                }
            }
            AttrPartEquivClass apec2 = new AttrPartEquivClass(attrPart, ARHeap);
            APECList.Add(apec2);
            APECHashDict[hash].Add(APECList.Count - 1);
            return APECList.Count - 1;
        }

        private int addAttrDeclarationToAECList(XmlElement attrDeclaration)
        {
            // zaevidujeme atribut do vhodne tridy ekvivalence
            string hash = attrDeclarationHashCode(attrDeclaration);               // spocitame hash pro danou atr. deklaraci
            if (hash == null)
                return -1;

            if (!AECHashDict.ContainsKey(hash))                                   // a mrkneme se, jestli uz tento hash nekdy padl
            {
                AttrEquivClass aec = new AttrEquivClass(attrDeclaration);         // pokud ne, vytvorime novou AEC
                AECList.Add(aec);                                                 // pridame ji do seznamu AEC
                AECHashDict[hash] = new List<int>();
                AECHashDict[hash].Add(AECList.Count-1);                           // a do hashovaciho seznamu dame index na danou polozku v AECListu
                return AECList.Count - 1;
            }

            foreach (int ix in AECHashDict[hash])
            {
                if (AECList[ix].checkCandidate(attrDeclaration))
                {
                    AECList[ix].add(attrDeclaration);
                    return ix;
                }
            }
            AttrEquivClass aec2 = new AttrEquivClass(attrDeclaration);     // vytvorime novou AEC
            AECList.Add(aec2);                                             // pridame ji do seznamu AEC
            AECHashDict[hash].Add(AECList.Count - 1);                     // a do hashovaciho seznamu dame index na danou polozku v AECListu
            return AECList.Count - 1;
        }

        

        // docasne hash listy pro roztrizeni atr. deklaraci a AP do trid ekvivalenci
        public Dictionary<string, List<int>> AECHashDict = new Dictionary<string, List<int>>();
        public Dictionary<string, List<int>> APECHashDict = new Dictionary<string, List<int>>();


        // seznamy vsech trid ekvivalence AP a atr. deklaraci
        public List<AttrEquivClass> AECList = new List<AttrEquivClass>();
        public List<AttrPartEquivClass> APECList = new List<AttrPartEquivClass>();
        public BinaryHeap<int> ARHeap = new BinaryHeap<int>();
        public List<int> ADList = new List<int>();


        // AEC, reprezentuje tridu ekvivalence atributu
        public class AttrEquivClass
        {
            private List<XmlElement> AttrList;

            public AttrEquivClass()
            {
                AttrList = new List<XmlElement>();
            }
            public AttrEquivClass(XmlElement pAttr)
            {
                AttrList = new List<XmlElement>();
                AttrList.Add(pAttr);
            }
            public AttrEquivClass(List<XmlElement> pAttrs)
            {
                AttrList = pAttrs;
            }

            public List<XmlElement> getAttributes()
            {
                return AttrList;
            }
            public void add(XmlElement pAttr)
            {
                AttrList.Add(pAttr);
            }
            public bool contains(XmlElement pAttr)
            {
                if (AttrList.Contains(pAttr))
                    return true;
                return false;
            }
            public bool checkCandidate(XmlElement pAttr)
            {
                if (AttrList.IsEmpty())
                    return false;

                XmlElement attr = AttrList[0];
                string name1 = attr.GetAttribute("name");
                string type1 = attr.GetAttribute("type");
                string use1 = attr.GetAttribute("use");
                string default1 = attr.GetAttribute("default");
                string name2 = pAttr.GetAttribute("name");
                string type2 = pAttr.GetAttribute("type");
                string use2 = pAttr.GetAttribute("use");
                string default2 = pAttr.GetAttribute("default");

                if (((name1==null && name2==null)||(name1.CompareTo(name2) == 0))
                && ((type1==null && type2==null)||(type1.CompareTo(type2) == 0))
                && ((use1==null && use2==null)||(use1.CompareTo(use2) == 0))
                && ((default1==null && default2==null)||(default1.CompareTo(default2) == 0)))
                    return true;
                return false;
            }
        }
        
        // AP, reprezentuje jednu atributovou cast
        public class AttrPart
        {
            private XmlElement parent;
            public List<int> ADList;
            //public BinaryHeap<int> 

            public AttrPart(XmlElement pParent, List<int> pADList)
            {
                parent = pParent;
                ADList = new List<int>(pADList);
            }

            public XmlElement getParent()
            {
                return parent;
            }
        }
        
        // APEC, reprezentuje tridu ekvivalence atributovych casti
        public class AttrPartEquivClass
        {
            public List<AttrPart> APList;            // seznam AP obsazenych ve tride
            //public List<AttrPart> APRefList;         // seznam AP, ktere se na tridu odkazuji
            public List<XmlElement> refElementList;        // seznam nodu, realizujicich reference na tuto tridu


            public BinaryHeap<int> ARHeap;
            public AttrPart repr;

            public AttrPartEquivClass(AttrPart attrPart, BinaryHeap<int> heap)
            {
                APList = new List<AttrPart>();
                APList.Add(attrPart);
                ARHeap = heap.Copy();
                refElementList = new List<XmlElement>();
                //APRefList = new List<AttrPart>();
                repr = null;
            }

            public bool isEmpty()
            {
                return APList.IsEmpty();
            }
            public void add(AttrPart pAP)
            {
                APList.Add(pAP);
                if ((repr == null)||(repr.ADList.Count > pAP.ADList.Count))
                    repr = pAP;
            }

            /*public void addRef(AttrPart pAP)
            {
                APRefList.Add(pAP);
            }*/

            public void addRefElement(XmlElement elem)
            {
                refElementList.Add(elem);
            }

            public bool checkCandidate(BinaryHeap<int> heap)
            {
                return this.ARHeap.CompareItems(heap);
            }

            public void chooseRepresentative()
            {
                AttrPart rep = null;
                foreach (AttrPart ap in APList)
                {
                    //if (APRefList.Contains(ap))
                    bool contains = false;
                    foreach (XmlElement elem in refElementList)
                    {
                        if (ap.getParent() == (elem.ParentNode))
                            contains = true;
                    }
                    if (contains)
                        continue;
                    if ((rep == null) || (ap.ADList.Count < rep.ADList.Count))
                        rep = ap;
                }
                this.repr = rep;
            }
        }

        public class ComposedAttrGroup
        {
            public XmlNode attrGroup;
            public int ixAPEC;

            public ComposedAttrGroup(XmlNode ag, int ix)
            {
                attrGroup = ag;
                ixAPEC = ix;
            }
        }

        
        private Dictionary<string, string> test(Dictionary<string, string> schemas)
        {
            string s = null;
            int i = 0;
            foreach (AttrEquivClass aec in AECList)
            {
                s += "\n" + i.ToString() + ": ";
                XmlElement attr = aec.getAttributes()[0];
                s += attr.GetAttribute("name") + ", " + attr.GetAttribute("type")
                          + ", " + attr.GetAttribute("use") + ", " + attr.GetAttribute("default");
                s += " (" + aec.getAttributes().Count + ") ";
                i++;
            }
            s += "\n\n";

            //-------------------------------------------------------------------

            i = 0;
            foreach (AttrPartEquivClass apec in APECList)
            {
                s += "\n" + i.ToString() + ":";
                s += "  ARHeap:";
                BinaryHeap<int> heapCopy = apec.ARHeap;
                while (heapCopy.Count > 0)
                {
                    s += " " + heapCopy.RemovePeek().ToString();
                }

                foreach (AttrPart ap in apec.APList)
                {
                    XmlElement parent = ap.getParent();
                    s += "\n    AP: " + parent.Name + " " + parent.GetAttribute("name");
                    s += "\n        ADList:";
                    foreach (int k in ap.ADList)
                    {
                        s += " " + k.ToString();
                    }
                }
                /*
                foreach (AttrPart ap in apec.APRefList)
                {
                    XmlElement parent = ap.getParent();
                    s += "\n    APRef: " + parent.Name + " " + parent.GetAttribute("name");
                }
                 */
                /*
                foreach (XmlElement elem in apec.refElementList)
                {
                    XmlElement parent = (XmlElement) elem.ParentNode;
                    s += "\n    APRef: " + parent.Name + " " + parent.GetAttribute("name");
                }
                i++;*/
            }
            schemas["Info"] = s;
            if (s2 != null)
                schemas["s2"] = s2;
            return schemas;
        }
        #endregion
    }
}



