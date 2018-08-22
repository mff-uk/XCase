using System;
using System.Linq;
using System.Collections.Generic;

namespace XCase.Model
{
	/// <summary>
	/// Stores visualization information for Associations 
	/// </summary>
	public class AssociationViewHelper : ConnectionViewHelper<Association>
	{
		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		public AssociationViewHelper()
		{
		}

		public AssociationViewHelper(Diagram diagram): base(diagram)
		{			
			MainLabelViewHelper = new AssociationLabelViewHelper(diagram);
		}

		#region Main Label properties

		public AssociationLabelViewHelper MainLabelViewHelper { get; set; }

		#endregion

		private bool useDiamond;
		public bool UseDiamond
		{
			get
			{
				return useDiamond;
			}
			set
			{
				if (AssociationEndsViewHelpers.Count > 2 && value == false)
				{
					throw new InvalidOperationException("Association must have an association diamond. It has more than 2 ends. ");
				}
				
				useDiamond = value;
				OnPropertyChanged("UseDiamond");
			}
		}

		public bool ForceUseDiamond { get; set; }

		/// <summary>
		/// Returns association points, but only for associations that do not use central diamond 
		/// (associations with more than two ends)
		/// </summary>
		/// <seealso cref="UseDiamond"/>
		public override ObservablePointCollection Points
		{
			get
			{
				if (UseDiamond)
				{
					throw new InvalidOperationException("Points collection is valid only if UseDiamond is false.");
				}
				return associationEndsViewHelpers[0].Points;
			}
		}


		private readonly List<AssociationEndViewHelper> associationEndsViewHelpers = new List<AssociationEndViewHelper>();
		public IList<AssociationEndViewHelper> AssociationEndsViewHelpers
		{
			get
			{
				return associationEndsViewHelpers;
			}
		}

		/// <summary>
		/// Creates <see cref="AssociationEndViewHelper"/> for each <paramref name="association"/>'s end.  
		/// </summary>
		/// <param name="association">model association</param>
		public void CreateEndsViewHelpers(Association association)
		{
			#region validate operation
			foreach (AssociationEnd associationEnd in association.Ends)
			{
				if (!Diagram.IsElementPresent(associationEnd.Class))
				{
					throw new InvalidOperationException(
						string.Format(
							"Association cannot be visualized on the diagram, association end {0} is not included in the diagram.",
							associationEnd));
				}
			}
			#endregion

			//associationEndsViewHelpers.Clear();
			
			List<AssociationEndViewHelper> used = new List<AssociationEndViewHelper>();

			// add new ends
			foreach (AssociationEnd associationEnd in association.Ends)
			{
				AssociationEndViewHelper endViewHelper = associationEndsViewHelpers.Where(helper => helper.AssociationEnd == associationEnd).SingleOrDefault();

				if (endViewHelper == null)
				{
					endViewHelper = new AssociationEndViewHelper(Diagram, this, associationEnd);
					associationEndsViewHelpers.Add(endViewHelper);
				}

				used.Add(endViewHelper);
			}

			if (used.Count == 2 && associationEndsViewHelpers.Count > 2 && UseDiamond)
			{
				associationEndsViewHelpers[0].Points.Clear();
			}

			associationEndsViewHelpers.RemoveAll(helper => !used.Contains(helper));
			
			if (association.Ends.Count > 2 || ForceUseDiamond)
			{
				UseDiamond = true;
			}
			else
			{
				UseDiamond = false; 
			}
		}

		public override ViewHelper Clone(Diagram diagram)
		{
			return new AssociationViewHelper(diagram);
		}

		public override void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			AssociationViewHelper copyAssociationViewHelper = (AssociationViewHelper) copy;
			copyAssociationViewHelper.UseDiamond = UseDiamond;

			MainLabelViewHelper.FillCopy(copyAssociationViewHelper.MainLabelViewHelper, modelMap);

			for (int i = 0; i < AssociationEndsViewHelpers.Count; i++)
			{
				AssociationEndViewHelper associationEndViewHelper = AssociationEndsViewHelpers[i];
				AssociationEnd copyEnd = ((Association) Map(associationEndViewHelper.AssociationEnd.Association, modelMap)).Ends[i];
				System.Diagnostics.Debug.Assert(copyEnd.Class == Map(associationEndViewHelper.AssociationEnd.Class, modelMap));
				AssociationEndViewHelper endViewHelperCopy = new AssociationEndViewHelper(copy.Diagram, copyAssociationViewHelper, copyEnd);
				associationEndViewHelper.FillCopy(endViewHelperCopy, modelMap);
				copyAssociationViewHelper.AssociationEndsViewHelpers.Add(endViewHelperCopy);
			}

			base.FillCopy(copy, modelMap);
		}
	}
}