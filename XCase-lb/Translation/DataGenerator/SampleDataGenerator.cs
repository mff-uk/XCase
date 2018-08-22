using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using XCase.Model;
using NamingSupport = XCase.Translation.XmlSchema.NamingSupport;

namespace XCase.Translation.DataGenerator
{
    public class SampleDataGenerator : DiagramTranslator<DataGeneratorContext, XmlNode>
    {
        public int InfinityBound = 10;

        public bool MinimalTree { get; set; } 

        public bool UseAttributesDefaultValues { get; set; }

        public bool GenerateComments { get; set; }

        private readonly DataTypeValuesGenerator valuesGenerator;

        public NamingSupport namingSupport;

        public SampleDataGenerator()
        {
            UseAttributesDefaultValues = true;
            GenerateComments = true;
            namingSupport = new NamingSupport { Log = Log };
            valuesGenerator = new DataTypeValuesGenerator(true);
        }

        public string GenerateSubtree(PSMElement element)
        {
            DataGeneratorContext c = new DataGeneratorContext();
            if (element is PSMClass)
            {
                TranslateClass((PSMClass) element, c);
            }
            else if (element is PSMAssociation)
            {
                TranslateAssociation((PSMAssociation) element, c);
            }
            else if (element is PSMClassUnion)
            {
                TranslateClassUnion((PSMClassUnion) element, c);
            }
            else if (element is PSMContentChoice)
            {
                TranslateContentChoice((PSMContentChoice) element, c);
            }
            else if (element is PSMContentContainer)
            {
                TranslateContentContainer((PSMContentContainer) element, c);
            }
            else if (element is PSMAttributeContainer)
            {
                TranslateAttributeContainer((PSMAttributeContainer) element, c);
            }
            else if (element is PSMAttribute)
            {
                TranslateAttributeAsElement((PSMAttribute) element, c);
            }
            return c.Document.InnerXml;
        }

        private const string AttributeQuotation = "\"";

        public string GenerateAttribute(PSMAttribute attribute)
        {
            if (attribute.Upper > 1 || attribute.Lower > 1)
                Log.AddError(string.Format("Attribute {0}.{1} has upper multiplicity {2} that is not valid for XML documents. ", attribute.Class.Name, attribute.Name, attribute.MultiplicityString));

            if (attribute.Lower == 1)
            {
                string value;
                if (attribute.Default != null && UseAttributesDefaultValues)
                    value = attribute.Default;
                else
                    value = valuesGenerator.GenerateValue(attribute.Type);

                string attributeName = namingSupport.NormalizeTypeName(attribute, at => at.AliasOrName);
                return string.Format("{0}={1}{2}{1}", attributeName, AttributeQuotation, value);
            }
            else // Lower == 0
                return string.Empty;
        }

        public override string Translate(PSMDiagram diagram)
        {
            Diagram = diagram;

            //TODO: This is only fixed, it needs to take into account the Content Containers!
            IEnumerable<PSMClass> rootCandidates = diagram.Roots.Where(c => c is PSMClass && (c as PSMClass).HasElementLabel).Cast<PSMClass>();

            if (rootCandidates.Count() == 0)
            {
                Log.AddError("No possible root element. Consider assigning an element label to one of the root classes.");
                return String.Empty;
            }
    
            try
            {
                PSMClass root = rootCandidates.ChooseOneRandomly();
                DataGeneratorContext context = new DataGeneratorContext();

                TranslateComments(null, context);
                TranslateClass(root, context);
                AddSchemaArguments((XmlElement)context.ClassNodes[root], context);

                // write with indentation
                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "  ";
                settings.NewLineChars = "\r\n";
                settings.NewLineHandling = NewLineHandling.Replace;
                XmlWriter writer = XmlWriter.Create(sb, settings);
                // ReSharper disable AssignNullToNotNullAttribute
                context.Document.Save(writer);
                // ReSharper restore AssignNullToNotNullAttribute
                // ReSharper disable PossibleNullReferenceException
                writer.Close();
                // ReSharper restore PossibleNullReferenceException
                
                return sb.ToString();
            }
            catch (XmlSchemaException e)
            {
                if (e.Message.Contains("is recursive and causes infinite nesting"))
                    return null;
                else
                    throw;
            }
        }

