using System;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public partial class NavigationBar : ContentView
	{
		public event EventHandler NextClicked;

		#region Properties

		public static readonly BindableProperty TitleProperty =
			BindableProperty.Create(nameof(Title), typeof(string), typeof(NavigationBar), string.Empty);

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly BindableProperty CanMoveBackProperty =
			BindableProperty.Create(nameof(CanMoveBack), typeof(bool), typeof(NavigationBar), false);

		public bool CanMoveBack
		{
			get { return (bool)GetValue(CanMoveBackProperty); }
			set { SetValue(CanMoveBackProperty, value); }
		}

		public static readonly BindableProperty CanCloseProperty =
			BindableProperty.Create(nameof(CanClose), typeof(bool), typeof(NavigationBar), false);

		public bool CanClose
		{
			get { return (bool)GetValue(CanCloseProperty); }
			set { SetValue(CanCloseProperty, value); }
		}

		public static int YOffset
		{
			get
			{
				return Device.RuntimePlatform == Device.Android ? 60 : 80;
			}
		}

		View _leftToolbar;
		public View LeftToolbar
		{
			get
			{
				return _leftToolbar;
			}
			set
			{
				if(value == null && leftToolbarView != null)
				{
					leftToolbarView.Content = null;
				}
				else
				{
					if(_leftToolbar != value)
					{
						_leftToolbar = value;

						if(leftToolbarView != null)
							leftToolbarView.Content = value;
					}
				}
			}
		}

		View _rightToolbar;
		public View RightToolbar
		{
			get
			{
				return _rightToolbar;
			}
			set
			{
				if(value == null && rightToolbarView != null)
				{
					rightToolbarView.Content = null;
				}
				else
				{
					if(_rightToolbar != value)
					{
						_rightToolbar = value;

						if(rightToolbarView != null)
							rightToolbarView.Content = value;
					}
				}
			}
		}

		#endregion

		public NavigationBar()
		{
			InitializeComponent();
			root.BindingContext = this;

			if(App.Instance.IsDesignMode)
			{
				CanMoveBack = true;
				CanClose = true;
				Title = "Page Title";
			}
		}

		protected override void OnParentSet()
		{
			base.OnParentSet();

			Parent.BindingContextChanged += (sender, e) =>
			{
				leftToolbarView.BindingContext = Parent.BindingContext;
				rightToolbarView.BindingContext = Parent.BindingContext;
			};
		}

		async void OnCloseClicked(object sender, EventArgs e)
		{
			try { await Navigation.PopModalAsync(); }
			catch(Exception) { }
		}

		async void OnBackClicked(object sender, EventArgs e)
		{
			try
			{
				await Navigation.PopAsyncAndNotify();
			}
			catch(Exception ex)
			{
				Logger.Instance.WriteLine(ex);
			}
		}

		void OnNextClicked(object sender, EventArgs e)
		{
			NextClicked?.Invoke(sender, e);
		}
	}
}