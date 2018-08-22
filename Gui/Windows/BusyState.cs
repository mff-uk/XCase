using System.Windows.Input;

namespace XCase.Gui
{
	public static class BusyState
	{
		public static bool IsBusy { get; private set; }

		public static void SetBusy()
		{
			IsBusy = true;
			Mouse.SetCursor(Cursors.Wait);
		}

		public static void SetNormalState()
		{
			IsBusy = false;
			Mouse.SetCursor(Cursors.Arrow);
		}
	}
}