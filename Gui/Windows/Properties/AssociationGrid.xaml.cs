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

namespace XCase.Gui
{
    /// <summary>
    /// AssociationGrid displays properties of PIM association selected on the canvas
    /// </summary>
    public partial class AssociationGrid
    {   
        RenameAssociationConverter renameAssociation = null;
        
        /// <summary>
        /// Initialization
        /// </summary>
        public AssociationGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Saves all unsaved values in the grid
        /// </summary>
        public override void UpdateContent() 
        {
            if (associationNameBox.IsFocused)
            {
                BindingExpression be = associationNameBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }

            renameAssociation = null;

            ends.UpdateContent();
        }

        /// <summary>
        /// Displays selected PIM Association
        /// </summary>
        /// <param name="a"></param>
        public void Display(PIM_Association a)
        {
            ends.Display(a.Association.Ends, a.Controller.DiagramController.ModelController);

            IDictionaryEnumerator ie = grid.Resources.GetEnumerator();
            while (ie.MoveNext())
            {
                if (ie.Key.ToString() == "renameAssociation")
                    renameAssociation = (RenameAssociationConverter)ie.Value;
            }

            associationNameBox.DataContext = a.Association;
            renameAssociation.associationController = a.Controller;
        }

        #region Event handlers

        private void associationNameBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ends.endsBox.Width = e.NewSize.Width + 64;
            foreach (TextBox t in ends.endBoxes)
            {
                t.Width = e.NewSize.Width;
            }
        }

        new private void KeyPressed_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left)
                e.Handled = true;

            if (e.Key == Key.Enter && ((Control)sender).Focusable)
            { 
                BindingExpression be = associationNameBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
                e.Handled = true; 
            }

        }

        private void AssociationGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 140)
            {
                associationNameBox.Width = e.NewSize.Width - 70;
                e.Handled = true;
            }
        }

        #endregion
    }
}
