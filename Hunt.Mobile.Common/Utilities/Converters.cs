using System;
using Xamarin.Forms;
using System.Globalization;
using System.Collections;
using System.IO;

//All these converters create a single instance that can be reused for all
//bindings and avoids the need to create a dedicated Resource in XAML
namespace Hunt.Mobile.Common
{
	public class NullIntValueConverter : IValueConverter
	{
		public static NullIntValueConverter Instance = new NullIntValueConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null ? string.Empty : value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null || !(value is string))
				return null;

			int i;
			if(int.TryParse((string)value, out i))
				return i;

			return null;
		}
	}

	public class InverseBoolConverter : IValueConverter
	{
		public static InverseBoolConverter Instance = new InverseBoolConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}
	}

	public class IsNotNullToBoolConverter : IValueConverter
	{
		public static IsNotNullToBoolConverter Instance = new IsNotNullToBoolConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}
	}

	public class BoolToOpacityConverter : IValueConverter
	{
		public static BoolToOpacityConverter Instance = new BoolToOpacityConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? 1 : 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null;
		}
	}

	public class IsNullToBoolConverter : IValueConverter
	{
		public static IsNullToBoolConverter Instance = new IsNullToBoolConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null;
		}
	}

	public class IsEmptyToHeightConverter : IValueConverter
	{
		public static IsEmptyToHeightConverter Instance = new IsEmptyToHeightConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var list = value as IList;

			if(list == null || list.Count == 0)
				return 200;

			return 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return false;
		}
	}

	public class IsEmptyConverter : IValueConverter
	{
		public static IsEmptyConverter Instance = new IsEmptyConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var list = value as IList;

			if(list == null)
				return true;

			return list.Count == 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return false;
		}
	}

	public class IsNotEmptyConverter : IValueConverter
	{
		public static IsNotEmptyConverter Instance = new IsNotEmptyConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var list = value as IList;

			if(list == null)
				return false;

			return list.Count > 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return false;
		}
	}

	public class BytesToImageSourceConverter : IValueConverter
	{
		public static BytesToImageSourceConverter Instance = new BytesToImageSourceConverter();

		/// <param name="value">To be added.</param>
		/// <param name="targetType">To be added.</param>
		/// <param name="parameter">To be added.</param>
		/// <param name="culture">To be added.</param>
		/// <summary>
		/// Convert the specified value, targetType, parameter and culture.
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			byte[] bytes = value as byte[];

			if(bytes == null)
			{
				return ImageSource.FromFile("default.png");
			}
			return ImageSource.FromStream(() => new MemoryStream(bytes));
		}

		/// <param name="value">To be added.</param>
		/// <param name="targetType">To be added.</param>
		/// <param name="parameter">To be added.</param>
		/// <param name="culture">To be added.</param>
		/// <summary>
		/// Converts the back.
		/// </summary>
		/// <returns>The back.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}