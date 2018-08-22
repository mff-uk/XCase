using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using XCase.Controller;
using XCase.Model;
using XCase.View.Controls;
using XCase.Controller.Dialogs;

namespace XCase.Gui
{
    /// <summary>
    /// PSMAssociationGrid displays properties of PSM association selected on the canvas
    /// </summary>
    public partial class PSMAssociationGrid
    {
        ChangePSMLowerConverter changeLower = null;
        ChangePSMUpperConverter changeUpper = null;

        /// <summary>
        /// Initialization
        /// </summary>
        public PSMAssociationGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Saves all unsaved values in the grid
        /// </summary>
        public override void UpdateContent()
        {
            if (upperBox.IsFocused && changeUpper != null)
            {
                CheckCorrectMultiplicity(upperBox, null);
                BindingExpression be = upperBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
            else
                if (lowerBox.IsFocused && changeLower != null)
                {
                    CheckCorrectMultiplicity(lowerBox, null);
                    BindingExpression be = lowerBox.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                }
            changeLower = null;
            changeUpper = null;

            njGrid.UpdateContent();
        }

        /// <summary>
        /// Displays selected PSM Association
        /// </summary>
        /// <param name="p"></param>
        public void Display(PSM_Association p)
        {
            
            IDictionaryEnumerator ie = grid.Resources.GetEnumerator();
            while (ie.MoveNext())
            {
                if (ie.Key.ToString() == "changePSMLowerConverter")
                {
                    changeLower = (ChangePSMLowerConverter)ie.Value;
                    changeLower.controller = p.Controller;
                }
                else
                    if (ie.Key.ToString() == "changePSMUpperConverter")
                    {
                        changeUpper = (ChangePSMUpperConverter)ie.Value;
                        changeUpper.controller = p.Controller;
                    }
            }
            
            lowerBox.DataContext = p.Association;
            upperBox.DataContext = p.Association;

            njGrid.njsBox.DataContext = p.Association.NestingJoins;
        }

        #region Input validation

        /// <summary>
        /// Checks correct PSM Association multiplicity after the change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckCorrectMultiplicity(object sender, RoutedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            PSMAssociation oldAssoc = (PSMAssociation)t.DataContext;

            NUml.Uml2.UnlimitedNatural oldUpper = oldAssoc.Upper;
            uint? oldLower = oldAssoc.Lower;

            Regex numbers = new Regex("^[0-9]+$");

            Match m = numbers.Match(((TextBox)sender).Text);
            if (m.Success) // correct numerical imput
            {
                if (t.Name.Equals("upperBox") && int.Parse(t.Text) >= oldLower)
                    return;
                else
                    if (t.Name.Equals("lowerBox"))
                    {
                        if (oldUpper.ToString().Equals("*") || int.Parse(t.Text) <= oldUpper.Value)
                            return;
                    }
            }
            else
                if (t.Text.Equals("*") && t.Name.Equals("upperBox"))
                    return;

            // New value refused

            ErrDialog ed = new ErrDialog();
            ed.SetText("Change element multiplicity\n", "Lower bound cannot be greater than upper bound");
            ed.Show();

            ((TextBox)sender).DataContext = null;
            ((TextBox)sender).DataContext = oldAssoc;
            return;

        }

        #endregion

        #region Event handlers

        new private void KeyPressed_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left)
                e.Handled = true;

            if (e.Key == Key.Enter && ((Control)sender).Focusable)
            {
                if (((TextBox)sender).Name.Equals("lowerBox") || ((TextBox)sender).Name.Equals("upperBox"))
                    CheckCorrectMultiplicity(sender, e);

                BindingExpression be = ((Control)sender).GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();

                e.Handled = true;
            }

        }

        private void ClassGridBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 140)
            {
                lowerBox.Width = e.NewSize.Width - 70;
                upperBox.Width = e.NewSize.Width - 70;
                njGrid.Width = e.NewSize.Width - 8;
                e.Handled = true;
            }
            e.Handled = true;
        }

        #endregion
    }
}
