using XCase.Model;

namespace XCase.Controller.Commands.Helpers
{
	/// <summary>
	/// Element Holder can only hold a Element. This is the same for ViewHelpers.
	/// </summary>
	/// <typeparam name="ViewHelperType"></typeparam>
    public class ViewHelperHolder<ViewHelperType> : HolderBase<ViewHelperType>
		where ViewHelperType : ViewHelper, IHolder
	{

	}
}