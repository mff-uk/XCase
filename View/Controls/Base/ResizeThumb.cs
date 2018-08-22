using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
	/// <summary>
	/// ResizeThumb is a subclass of <see cref="Thumb"/> control used to resize
	/// another control (reference to associated control is in <see cref="belongsTo"/>).
	/// When multiple resizeable(see <see cref="IResizable"/> elements are selected,
	/// all elements are resized together). 
	/// </summary>
	/// <seealso cref="ResizeElementCommand"/>
	/// <seealso cref="IResizable"/>
    public class ResizeThumb : Thumb
    {
		/// <summary>
		/// Control that is resized by this ResizedThumb
		/// </summary>
        public Control belongsTo;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResizeThumb"/> class.
		/// </summary>
        public ResizeThumb()
        {
            DragDelta += ResizeThumb_DragDelta;
			DragCompleted += ResizeThumb_DragCompleted;
			DragStarted += ResizeThumb_DragStarted;
        }

    	private Dictionary<Control, Size> startSizes;

		/// <summary>
		/// Initializes the resizing process, stores old sizes of all the selected resizable elements. 
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragStartedEventArgs"/> instance containing the event data.</param>
		void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
		{
			Control designerItem = this.belongsTo;
			XCaseCanvas designer = VisualTreeHelper.GetParent(designerItem) as XCaseCanvas;

			startSizes = new Dictionary<Control, Size>();
			foreach (Control item in (designer.SelectedItems).OfType<IResizable>())
			{
				startSizes[item] = new Size(item.Width, item.Height);
			}
		}

		/// <summary>
		/// Finalizes the resizing process, issues the resize command (see <see cref="ResizeElementCommand"/>). 
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragCompletedEventArgs"/> instance containing the event data.</param>
		void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			Control designerItem = this.belongsTo;
			XCaseCanvas designer = VisualTreeHelper.GetParent(designerItem) as XCaseCanvas;

			if (designer != null)
			{
				MacroCommand<DiagramController> macroResize = MacroCommandFactory<DiagramController>.Factory().Create(designer.Controller);
				macroResize.Description = CommandDescription.RESIZE_MACRO;
				// generate move Commands for all resized elements and execute them as macro
				foreach (Control item in designer.SelectedItems.OfType<IResizable>())
				{
					item.Width = startSizes[item].Width;
					item.Height = startSizes[item].Height;
					
					CommandBase command =
						ViewController.CreateResizeCommand(item.ActualWidth, item.ActualHeight, ((IResizable)item).ViewHelper,
						                                   designer.Controller);
					macroResize.Commands.Add(command);
				}
				macroResize.Execute();
			}
		}

		/// <summary>
		/// Proceeds with resizing, checks the bounds. 
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
    	void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Control designerItem = this.belongsTo;
            XCaseCanvas designer = VisualTreeHelper.GetParent(designerItem) as XCaseCanvas;

            if (designerItem != null && designer != null)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;
                double minDeltaHorizontal = double.MaxValue;
                double minDeltaVertical = double.MaxValue;

                // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
                // calculate min value for each parameter for each item
				foreach (Control item in designer.SelectedItems.OfType<IResizable>())
                {
                    double left = Canvas.GetLeft(item);
                    double top = Canvas.GetTop(item);

                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

                    minDeltaVertical = Math.Min(minDeltaVertical, item.ActualHeight - item.MinHeight);
                    minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.ActualWidth - item.MinWidth);
                }

                double dragDeltaVertical, dragDeltaHorizontal;
				foreach (Control item in designer.SelectedItems.OfType<IResizable>())
                {
                    
                    switch (VerticalAlignment)
                    {
                        case VerticalAlignment.Bottom:
                            dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                            item.Height = item.ActualHeight - dragDeltaVertical;
                            break;
                        case VerticalAlignment.Top:
                            double top = Canvas.GetTop(item);
                            dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                            Canvas.SetTop(item, top + dragDeltaVertical);
                            item.Height = item.ActualHeight - dragDeltaVertical;
                            break;
                        default:
                            break;
                    }

                    switch (HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            double left = Canvas.GetLeft(item);
                            dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                            Canvas.SetLeft(item, left + dragDeltaHorizontal);
                            item.Width = item.ActualWidth - dragDeltaHorizontal;
                            break;
                        case HorizontalAlignment.Right:
                            dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                            item.Width = item.ActualWidth - dragDeltaHorizontal;
                            break;
                        default:
                            break;
                    }
                    
                }
                e.Handled = true;
            }
        }
    }
}
