using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections;
using XCase.Controller;
using XCase.Model;
using XCase.View.Controls;
using System.Text.RegularExpressions;
using XCase.Controller.Dialogs;

namespace XCase.Gui
{
    /// <summary>
    /// AssociationEndsGrid displays properties of AssociationEnds collection of a PIM association
    /// </summary>
    public partial class AssociationEndsGrid
    {
        ChangeLowerConverter changeLower = null;
        ChangeUpperConverter changeUpper = null;
        ChangeRoleConverter changeRole = null;

        public List<TextBox> endBoxes = new List<TextBox>();

        /// <summary>
        /// Initialization
        /// </summary>
        public AssociationEndsGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Saves all unsaved values in the grid
        /// </summary>
        public override void UpdateContent()
        {
            try
            {
                foreach (TextBox t in endBoxes)
                {
                    if (t.Name.Equals("lowerBox") && t.IsFocused && changeLower != null)
                    {
                        CheckCorrectMultiplicity(t, null);
                        BindingExpression be = t.GetBindingExpression(TextBox.TextProperty);
                        be.UpdateSource();
                    }
                    else if (t.Name.Equals("upperBox") && t.IsFocused && changeUpper != null)
                    {
                        CheckCorrectMultiplicity(t, null);
                        BindingExpression be = t.GetBindingExpression(TextBox.TextProperty);
                        be.UpdateSource();
                    }
                    else if (t.Name.Equals("roleBox") && t.IsFocused && changeRole != null)
                    {
                        BindingExpression be = t.GetBindingExpression(TextBox.TextProperty);
                        be.UpdateSource();
                    }
                }
            }
            catch
            {
                
            }

            changeLower = null;
            changeUpper = null;
            changeRole = null;

            endsBox.DataContext = null;
            
            endBoxes.Clear();
        }

        /// <summary>
        /// Displays AssociationEnds
        /// </summary>
        /// <param name="ends"></param>
        /// <param name="mc"></param>
        public void Display(IList<AssociationEnd> ends, ModelController mc)
        {
            IDictionaryEnumerator ie = stackPanel.Resources.GetEnumerator();
            while (ie.MoveNext())
            {
                if (ie.Key.ToString() == "changeLowerConverter")
                {
                    changeLower = (ChangeLowerConverter)ie.Value;
                    changeLower.modelController = mc;
                }
                else
                    if (ie.Key.ToString() == "changeUpperConverter")
                    {
                        changeUpper = (ChangeUpperConverter)ie.Value;
                        changeUpper.modelController = mc;
                    }
                    else
                        if (ie.Key.ToString() == "changeRoleConverter")
                        {
                            changeRole = (ChangeRoleConverter)ie.Value;
                            changeRole.modelController = mc;
                        }
            }

            endsBox.DataContext = ends;
        }

        /// <summary>
        /// Checks whether the new multiplicity is a correct one
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckCorrectMultiplicity(object sender, RoutedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            AssociationEnd oldEnd = (AssociationEnd)t.DataContext;

            NUml.Uml2.UnlimitedNatural oldUpper = oldEnd.Upper;
            uint? oldLower = oldEnd.Lower;

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
            ((TextBox)sender).DataContext = oldEnd;
            return;

        }

        #region Event handlers

        new private void KeyPressed_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left)
                e.Handled = true;

            if (e.Key == Key.Enter && ((Control)sender).Focusable)
            {
                if (((TextBox)sender).IsReadOnly)
                    return;

                if (((TextBox)sender).Name.Equals("lowerBox") || ((TextBox)sender).Name.Equals("upperBox"))
                    CheckCorrectMultiplicity(sender, e);

                BindingExpression be = ((Control)sender).GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();

                e.Handled = true;
            }

        }

        new protected void KeyPressed_Up(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up && !((TextBox)sender).Name.Equals("roleBox"))
            {
                ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            }
            else
                if (e.Key == Key.Down && !((TextBox)sender).Name.Equals("upperBox"))
                {
                    ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            e.Handled = true;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!endBoxes.Contains((TextBox)sender))
                endBoxes.Add((TextBox)sender);

            if (changeLower != null)
                changeLower.selectedAssociationEnd = (AssociationEnd)((TextBox)sender).DataContext;

            if (changeUpper != null)
                changeUpper.selectedAssociationEnd = (AssociationEnd)((TextBox)sender).DataContext;

            if (changeRole != null)
                changeRole.selectedAssociationEnd = (AssociationEnd)((TextBox)sender).DataContext;
        }

        private void TextBox_Initialized(object sender, EventArgs e)
        {
            if (!endBoxes.Contains((TextBox)sender))
                endBoxes.Add((TextBox)sender);
        }

        private void TextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!endBoxes.Contains((TextBox)sender))
            {
                endBoxes.Add((TextBox)sender);

                if (((TextBox)sender).Name.Equals("classBox"))
                {
                    object oldContext = ((TextBox)sender).DataContext;
                    ((TextBox)sender).DataContext = null;
                    ((TextBox)sender).DataContext = oldContext;
                }
            }
        }

        #endregion
    }
}
