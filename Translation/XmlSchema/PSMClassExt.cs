using XCase.Model;
using System.Linq;

namespace XCase.Translation.XmlSchema
{
	/// <summary>
	/// Defines some extension methods for 
	/// <see cref="PSMClass"/> that are used during translation. 
	/// </summary>
	public static class PSMClassExt
	{
		/// <summary>
		/// Returns true if the <paramref name="specialized"/> class
		/// can substitute its parent in an associatoin.
		/// </summary>
		/// <param name="specialized"> specialized class</param>
		/// <returns>true if the <paramref name="specialized"/> class
		/// can substitute its parent in an associatoin</returns>
		public static bool CanSubstituteInAssociation(this PSMClass specialized)
		{
			/* 
			 * jeste jedna takova slozita podminka: pokud ma abstract PSM class prirazeny 
			 * element label, tak nebude odvozena odpovidajici element declaration. 
			 * vyjimkou je, pokud je od ni podedena nejaka non-abstract PSM class bez element labelu
			 */ 

			if (!specialized.IsAbstract)
			{
				return true; 
			}
			else
			{
				return specialized.NonAbstractWithoutLabelRecursive();
			}
		}

		/// <summary>
		/// Class can be declared as element if it has an element label
		/// and it is not abstract (or if it is abstract and it has some 
		/// non-abstract specification without element label)
		/// </summary>
		/// <param name="psmClass"></param>
		/// <returns></returns>
		public static bool CanBeDeclaredAsElement(this PSMClass psmClass)
		{
			if (psmClass.HasElementLabel)
			{
				if (psmClass.IsAbstract)
				{
					if (NonAbstractWithoutLabelRecursive(psmClass))
					{
						return true;
					}
					return false;
				}
				else
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns true if <paramref name="psmClass"/> does not have an element label 
		/// and is not abstract or there is a specialization that does not have an element label
		/// and is not abstract (and there are no specialziations with element labels between them).
		/// </summary>
		/// <param name="psmClass">tested class</param>
		public static bool NonAbstractWithoutLabelRecursive(this PSMClass psmClass)
		{
			return NonAbstractWithoutLabelRecursive(psmClass, true);
		}

		/// <summary>
		/// Returns true if <paramref name="psmClass"/> does not have an element label
		/// and is not abstract or there is a specialization that does not have an element label
		/// and is not abstract (and there are no specialziations with element labels between them).
		/// </summary>
		/// <param name="psmClass">tested class</param>
		/// <param name="forceContinueOnSubclasses">if set to <c>true</c> testing continues on subclasses even 
		/// when <paramref name="psmClass"/> has an element label. Only for recursive calls.</param>
		/// <returns></returns>
		private static bool NonAbstractWithoutLabelRecursive(this PSMClass psmClass, bool forceContinueOnSubclasses)
		{
			if (!psmClass.HasElementLabel && !psmClass.IsAbstract)
			{
				return true;
			}
			if ((!psmClass.HasElementLabel || forceContinueOnSubclasses) && 
				psmClass.Specifications.Any(generalization => NonAbstractWithoutLabelRecursive((PSMClass)generalization.Specific, false)))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Returns true of <paramref name="psmClass"/> is a specialization 
		/// of some of the root classes in <paramref name="diagram"/>.
		/// </summary>
		/// <param name="psmClass">psm class</param>
		/// <param name="diagram">PSM diagram</param>
		/// <param name="abstractRootsOnly">if set to true, only roots with <see cref="Class.IsAbstract"/> flag are considered</param>
		/// <param name="root">root that is specialized by <paramref name="psmClass"/> (if any)</param>
		/// <returns></returns>
		public static bool IsClassSpecializedRoot(this PSMClass psmClass, PSMDiagram diagram, bool abstractRootsOnly, out PSMClass root)
		{
			PSMClass r = psmClass;
			while (r.Generalizations.Count() != 0)
				r = (PSMClass)r.Generalizations.First().General;
			root = r;
			return ((r.IsAbstract || !abstractRootsOnly) && diagram.Roots.Contains(r));
		}
	}
}