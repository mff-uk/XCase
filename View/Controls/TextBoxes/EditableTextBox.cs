using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using XCase.Controller;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
    /// <summary>
    /// TextBox for displaying and editing Class properties and methods
    /// </summary>
    public class EditableTextBox : TextBox 
    {
        Brush originalTextBrush = Brushes.Black;
        public Brush OriginalTextBrush { get { return originalTextBrush; } set { originalTextBrush = value; Foreground = value; } }
        Brush mouseOverBrush = Brushes.Red;
        public Brush MouseOverBrush { get { return mouseOverBrush; } set { mouseOverBrush = value; } }
        Brush originalBackgroundBrush = Brushes.Transparent;
        public Brush OriginalBackgroundBrush { get { return originalBackgroundBrush; } set { originalBackgroundBrush = value; Background = value; } }
        Brush editableBackgroundBrush = Brushes.White;
        public Brush EditableBackgroundBrush { get { return editableBackgroundBrush; } set { editableBackgroundBrush = value; } }

        private XCaseCanvas xCaseCanvas;
        protected XCaseCanvas XCaseCanvas
        {
            get
            {
                if (xCaseCanvas == null)
                {
                    FrameworkElement c = (FrameworkElement)this.Parent;
                    while (!(c is XCaseCanvas) && c.Parent != null)
                        c = (FrameworkElement)c.Parent;

                    if (c is XCaseCanvas)
                        xCaseCanvas = (XCaseCanvas)c;
                    else if (c.TemplatedParent != null)
                    {
                        c = (FrameworkElement) c.TemplatedParent;
                        if (c is IModelElementRepresentant)
                        {
                            xCaseCanvas = ((IModelElementRepresentant) c).XCaseCanvas;
                        }
                    }
                }
                return xCaseCanvas;
            }
        }

        public bool CanBeEmpty = false;

    	private string valueBeforeEdit = null;

        public ContextMenuItem mi_Rename;

		bool myeditable;

        /// <summary>
        /// Switches between editable and not editable mode
        /// </summary>
        public bool myEditable
        {
            get { return myeditable; }
            set
            {
				if (myeditable != value)
				{
					myeditable = value;
					if (value == true)
					{
						valueBeforeEdit = Text;
						IsReadOnly = false;
						Background = EditableBackgroundBrush;
						Focusable = true;
						Cursor = Cursors.IBeam;
						Foreground = OriginalTextBrush;

						Focus();
						Select(0, Text.Length);
					}
					else
					{
						IsReadOnly = true;
						Background = OriginalBackgroundBrush;
						Focusable = false;
						Cursor = Cursors.Arrow;

						if (valueBeforeEdit != null && valueBeforeEdit != Text && TextEdited != null)
						{
							StringEventArgs args = new StringEventArgs() { Data = Text };
							Text = valueBeforeEdit;
							if (CanBeEmpty || args.Data.Length > 0) TextEdited(this, args);
						}
						valueBeforeEdit = null;
					}
				}
            }
        }

    	public event StringEventHandler TextEdited; 

        bool mousein;
        /// <summary>
        /// This is for highlighting textboxes when mouse moves over them
        /// </summary>
        bool MouseIn
        {
            get { return mousein; }
            set
            {
                mousein = value;
                if (mousein)
                {
                    Foreground = MouseOverBrush;
                }
                else
                {
                    Foreground = OriginalTextBrush;
                }
            }
        }

        
        public EditableTextBox()
            : base()
        {
            
            Background = System.Windows.Media.Brushes.Transparent;
            TextAlignment = System.Windows.TextAlignment.Left;
            BorderThickness = new System.Windows.Thickness(0.0);
            IsReadOnly = true;
            Focusable = false;
            IsTabStop = false;
            IsHitTestVisible = true;
            Cursor = System.Windows.Input.Cursors.Arrow;
            Margin = new System.Windows.Thickness(5,0,5,0);
            KeyDown += new System.Windows.Input.KeyEventHandler(ClassTextBox_KeyDown);
            KeyUp += new KeyEventHandler(EditableTextBox_KeyUp);
            LostFocus += new System.Windows.RoutedEventHandler(ClassTextBox_LostFocus);
            LostKeyboardFocus += new KeyboardFocusChangedEventHandler(ClassTextBox_LostKeyboardFocus);
            MouseEnter += new MouseEventHandler(ClassTextBox_MouseEnter);
            MouseLeave += new MouseEventHandler(ClassTextBox_MouseLeave);
            ContextMenuOpening += new ContextMenuEventHandler(ClassTextBox_ContextMenuOpening);
            ContextMenuClosing += new ContextMenuEventHandler(ClassTextBox_ContextMenuClosing);
        	FocusVisualStyle = null;
            ResetContextMenu();
        }

        public void ResetContextMenu()
        {
            ContextMenu = new ContextMenu();
            mi_Rename = new ContextMenuItem("Rename");
            mi_Rename.Icon = ContextMenuIcon.GetContextIcon("pencil");
            mi_Rename.Click += new System.Windows.RoutedEventHandler(mi_Click);
            ContextMenu.Items.Add(mi_Rename);
        }
        
        void EditableTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && myEditable) e.Handled = true;
        }

        void ClassTextBox_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            MouseIn = false;
        }

        void ClassTextBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            mousein = false;
        }

        void ClassTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseIn = false;
            e.Handled = false;
        }

        void ClassTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!myEditable) MouseIn = true;
            e.Handled = false;
        }

        void ClassTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            myEditable = false;
        }

        void ClassTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            myEditable = false;
        }

        void ClassTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                myEditable = false;
            	this.Focus();
            	UIElement parent = this.Parent as UIElement;
            	if (parent != null)
					parent.Focus();
            }
            if (e.Key == Key.Delete) e.Handled = true;
        }

        void mi_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            myEditable = true;
        }
    }

	
}
