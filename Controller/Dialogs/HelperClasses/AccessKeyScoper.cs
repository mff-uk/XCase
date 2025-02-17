using System.Windows;
using System.Windows.Input;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Class used to manage generic scoping of access keys
	///	The class is described in this article: http://neilmosafi.blogspot.com/2007/04/default-buttons-in-wpf-and-multiple.html .
	/// </summary>
	public static class AccessKeyScoper
	{
		/// <summary>
		///    Identifies the IsAccessKeyScope attached dependency property
		/// </summary>
		public static readonly DependencyProperty IsAccessKeyScopeProperty =
			DependencyProperty.RegisterAttached("IsAccessKeyScope", typeof(bool), typeof(AccessKeyScoper), new PropertyMetadata(false, HandleIsAccessKeyScopePropertyChanged));

		/// <summary>
		///    Sets the IsAccessKeyScope attached property value for the specified object
		/// </summary>
		/// <param name="obj">The object to retrieve the value for</param>
		/// <param name="value">Whether the object is an access key scope</param>
		public static void SetIsAccessKeyScope(DependencyObject obj, bool value)
		{
			obj.SetValue(AccessKeyScoper.IsAccessKeyScopeProperty, value);
		}

		/// <summary>
		///    Gets the value of the IsAccessKeyScope attached property for the specified object
		/// </summary>
		/// <param name="obj">The object to retrieve the value for</param>
		/// <returns>The value of IsAccessKeyScope attached property for the specified object</returns>
		public static bool GetIsAccessKeyScope(DependencyObject obj)
		{
			return (bool)obj.GetValue(AccessKeyScoper.IsAccessKeyScopeProperty);
		}

		private static void HandleIsAccessKeyScopePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue.Equals(true))
			{
				AccessKeyManager.AddAccessKeyPressedHandler(d, HandleScopedElementAccessKeyPressed);
			}
			else
			{
				AccessKeyManager.RemoveAccessKeyPressedHandler(d, HandleScopedElementAccessKeyPressed);
			}
		}

		private static void HandleScopedElementAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
		{
			if (!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt) && GetIsAccessKeyScope((DependencyObject)sender))
			{
				e.Scope = sender;
				e.Handled = true;
			}
		}
	}
}