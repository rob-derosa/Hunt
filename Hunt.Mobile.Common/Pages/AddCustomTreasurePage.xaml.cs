using System;

namespace Hunt.Mobile.Common
{
	public partial class AddCustomTreasurePage : BaseContentPage<AddCustomTreasureViewModel>
	{
		public AddCustomTreasurePage()
		{
			if(IsDesignMode)
			{
				ViewModel.SetGame(App.Instance.CurrentGame);
			}

			Initialize();
		}

		public AddCustomTreasurePage(AddCustomTreasureViewModel viewModel)
		{
			ViewModel = viewModel;
			BindingContext = ViewModel;
			Initialize();
		}

		void Initialize()
		{
			InitializeComponent();
		}

		void SubmitClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(ViewModel.AssignedTag))
			{
				Hud.Instance.ShowToast("Please enter a tag that defines this object");
				return;
			}

			if(string.IsNullOrWhiteSpace(ViewModel.Hint))
			{
				Hud.Instance.ShowToast("Please enter a hint as a clue for the players");
				return;
			}

			//var success = await ViewModel.TrainImageSet();
			//if(success)
			//{
			//	var page = new AssignAttributesPage(ViewModel);
			//	await Navigation.PushAsync(page);
			//}
			//else
			//{
			//	Hud.Instance.ShowToast($"No objects were able to be identified in the photo. Please take another photo.");
			//	ViewModel.Photo = null;
			//}
		}
	}
}