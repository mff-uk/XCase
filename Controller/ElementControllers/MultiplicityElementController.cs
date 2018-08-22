using System;
using System.Windows;
using NUml.Uml2;
using XCase.Controller.Commands;
using XCase.Controller.Dialogs;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller for any MultiplicityElement
    /// </summary>
    public class MultiplicityElementController
	{
		public static UnlimitedNatural ParseUnlimitedNatural(string value)
		{
			if (value == "*")
				return UnlimitedNatural.Infinity;
			uint result;

			if (!uint.TryParse(value, out result))
			{
				throw new FormatException(CommandError.CMDERR_CARDINALITY_FORMAT + value);
			}

			return result;
		}

		public static uint? ParseNullabelUint(string value)
		{
			uint result;
			if (value == string.Empty)
				return null;

			if (!uint.TryParse(value, out result))
			{
				throw new FormatException(CommandError.CMDERR_CARDINALITY_FORMAT + value);
			}

			return result;
		}

		public static bool ParseMultiplicityString(string newCardinality, out uint? lower, out UnlimitedNatural upper)
		{
			try
			{
				if (newCardinality.Contains(".."))
				{
					int pos = newCardinality.IndexOf("..");
					lower = ParseNullabelUint(newCardinality.Substring(0, pos));
					upper = ParseUnlimitedNatural(newCardinality.Substring(pos + 2));
				}
				else
				{
					lower = uint.Parse(newCardinality);
					upper = (UnlimitedNatural)lower;
				}
				return true; 
			}
			catch (FormatException)
			{
				ErrDialog errorDialog = new ErrDialog();
				errorDialog.Title = "Bad cardinality format";
				errorDialog.label1.Text = "Solution:";
				errorDialog.textBlock1.Content  = CommandError.CMDERR_CARDINALITY_FORMAT + newCardinality;
				errorDialog.label2.Text = "";
				errorDialog.tbCommand.Text = "Input cardinality in following format: '<lower>..<upper>' or '<cardinality>' or '<lower>..*'";
				errorDialog.tbExMsg.Visibility = Visibility.Collapsed;
				errorDialog.ShowDialog();
				lower = 0;
				upper = 0;
				return false;
			}
		}

		public static void ChangeMultiplicityOfElement(Model.MultiplicityElement element, XCase.Model.Element associatedElement, uint ? lower, UnlimitedNatural upper, ModelController modelControlller)
		{
            ChangeElementMultiplicityMacroCommand c = (ChangeElementMultiplicityMacroCommand)ChangeElementMultiplicityMacroCommandFactory.Factory().Create(modelControlller);
			if (associatedElement != null)
				c.AssociatedElements.Add(associatedElement);
			c.Lower = lower;
			c.Upper = upper;
			c.Element = element;
            c.InitializeCommand();
			if (c.Commands.Count > 0) c.Execute();
		}

        public static bool IsMultiplicityStringValid(string newCardinality)
        {
            uint? lower;
            UnlimitedNatural upper;

            if (newCardinality.Contains(".."))
            {
                int pos = newCardinality.IndexOf("..");
                lower = ParseNullabelUint(newCardinality.Substring(0, pos));
                upper = ParseUnlimitedNatural(newCardinality.Substring(pos + 2));
            }
            else
            {
                lower = uint.Parse(newCardinality);
                upper = (UnlimitedNatural)lower;
            }
            return IsMultiplicityValid(lower, upper); 
        }

        public static bool IsMultiplicityValid(uint? lower, UnlimitedNatural upper)
        {
            return !lower.HasValue || lower <= upper;
        }
	}
}