using System.ComponentModel;
using Hunt.Mobile.Common;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Entry), typeof(Hunt.Mobile.iOS.EntryExtendRenderer))]

namespace Hunt.Mobile.iOS
{
	[Preserve(AllMembers = true)]
	public class EntryExtendRenderer : EntryRenderer
	{
		EntryExtend _properties;

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			_properties = (e.NewElement != null) ? new EntryExtend(e.NewElement) : null;

			if(Control != null && _properties != null)
			{
				Control.BorderStyle = UIKit.UITextBorderStyle.None;
				UpdateMaxLength(EntryExtend.GetMaxLength(Element));
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(_properties != null)
			{
				if(e.PropertyName == EntryExtend.MaxLengthProperty.PropertyName)
				{
					UpdateMaxLength(EntryExtend.GetMaxLength(Element));
				}
			}

			base.OnElementPropertyChanged(sender, e);
		}

		void UpdateMaxLength(long maxLength)
		{
			Control.ShouldChangeCharacters = (textField, range, replacementString) =>
			{
				var newLength = textField.Text.Length + replacementString.Length - range.Length;
				return newLength <= maxLength;
			};
		}
	}
}