        private void TranslateComments(Element element, DataGeneratorContext context)
        {
            if (!GenerateComments) 
                return;

            IEnumerable<Comment> comments;
            if (element != null)
                comments = element.Comments;
            else
                comments = Diagram.DiagramElements.Keys.OfType<Comment>().Where(comment => comment.AnnotatedElement is Model.Model);

            foreach (Comment comment in comments)
            {
                XmlComment xmlComment = context.Document.CreateComment(comment.Body);
                if (context.CurrentElement != null)
                    context.CurrentElement.AppendChild(xmlComment);
                else
                    context.Document.AppendChild(xmlComment);
            }
        }

        protected override XmlNode TranslateClass(PSMClass psmClass, DataGeneratorContext context)
        {
            if (psmClass.Specifications.Count > 0)
            {
                // identify the set of possible instances
                List<PSMClass> candidates = new List<PSMClass>();
                FindSpecificationsRecursive(ref candidates, psmClass);
                PSMClass selectedClass = candidates.Where(candidate => !candidate.IsAbstract && candidate.HasElementLabel).ChooseOneRandomly();
                return TranslateInstantiatedClass(selectedClass, false, context);
            }
            else
            {
                return TranslateInstantiatedClass(psmClass, false, context);
            }
        }

        private static void FindSpecificationsRecursive(ref List<PSMClass> result, PSMClass psmClass)
        {
            result.Add(psmClass);

            foreach (Generalization generalization in psmClass.Specifications)
            {
                FindSpecificationsRecursive(ref result, (PSMClass)generalization.Specific);
            }
        }

        public string ProjectNamespace
        {
            get
            {
                return Diagram.Project.Schema.XMLNamespaceOrDefaultNamespace;
            }
        }

        private XmlNode TranslateInstantiatedClass(PSMClass psmClass, bool ignoreAncestors, DataGeneratorContext context)
        {
            XmlElement xmlElement = null;
            XmlElement prevCurrentElement = context.CurrentElement;

            TranslateComments(psmClass, context);
            PathElements.Enqueue(psmClass);
            if (!context.TranslatingRepresentedClass && !context.TranslatingAncestors)
            {
                if (psmClass.HasElementLabel)
                {
                    string elementName = namingSupport.NormalizeTypeName(psmClass, c => c.ElementName);
                    if (context.CurrentElement == null)
                    {
                        xmlElement = context.Document.CreateElement(elementName, ProjectNamespace);
                        context.Document.AppendChild(xmlElement);
                    }
                    else
                    {
                        xmlElement = context.Document.CreateElement(elementName, ProjectNamespace);
                        context.CurrentElement.AppendChild(xmlElement);
                    }
                    context.CurrentElement = xmlElement;
                    context.ClassNodes[psmClass] = xmlElement;
                }
            }
            else
            {
                context.TranslatingRepresentedClass = false;
                context.TranslatingAncestors = false;
            }

            if (psmClass.Generalizations.Count > 0 && !ignoreAncestors)
            {
                TranslateAncestors(psmClass, context);
            }

            TranslateAttributes(psmClass.Attributes, context);

            if (psmClass.IsStructuralRepresentative)
            {
                context.TranslatingRepresentedClass = true;
                TranslateClass(psmClass.RepresentedPSMClass, context);
                context.TranslatingRepresentedClass = false;
            }


            TranslateComponents(psmClass, context);

            PathElements.Dequeue();

            context.CurrentElement = prevCurrentElement;
            return xmlElement;
        }

        private static void AddSchemaArguments(XmlElement rootElement, DataGeneratorContext context)
        {
            XmlAttribute schemaInstance = context.Document.CreateAttribute("xmlns:xsi", "http://www.w3.org/2000/xmlns/");
            schemaInstance.Value = "http://www.w3.org/2001/XMLSchema-instance";
            rootElement.Attributes.Append(schemaInstance);
        }

