using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Hunt.Mobile.Common.HuntViewCell), typeof(Hunt.Mobile.iOS.HuntViewCellRenderer))]

namespace Hunt.Mobile.iOS
{
	[Preserve(AllMembers = true)]
	public class HuntViewCellRenderer : ViewCellRenderer
	{
		public override UIKit.UITableViewCell GetCell(Cell item, UIKit.UITableViewCell reusableCell, UIKit.UITableView tv)
		{
			var cell = base.GetCell(item, reusableCell, tv);
			cell.SelectionStyle = UIKit.UITableViewCellSelectionStyle.None;
			return cell;
		}
	}
}