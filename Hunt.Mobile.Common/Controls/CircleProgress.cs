using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public class CircleProgress : SKCanvasView
	{
		public CircleProgress()
		{
			PaintSurface += OnPaintSurface;

			if(App.Instance.IsDesignMode)
				BackgroundColor = Color.FromHex("#7000");
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
		}

		public static readonly BindableProperty ColorProperty =
			BindableProperty.Create(nameof(Color), typeof(Color), typeof(CircleProgress), Color.White);

		public Color Color
		{
			get { return (Color)GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}

		public static readonly BindableProperty PercentageCompleteProperty =
			BindableProperty.Create(nameof(PercentageComplete), typeof(int), typeof(CircleProgress), 0);

		protected override void OnPropertyChanged(string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if(propertyName == nameof(PercentageComplete))
			{
				InvalidateSurface();
			}
		}

		public int PercentageComplete
		{
			get { return (int)GetValue(PercentageCompleteProperty); }
			set { SetValue(PercentageCompleteProperty, value); }
		}

		void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			SKImageInfo info = e.Info;
			SKCanvas canvas = e.Surface.Canvas;

			canvas.Clear();

			float startAngle = -90;
			float sweepAngle = 360 * (float)(PercentageComplete * .01);

			var arcPaint = new SKPaint();
			arcPaint.IsStroke = true;
			arcPaint.StrokeCap = SKStrokeCap.Round;
			arcPaint.StrokeWidth = 8;

			var padding = 10;
			var paint = new SKPaint();
			paint.IsStroke = true;
			paint.StrokeWidth = 3;
			paint.IsAntialias = true;
			paint.ColorFilter = SKColorFilter.CreateBlendMode(Color.FromHex("#7FFF").ToSKColor(), SKBlendMode.SrcIn);
			canvas.DrawCircle(info.Width / 2, info.Height / 2, info.Height / 2 - padding, paint);

			using(SKPath path = new SKPath())
			{
				var rect = new SKRect(padding, padding, info.Width - padding, info.Height - padding);
				arcPaint.ColorFilter = SKColorFilter.CreateBlendMode(Color.ToSKColor(), SKBlendMode.SrcIn);
				arcPaint.IsAntialias = true;
				path.AddArc(rect, startAngle, sweepAngle);
				canvas.DrawPath(path, arcPaint);
			}
		}
	}
}
