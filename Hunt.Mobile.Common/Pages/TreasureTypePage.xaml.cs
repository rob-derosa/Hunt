using System;
using System.Windows.Input;
using Hunt.Common;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public partial class TreasureTypePage : BaseContentPage<BaseAddTreasureViewModel>
	{
		public TreasureTypePage()
		{
			Initialize();
		}

		public TreasureTypePage(Game game)
		{
			ViewModel.SetGame(game);
			Initialize();
		}

		void Initialize()
		{
			InitializeComponent();
		}

		async void CustomTreasureClicked(object sender, EventArgs e)
		{
			var page = new AddCustomTreasureImagesPage(ViewModel.Game);
			page.ViewModel.OnTreasureAdded = ViewModel.OnTreasureAdded;
			await Navigation.PushAsync(page);
		}

		async void GenericTreasureClicked(object sender, EventArgs e)
		{
			var page = new AddGenericTreasurePage(ViewModel.Game);
			page.ViewModel.OnTreasureAdded = ViewModel.OnTreasureAdded;
			await Navigation.PushAsync(page);
		}
	}
}