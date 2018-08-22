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
    /// CommentGrid displays properties of a comment selected on the canvas
    /// </summary>
    public partial class CommentGrid : XCaseGridBase
    {        
        private ChangeCommentConverter changeComment = null;
        
        /// <summary>
        /// Initialization
        /// </summary>
        public CommentGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Saves all unsaved values in the grid
        /// </summary>
        public override void UpdateContent()
        {
            if (commentText.IsFocused)
            {
                BindingExpression be = commentText.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
            changeComment = null;

            appearance.UpdateContent();
        }

        /// <summary>
        /// Displays comment in the Properties window
        /// </summary>
        /// <param name="c"></param>
        public void Display(XCaseComment c)
        {
            IDictionaryEnumerator ie = grid.Resources.GetEnumerator();
            while (ie.MoveNext())
            {
                if (ie.Key.ToString() == "changeComment")
                    changeComment = (ChangeCommentConverter)ie.Value;
            }

            changeComment.SelectedComment = c;
            commentText.DataContext = c.ModelComment;

            appearance.SetElement(c.Controller.DiagramController, c.ViewHelper);
        }

        #region Event handlers

        private void element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            appearance.ExtendWidth(e.NewSize.Width-62);
        }

        private void XCaseGridBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 140)
            {
                commentText.Width = e.NewSize.Width-10;
                e.Handled = true;
            }
        }

        new private void KeyPressed_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    // new line
                    TextBox t = ((TextBox)sender);
                    int oldCaretIndex = t.CaretIndex;
                    t.Text = t.Text.Insert(oldCaretIndex, "\r\n");
                    t.CaretIndex = oldCaretIndex + 1;
                }
                else
                {
                    // confirm entered value
                    BindingExpression be = ((Control)sender).GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                }

                e.Handled = true;
            }
        }

        #endregion
    }

}

