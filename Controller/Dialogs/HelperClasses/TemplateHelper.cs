using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Provides some static methods to work with XAML template. 
	/// </summary>
	public static class TemplateHelper
	{
		/// <summary>
		/// Gets the part of data template.
		/// </summary>
		/// <typeparam name="PartType">The type of the part returned.</typeparam>
		/// <param name="obj">The obj where to look for the template part.</param>
		/// <param name="templatePartName">Name of the template part.</param>
		/// <returns>control found in objects template</returns>
		public static PartType GetPartOfDataTemplate<PartType>(DependencyObject obj, string templatePartName)
		{
			ContentPresenter contentPresenter = FindVisualChild<ContentPresenter>(obj);
			return (PartType)contentPresenter.ContentTemplate.FindName(templatePartName, contentPresenter);
		}

		/// <summary>
		/// Finds the visual child of an object of a certain type.
		/// </summary>
		/// <typeparam name="childItem">The type of the child item.</typeparam>
		/// <param name="obj">The obj where to look for child.</param>
		/// <returns></returns>
		public static childItem FindVisualChild<childItem>(DependencyObject obj)
			where childItem : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child != null && child is childItem)
					return (childItem)child;
				else
				{
					childItem childOfChild = FindVisualChild<childItem>(child);
					if (childOfChild != null)
						return childOfChild;
				}
			}
			return null;
		}
	}
}