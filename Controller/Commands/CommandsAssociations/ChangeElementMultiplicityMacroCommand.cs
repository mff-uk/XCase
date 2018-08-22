using MultiplicityElement = XCase.Model.MultiplicityElement;
using XCase.Model;
using System;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Sets new multiplicity with dependencies
	/// </summary>
    public class ChangeElementMultiplicityMacroCommand : MacroCommand<ModelController>
    {
        public ChangeElementMultiplicityMacroCommand(ModelController controller)
    		: base(controller)
    	{
    		Description = CommandDescription.CHANGE_MULTIPLICITY;
    	}

		/// <summary>
		/// Modified element
		/// </summary>
		[MandatoryArgument]
        public MultiplicityElement Element { get; set; }

        private uint ? oldLower { get; set; }
        
		private NUml.Uml2.UnlimitedNatural ? oldUpper { get; set; }

		/// <summary>
		/// Lower multiplicity bound
		/// </summary>
        public uint? Lower { get; set; }
    	
		/// <summary>
		/// Upper multiplicity bound
		/// </summary>
        public NUml.Uml2.UnlimitedNatural? Upper { get; set; }

        public void InitializeCommand()
        {
            if (Element is PSMAttribute)
            {
                if ((Element as PSMAttribute).RepresentedAttribute != null)
                {
                    Dialogs.OkCancelDialog d = new Dialogs.OkCancelDialog();
                    d.Title = "Change multiplicity";
                    d.PrimaryContent = "Change multiplicity";
                    d.SecondaryContent = string.Format("Change represented attribute's ({0}) multiplicity as well?", Element);
                    d.OkButtonContent = "Yes";
                    d.CancelButtonContent = "No";
                    if (d.ShowDialog() == true)
                    {
                        ChangeElementMultiplicityCommand c1 = ChangeElementMultiplicityCommandFactory.Factory().Create(Controller) as ChangeElementMultiplicityCommand;
                        c1.Element = (Element as PSMAttribute).RepresentedAttribute;
                        c1.Lower = Lower;
                        c1.Upper = Upper;
                        Commands.Add(c1);
                    }
                }
            }
            else if (Element is Property)
            {
                if ((Element as Property).DerivedPSMAttributes.Count > 0)
                {
                    Dialogs.OkCancelDialog d = new Dialogs.OkCancelDialog();
                    d.Title = "Change multiplicity";
                    d.PrimaryContent = "Change multiplicity";
                    string temp = Environment.NewLine;
                    foreach (PSMAttribute a in (Element as Property).DerivedPSMAttributes)
                    {
                        temp += a.Class.Diagram.Caption + ": " + a.Class + "." + a + Environment.NewLine;
                    }
                    temp = temp.Remove(temp.Length - 1);
                    d.SecondaryContent = "Change derived attributes' multiplicity as well?" + Environment.NewLine + "Derived attributes:" + Environment.NewLine + temp;
                    d.OkButtonContent = "Yes";
                    d.CancelButtonContent = "No";
                    if (d.ShowDialog() == true)
                        foreach (PSMAttribute a in (Element as Property).DerivedPSMAttributes)
                        {
                            ChangeElementMultiplicityCommand c2 = ChangeElementMultiplicityCommandFactory.Factory().Create(Controller) as ChangeElementMultiplicityCommand;
                            c2.Element = a;
                            c2.Lower = Lower;
                            c2.Upper = Upper;
                            Commands.Add(c2);
                        }
                }
            }
            
            ChangeElementMultiplicityCommand c = ChangeElementMultiplicityCommandFactory.Factory().Create(Controller) as ChangeElementMultiplicityCommand;
            c.Element = Element;
            c.Lower = Lower;
            c.Upper = Upper;
            Commands.Add(c);
        }
    }

    #region ChangeElementMultiplicityMacroCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="ChangeElementMultiplicityMacroCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class ChangeElementMultiplicityMacroCommandFactory : ModelCommandFactory<ChangeElementMultiplicityMacroCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
        private ChangeElementMultiplicityMacroCommandFactory()
		{
		}

		/// <summary>
        /// Creates new instance of ChangeElementMultiplicityMacroCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
            return new ChangeElementMultiplicityMacroCommand(modelController);
		}
	}

	#endregion
}