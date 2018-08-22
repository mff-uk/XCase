using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using XCase.View.Interfaces;
using System.Linq;
using System.Windows;

namespace XCase.View.Controls
{
	/// <summary>
	/// Collection of selected items on canvas.
	/// </summary>
	public class SelectedItemsCollection : ObservableCollection<ISelectable>
	{
		/// <summary>
		/// When a control is only a part of another control, this call returns the 
		/// topmost control
		/// </summary>
		/// <param name="item">control</param>
		/// <returns>owner of <see paramref="item"/></returns>
		public static ISelectable GetOwner(ISelectable item)
		{
			XCaseJunction junction = item as XCaseJunction;
			if (junction != null)
			{
				if (junction.SelectionOwner != null)
				{
					if (junction.Association != null && junction.Association.AssociationClass != null)
					{
							return junction.Association.AssociationClass;
					}
					else
					{
						return junction.SelectionOwner;
					}
				}
			}

			return item;
		}

		/// <summary>
		/// Inserts an item into the collection at the specified index. If item only a part of another control,
		/// the owner of the control is inserted. 
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert.</param>
		protected override void InsertItem(int index, ISelectable item)
		{
			ISelectable owner = GetOwner(item);
			if (this.Contains(owner))
			{
				return;
			}
			base.InsertItem(index, owner);	
		}

		bool locked = false;

		/// <summary>
		/// Inserts <paramref name="items"/> to collection and marks them 
		/// as selected. Old selection is lost.
		/// </summary>
		/// <param name="items">items to become selected</param>
		public void SetSelection(IEnumerable<object> items)
		{
			if (!locked)
			{
				try
				{
					locked = true;
					foreach (ISelectable item in this)
						item.IsSelected = false;
					Clear();
					foreach (ISelectable item in items)
					{
						item.IsSelected = true;
						Add(item);
					}
                    if (items.Count() > 0 && items.Any(i => i is FrameworkElement))
                    {
                        ((FrameworkElement)items.First(i => i is FrameworkElement)).BringIntoView();
                    }
				}
				finally
				{
					locked = false;
				}
			}
		}
		
		/// <summary>
		/// Inserts <paramref name="items"/> to collection and marks them 
		/// as selected. Old selection is lost.
		/// </summary>
		/// <param name="items">items to become selected</param>
		public void SetSelection(params ISelectable[] items)
		{
			SetSelection((IEnumerable<ISelectable>)items);
		}
	}
}