        private void TranslateAncestors(PSMClass psmClass, DataGeneratorContext context)
        {
            List<PSMClass> ancestors = FindClassAncestors(psmClass);

            foreach (PSMClass ancestor in ancestors)
            {
                context.TranslatingAncestors = true;
                TranslateInstantiatedClass(ancestor, true, context);
            }
            context.TranslatingAncestors = false;
        }

        private static List<PSMClass> FindClassAncestors(PSMClass psmClass)
        {
            PSMClass level = psmClass;
            List<PSMClass> ancestors = new List<PSMClass>();
            while (level != null)
            {
                if (level.Generalizations.Count > 1)
                    throw new Exception("PSM class can have only one generalization, multiple inheritance is forbidden. ");
                else if (level.Generalizations.Count == 1)
                {
                    PSMClass generalClass = (PSMClass)level.Generalizations[0].General;
                    ancestors.Add(generalClass);
                    level = generalClass;
                }
                else
                {
                    level = null;
                }
            }
            ancestors.Reverse();
            return ancestors;
        }

        private void TranslateComponents(PSMSuperordinateComponent psmSuperordinateComponent, DataGeneratorContext context)
        {
            foreach (PSMSubordinateComponent component in psmSuperordinateComponent.Components)
            {
                TranslateSubordinateComponent(component, context);
            }
        }

        protected void TranslateAttributes(IList<Property> attributes, DataGeneratorContext context)
        {
            foreach (PSMAttribute attribute in attributes)
            {
                TranslateAttribute(attribute, context);
            }
        }

        private void TranslateAttribute(PSMAttribute attribute, DataGeneratorContext context)
        {
            if (attribute.Upper > 1 || attribute.Lower > 1)
                Log.AddError(string.Format("Attribute {0}.{1} has upper multiplicity {2} that is not valid for XML documents. ", attribute.Class.Name, attribute.Name, attribute.MultiplicityString));

            bool appears = false;

            // mandatory attribute
            if (attribute.Lower == 1)
                appears = true;

            if (attribute.Lower == 0 || attribute.Lower == null)
            {
                appears = RandomGenerator.Toss(2, 1);
            }

            if (appears)
            {
                string value;
                if (attribute.Default != null && UseAttributesDefaultValues)
                    value = attribute.Default;
                else
                    value = valuesGenerator.GenerateValue(attribute.Type);

                string attributeName = namingSupport.NormalizeTypeName(attribute, at => at.AliasOrName);
                XmlAttribute a = context.Document.CreateAttribute(attributeName);
                a.Value = value;
                context.CurrentElement.Attributes.Append(a);
            }
        }

        private void TranslateAttributeAsElement(PSMAttribute attribute, DataGeneratorContext context)
        {
            int count = ChooseMultiplicity(attribute);

            for (int i = 0; i < count; i++)
            {
                string attributeName = namingSupport.NormalizeTypeName(attribute, a => a.AliasOrName);
                XmlElement element = context.Document.CreateElement(attributeName, ProjectNamespace);
                string value;
                if (attribute.Default != null && UseAttributesDefaultValues)
                    value = attribute.Default;
                else
                    value = valuesGenerator.GenerateValue(attribute.Type);
                XmlText text = context.Document.CreateTextNode(value);
                context.CurrentElement.AppendChild(element);
                element.AppendChild(text);
            }
        }

        protected override void TranslateContentChoice(PSMContentChoice contentChoice, DataGeneratorContext context)
        {
            TranslateComments(contentChoice, context);
            PSMSubordinateComponent selectedPath = contentChoice.Components.ChooseOneRandomly();
            TranslateSubordinateComponent(selectedPath, context);
        }

