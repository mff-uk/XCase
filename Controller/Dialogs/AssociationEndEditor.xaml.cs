using System.Windows.Controls;
using NUml.Uml2;
using XCase.Model;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Interaction logic for AssociationEndEditor.xaml
	/// </summary>
	public partial class AssociationEndEditor : UserControl
	{
		public AssociationEnd AssociationEnd { get; private set; }

		public AssociationEndEditor()
		{
			InitializeComponent();
		}

		internal int oldKindIndex;

		public AssociationEndEditor(AssociationEnd associationEnd)
			: this()
		{

			AssociationEnd = associationEnd;

			this.tbRole.Text = associationEnd.Name;
			this.lClass.Content = associationEnd.Class.Name;
			this.tbLower.Text = associationEnd.Lower.ToString();
			this.tbUpper.Text = associationEnd.Upper.ToString();

			switch (associationEnd.Aggregation)
			{
				case AggregationKind.none:
					cbType.SelectedIndex = 0;
					break;
				case AggregationKind.shared:
					cbType.SelectedIndex = 1;
					break;
				case AggregationKind.composite:
					cbType.SelectedIndex = 2;
					break;
			}

			oldKindIndex = cbType.SelectedIndex;
		}
	}
}
