using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using XCase.Model;

namespace Tests.ModelIntegrity
{
	public static class ModelConsistency
	{
		public static void CheckEverything(Project project)
		{
			foreach (Diagram diagram in project.Diagrams)
			{
				CheckViewHelpersDiagram(diagram);
				CheckElementSchema(diagram.DiagramElements.Keys, project.Schema, null);
				if (diagram is PSMDiagram)
				{
					CheckPsmElementsDiagram((PSMDiagram)diagram);
					CheckPsmParentsAndRoots((PSMDiagram)diagram);
					CheckPsmClassParent((PSMDiagram)diagram);
				}
			}
		}

		private static void CheckPsmClassParent(PSMDiagram diagram)
		{
			foreach (PSMClass psmClass in diagram.DiagramElements.Keys.OfType<PSMClass>())
			{
				if (psmClass.ParentAssociation != null)
				{
					if (psmClass.ParentAssociation.Child != psmClass)
					{
						throw new ModelConsistencyException(string.Format("Bad class parent association component {0}", psmClass));
					}
					if (psmClass.ParentUnion != null)
					{
						throw new ModelConsistencyException(string.Format("Bad class parent association component {0}", psmClass));
					}
				}
				else if (psmClass.ParentUnion != null)
				{
					if (!psmClass.ParentUnion.Components.Contains(psmClass))
					{
						throw new ModelConsistencyException(string.Format("Bad class parent union component {0}", psmClass));
					}
					if (psmClass.ParentAssociation != null)
					{
						throw new ModelConsistencyException(string.Format("Bad class parent association component {0}", psmClass));
					}
				}
				else
				{
					if (!diagram.Roots.Contains(psmClass) && psmClass.Generalizations.Count == 0)
					{
						throw new ModelConsistencyException(string.Format("Bad class {0}", psmClass));
					}
				}
			}
		}

		public static void CheckPsmParentsAndRoots(PSMDiagram diagram)
		{
			foreach (Element element in diagram.DiagramElements.Keys)
			{
				PSMSubordinateComponent subordinateComponent = (element as PSMSubordinateComponent);
				if (subordinateComponent != null)
				{
					if (subordinateComponent.Parent == null && !diagram.Roots.Contains((PSMClass)subordinateComponent))
					{
						throw new ModelConsistencyException(string.Format("Bad subordinate component {0}", subordinateComponent));
					}
					if (subordinateComponent.Parent != null)
					{
						if (!subordinateComponent.Parent.Components.Contains(subordinateComponent))
						{
							throw new ModelConsistencyException(string.Format("Bad subordinate component {0}", subordinateComponent));
						}
					}
				}

				PSMSuperordinateComponent superordinateComponent = element as PSMSuperordinateComponent;

				if (superordinateComponent != null)
				{
					foreach (PSMSubordinateComponent component in superordinateComponent.Components)
					{
						if (component.Parent != superordinateComponent)
						{
							throw new ModelConsistencyException(string.Format("Bad superordinateComponent component {0}", superordinateComponent));
						}
					}
				}
			}
		}

		public static void CheckPsmElementsDiagram(PSMDiagram diagram)
		{
			foreach (Element element in diagram.DiagramElements.Keys)
			{
				PSMElement psmElement = element as PSMElement;
				if (psmElement != null)
				{
					if (psmElement.Diagram != diagram)
					{
						throw new ModelConsistencyException(string.Format("Element {0}  has wrong diagram.", psmElement));
					}
				}
			}
		}

		public static void CheckViewHelpersDiagram(Diagram diagram)
		{
			foreach (KeyValuePair<Element, ViewHelper> kvp in diagram.DiagramElements)
			{
				if (kvp.Value.Diagram != diagram)
				{
					throw new ModelConsistencyException(string.Format("ViewHelper {0} for element {1} has wrong diagram.", kvp.Key,
																	  kvp.Value));
				}
			}
		}

		public static void CheckElementSchema(IEnumerable<Element> elements, Schema schema, List<Element> checkedAlready)
		{
			if (checkedAlready == null)
				checkedAlready = new List<Element>();
			foreach (Element element in elements)
			{
				CheckElementSchema(element, schema, checkedAlready);
			}
		}

		public static void CheckElementSchema(Element element, Schema schema, List<Element> checkedAlready)
		{
			if (element == null)
			{
				return;
			}

			if (checkedAlready != null)
			{
				if (checkedAlready.Contains(element))
					return;
				else
					checkedAlready.Add(element); 
			}

			if (element.Schema != schema)
			{
				throw new ModelConsistencyException(string.Format("Schema of element {0} differs.", element));
			}

			// reiterate through properties
			Type type = element.GetType();
			Type elementInterfaceType = typeof(Element);
			Type elementCollectionType = typeof(IEnumerable);

			foreach (PropertyInfo propertyInfo in type.GetProperties())
			{
				if (elementInterfaceType.IsAssignableFrom(propertyInfo.PropertyType))
				{
					//System.Diagnostics.Debug.WriteLine(String.Format("Checking property {0}.{1}.", type.Name, propertyInfo.Name));
					Element value = propertyInfo.GetValue(element, null) as Element;
					if (value != null)
					{
						CheckElementSchema(value, schema, checkedAlready);
					}
				}

				if (elementCollectionType.IsAssignableFrom(propertyInfo.PropertyType))
				{
					IEnumerable theCollection = propertyInfo.GetValue(element, null) as IEnumerable;
					if (theCollection != null)
					{
						foreach (object item in theCollection)
						{
							if (item is Element)
							{
								CheckElementSchema(element, schema, checkedAlready);
							}
						}
						
					}
				}
			}

		}


	}

	public class ModelConsistencyException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Exception"/> class.
		/// </summary>
		public ModelConsistencyException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error. </param>
		public ModelConsistencyException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception. </param><param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
		public ModelConsistencyException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Exception"/> class with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown. </param><param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination. </param><exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception><exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
		protected ModelConsistencyException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}