        protected override void TranslateContentContainer(PSMContentContainer contentContainer, DataGeneratorContext context)
        {
            XmlElement prevCurrentElement = context.CurrentElement;

            TranslateComments(contentContainer, context);
            string elementName = namingSupport.NormalizeTypeName(contentContainer, c => contentContainer.Name);
            XmlElement xmlElement = context.Document.CreateElement(elementName, ProjectNamespace);
            context.CurrentElement.AppendChild(xmlElement);
            // set new current element
            context.CurrentElement = xmlElement;
            TranslateComponents(contentContainer, context);
            // return current element to previous value
            context.CurrentElement = prevCurrentElement;
        }

        protected override void TranslateAssociation(PSMAssociation association, DataGeneratorContext context)
        {
            PathElements.Enqueue(association);
            TranslateComments(association, context);
            base.TranslateAssociation(association, context);
            PathElements.Dequeue();
        }

        public Queue<Element> PathElements = new Queue<Element>();

        public int GetElementOccurrences(Element element)
        {
            return PathElements.Count(pathElement => pathElement == element);
        }

        protected override void TranslateAssociationChild(PSMAssociationChild associationChild, DataGeneratorContext context)
        {
            int count = 1;
            if (associationChild.ParentAssociation != null)
            {
                count = ChooseMultiplicity(associationChild.ParentAssociation);
            }

            for (int i = 0; i < count; i++)
            {
                if (associationChild is PSMClass)
                {
                    PSMClass psmClass = (PSMClass)associationChild;
                    if (!psmClass.HasElementLabel)
                    {
                        XmlComment xmlComment = context.Document.CreateComment(string.Format("Content group {0} {1}", psmClass.Name, i));
                        context.CurrentElement.AppendChild(xmlComment);
                    }
                    TranslateClass(psmClass, context);
                }
                else
                {
                    TranslateClassUnion((PSMClassUnion)associationChild, context);
                }
            }
        }

        private void TranslateClassUnion(PSMClassUnion classUnion, DataGeneratorContext context)
        {
            PSMAssociationChild selectedPath = classUnion.Components.ChooseOneRandomly();
            TranslateComments(classUnion, context);
            TranslateAssociationChild(selectedPath, context);
        }

        protected override void TranslateAttributeContainer(PSMAttributeContainer attributeContainer, DataGeneratorContext context)
        {
            foreach (PSMAttribute attribute in attributeContainer.PSMAttributes)
            {
                TranslateComments(attributeContainer, context);
                TranslateAttributeAsElement(attribute, context);
            }
        }

        private int ChooseMultiplicity(MultiplicityElement multiplicityElement)
        {
            int lower = multiplicityElement.Lower.HasValue ? (int)multiplicityElement.Lower.Value : 0;
            int upper = multiplicityElement.Upper != NUml.Uml2.UnlimitedNatural.Infinity ? (int)multiplicityElement.Upper.Value : InfinityBound;

            if (multiplicityElement is PSMAssociation)
            {
                PSMAssociation association = (PSMAssociation)multiplicityElement;
                int occurrences = GetElementOccurrences(association);
                if (occurrences > 10 && association.Lower > 0)
                {
                    Log.AddError(string.Format("Association {0} is recursive and causes infinite nesting.", association));
                    throw new XmlSchemaException(string.Format("Association {0} is recursive and causes infinite nesting.", association));
                }
                if (MinimalTree)
                    return lower;
                else 
                    return RandomGenerator.Next(lower, upper + 1, occurrences - 1);
            }
            else
            {
                if (MinimalTree)
                    return lower;
                else 
                    return RandomGenerator.Next(lower, upper + 1);
            }
        }
    }

    public class DataGeneratorContext
    {
        public XmlDocument Document { get; private set; }

        public XmlElement CurrentElement { get; set; }

        public Dictionary<PSMClass, XmlNode> ClassNodes { get; private set; }

        public bool TranslatingRepresentedClass { get; set; }

        public bool TranslatingAncestors { get; set; }

        public DataGeneratorContext()
        {
            Document = new XmlDocument();
            ClassNodes = new Dictionary<PSMClass, XmlNode>();
        }
    }
}
