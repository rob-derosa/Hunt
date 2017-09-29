using System;
using System.IO;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	/// <summary>
	/// Uses SVG instead of bitmap data to display an image
	/// All SCG files must be embedded as a resource in the /Resources folder
	/// </summary>
	public class SvgImage : SKCanvasView
	{
		string _fileContent;

		public SvgImage()
		{
			PaintSurface += OnPaintSurface;
			HeightRequest = 24;
			WidthRequest = 24;

			if (App.Instance.IsDesignMode)
				BackgroundColor = Color.FromHex("#7000");

#pragma warning disable CS0618 // Type or member is obsolete
			GestureRecognizers.Add(new TapGestureRecognizer((obj) => Clicked?.Invoke(this, new EventArgs())));
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public event EventHandler<EventArgs> Clicked;

		public static readonly BindableProperty ColorProperty =
			BindableProperty.Create(nameof(Color), typeof(Color), typeof(SvgImage), Color.White);

		public Color Color
		{
			get { return (Color)GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}

		public static readonly BindableProperty SourceProperty =
			BindableProperty.Create(nameof(Source), typeof(string), typeof(SvgImage), null);

		public string Source
		{
			get { return (string)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			if(string.IsNullOrEmpty(Source))
				return;

			try
			{
				if(_fileContent == null)
					_fileContent = Source.GetFileContents();

				var svg = new SkiaSharp.Extended.Svg.SKSvg();
				var bytes = System.Text.Encoding.UTF8.GetBytes(_fileContent);
				var stream = new MemoryStream(bytes);

				svg.Load(stream);
				var canvas = e.Surface.Canvas;
				using(var paint = new SKPaint())
				{
					if(Color != Color.Lime)
					{
						//Set the paint color
						paint.ColorFilter = SKColorFilter.CreateBlendMode(Color.ToSKColor(), SKBlendMode.SrcIn);
					}

					//Scale up the SVG image to fill the canvas
					float canvasMin = Math.Min(e.Info.Width, e.Info.Height);
					float svgMax = Math.Max(svg.Picture.CullRect.Width, svg.Picture.CullRect.Height);
					float scale = canvasMin / svgMax;
					var matrix = SKMatrix.MakeScale(scale, scale);

					canvas.Clear(Color.Transparent.ToSKColor());
					canvas.DrawPicture(svg.Picture, ref matrix, paint);
				}
			}
			catch(Exception ex)
			{
				Logger.Instance.WriteLine($"Error drawing SvgImage w/ ImagePath {Source}: {ex}");
			}
		}
	}
}
