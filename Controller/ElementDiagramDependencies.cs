using System.Collections.Generic;
using System.Linq;
using XCase.Model;

namespace XCase.Controller
{
	/// <summary>
	/// Holds usages of elemens in diagrams
	/// </summary>
	public class ElementDiagramDependencies : Dictionary<Element, List<Diagram>>
	{
		/// <summary>
		/// Finds occurrences of <paramref name="elements"/> in diagrams in <paramref name="project"/>.
		/// </summary>
		/// <param name="project">project where occurrences are searched</param>
		/// <param name="elements">searched elements</param>
		/// <param name="excludedDiagram">this diagram will be ignored (can be left to null)</param>
		/// <returns>set of referencing diagrams for each element</returns>
		public static ElementDiagramDependencies FindElementDiagramDependencies(Project project, IList<Element> elements, Diagram excludedDiagram)
		{
			ElementDiagramDependencies result = new ElementDiagramDependencies();
			foreach (Element element in elements)
			{
				foreach (Diagram diagram in project.PSMDiagrams.Cast<Diagram>().Union(project.PIMDiagrams.Cast<Diagram>()))
				{
					if (diagram != excludedDiagram && diagram.IsElementPresent(element))
					{
						if (!result.ContainsKey(element))
							result[element] = new List<Diagram>();
						if (!result[element].Contains(diagram))
							result[element].Add(diagram);
					}
				}
			}
			return result;
		}
	}
}