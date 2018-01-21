using System;
using System.Windows.Input;
using Hunt.Common;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public partial class AssignAttributesPage : BaseContentPage<AddGenericTreasureViewModel>
	{
		public AssignAttributesPage()
		{
			Initialize();
		}

		public AssignAttributesPage(AddGenericTreasureViewModel viewModel)
		{
			ViewModel = viewModel;
			Initialize();
		}

		void Initialize()
		{
			if(IsDesignMode)
				ViewModel.SetGame(App.Instance.CurrentGame);

			InitializeComponent();

			if(ViewModel.AvailableAttributes != null)
				DrawAttributeCells();
		}

		#region DrawAttributeCells

		void DrawAttributeCells()
		{
			var rootGrid = new Grid
			{
				RowSpacing = 8,
				ColumnSpacing = 8,
			};

			rootGrid.ColumnDefinitions.Add(new ColumnDefinition());
			rootGrid.ColumnDefinitions.Add(new ColumnDefinition());

			var colorIndex = 0;
			var itemIndex = 0;
			var rowIndex = rootGrid.RowDefinitions.Count;

			foreach(var attribute in ViewModel.AvailableAttributes)
			{
				if(itemIndex % 2 == 0)
					rootGrid.RowDefinitions.Add(new RowDefinition { Height = 80 });

				var isLeft = itemIndex % 2 == 0;

				var grid = new Grid
				{
					VerticalOptions = LayoutOptions.FillAndExpand,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Margin = isLeft ? new Thickness(6, 0, 0, 0) : new Thickness(0, 0, 6, 0),
				};
				Grid.SetRow(grid, rowIndex);
				Grid.SetColumn(grid, itemIndex % 2);

				var button = new Button
				{
					Text = attribute,
					FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
					FontAttributes = FontAttributes.Bold,
					BackgroundColor = Color.FromHex("#03FFFFFF"),
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill,
					Margin = new Thickness(0),
					AutomationId = $"tag{itemIndex}",
				};

				button.Clicked += AttributeButtonClicked;
				grid.Children.Add(button);
				rootGrid.Children.Add(grid);

				if(itemIndex % 2 == 1)
					rowIndex++;

				itemIndex++;
				colorIndex++;
			}

			rootView.Children.Add(rootGrid);
		}

		#endregion

		void AttributeButtonClicked(object sender, EventArgs e)
		{
			var btn = sender as Button;

			if(btn.CommandParameter == null || !(bool)btn.CommandParameter)
			{
				if(ViewModel.SelectedAttributes.Count >= 2)
				{
					Hud.Instance.ShowToast("Please unselect a tag first.");
					return;
				}

				btn.CommandParameter = true;
				btn.BackgroundColor = Color.FromHex("#1FFF");
				btn.FontAttributes = FontAttributes.Bold;
				ViewModel.AddAttribute(btn.Text);
			}
			else
			{
				btn.CommandParameter = false;
				btn.BackgroundColor = Color.FromHex("#03FFFFFF");
				btn.FontAttributes = FontAttributes.None;
				ViewModel.RemoveAttribute(btn.Text);
			}
		}

		async void SaveClicked(object sender, EventArgs e)
		{
			var success = await ViewModel.SaveTreasure();

			if(!success)
				return;

            await Navigation.PopModalAsync();
		}
	}
}