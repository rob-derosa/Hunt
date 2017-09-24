using System;
using System.Windows.Input;
using Hunt.Common;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public partial class TeamListPage : BaseContentPage<TeamListViewModel>
	{
		ICommand ChooseTeamCommand
		{
			get { return new Command(HandleChooseTeam); }
		}

		public TeamListPage()
		{
			Initialize();
		}

		public TeamListPage(Game game)
		{
			ViewModel.Game = game;
			ViewModel.RefreshedGame += (sender, e) => Device.BeginInvokeOnMainThread(DrawTeamCells);
			Initialize();

			if(!game.IsCoordinator() && game.GetTeam() == null)
			{
				navBar.Title = "Choose Team";
			}
			else
			{
				navBar.Title = "Teams";
			}
		}

		void Initialize()
		{
			if(IsDesignMode)
				ViewModel.Game = App.Instance.CurrentGame;

			InitializeComponent();

			if(ViewModel.Game != null)
				DrawTeamCells();
		}

		#region DrawTeamCells

		void DrawTeamCells()
		{
			var rootGrid = new Grid
			{
				RowSpacing = 8,
				ColumnSpacing = 8,
			};

			rootGrid.ColumnDefinitions.Add(new ColumnDefinition());
			rootGrid.ColumnDefinitions.Add(new ColumnDefinition());

			var colors = new string[] {
					"aqua",
					"orange",
					"darkPurple",
					"blue",
					"magenta",
					"lightPurple",
					"gray",
					"fucia",
					"pink",
				};

			var colorIndex = 0;
			var itemIndex = 0;
			var rowIndex = rootGrid.RowDefinitions.Count;

			foreach(var team in ViewModel.Game.Teams)
			{
				if(itemIndex % 2 == 0)
					rootGrid.RowDefinitions.Add(new RowDefinition { Height = 160 });

				if(colorIndex >= colors.Length)
					colorIndex = 0;

				var isLeft = itemIndex % 2 == 0;
				var bgColor = Color.FromHex("#1FFF");

				if(ViewModel.Game.IsPrepping && team.Players.Count >= ViewModel.Game.PlayerCountPerTeam)
					bgColor = Color.FromHex("#04FFFFFF");

				var grid = new Grid
				{
					VerticalOptions = LayoutOptions.FillAndExpand,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					BackgroundColor = bgColor,
					Margin = isLeft ? new Thickness(6, 0, 0, 0) : new Thickness(0, 0, 6, 0),
				};
				Grid.SetRow(grid, rowIndex);
				Grid.SetColumn(grid, itemIndex % 2);

				var bar = new ContentView
				{
					WidthRequest = 6,
					BackgroundColor = (Color)Application.Current.Resources[colors[colorIndex]],
					VerticalOptions = LayoutOptions.FillAndExpand,
					HorizontalOptions = isLeft ? LayoutOptions.Start : LayoutOptions.End,
				};

				var teamNameLabel = new Label
				{
					Text = team.Name,
					Margin = new Thickness(30, -20, 30, 0),
					FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
					LineBreakMode = LineBreakMode.WordWrap,
					TextColor = Color.White,
					FontAttributes = FontAttributes.Bold,
					HorizontalTextAlignment = TextAlignment.Center,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
				};

				var avatarStack = new StackLayout
				{
					Spacing = 10,
					Margin = new Thickness(0, 0, 0, 10),
					VerticalOptions = LayoutOptions.End,
					HorizontalOptions = LayoutOptions.Center,
					Orientation = StackOrientation.Horizontal,
				};

				var size = 26;
				for(int i = 0; i < ViewModel.Game.PlayerCountPerTeam; i++)
				{
					Player player = null;
					if(i < team.Players.Count)
						player = team.Players[i];

					if(player != null)
					{
						if(!string.IsNullOrEmpty(player.Avatar))
						{
							var avatar = new CircleImage
							{
								HeightRequest = size,
								WidthRequest = size,
								Source = new UriImageSource
								{
									CachingEnabled = true,
									Uri = new Uri(player.Avatar)
								}
							};
							avatarStack.Children.Add(avatar);
						}
					}
					else
					{
						var ring = new SvgImage
						{
							WidthRequest = size,
							HeightRequest = size,
							Source = "incomplete_item.svg"
						};
						avatarStack.Children.Add(ring);
					}
				}

				var button = new Button
				{
					BackgroundColor = Color.Transparent,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill,
					CommandParameter = team.Id,
					AutomationId = $"teamButton{ViewModel.Game.Teams.IndexOf(team)}",
					Command = ChooseTeamCommand,
				};

				grid.Children.Add(bar);
				grid.Children.Add(avatarStack);
				grid.Children.Add(teamNameLabel);
				grid.Children.Add(button);
				rootGrid.Children.Add(grid);

				if(itemIndex % 2 == 1)
					rowIndex++;

				itemIndex++;
				colorIndex++;
			}

			scrollView.Content = rootGrid;
		}

		#endregion

		async void HandleChooseTeam(object parameter)
		{
			if(ViewModel.Game.IsCoordinator() || ViewModel.Game.HasEnded || ViewModel.Game.GetTeam() != null)
				return;

			var game = await ViewModel.JoinTeam(parameter.ToString());

			if (game != null)
			{
				await Navigation.PushAsync(new GameDetailsPage(game));
				Navigation.RemoveSecondToLastPage();
			}
		}
	}
}