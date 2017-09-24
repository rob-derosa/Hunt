using System;
using System.Linq;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public class EntryExtend
	{
		public static readonly BindableProperty MaxLengthProperty =
			BindableProperty.CreateAttached("MaxLength", typeof(long), typeof(Entry), long.MaxValue);

		public static long GetMaxLength(BindableObject view)
		{
			return (long)view.GetValue(MaxLengthProperty);
		}

		public static void SetMaxLength(BindableObject view, long value)
		{
			view.SetValue(MaxLengthProperty, value);
		}

		WeakReference<BindableObject> _bindable;

		public EntryExtend(BindableObject bindable)
		{
			_bindable = new WeakReference<BindableObject>(bindable);
		}

		public BindableObject Bindable
		{
			get
			{
				BindableObject bindable;

				if(_bindable.TryGetTarget(out bindable))
					return bindable;

				return null;
			}
			set
			{
				_bindable.SetTarget(value);
			}
		}

		public long MaxLength
		{
			get { return GetMaxLength(Bindable); }
			set { SetMaxLength(Bindable, value); }
		}
	}
}