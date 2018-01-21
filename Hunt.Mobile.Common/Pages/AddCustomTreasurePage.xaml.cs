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

		async void SubmitClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(ViewModel.AssignedTags))
			{
				Hud.Instance.ShowToast("Please enter some comma-separated tags that define this object");
				return;
			}

			if(string.IsNullOrWhiteSpace(ViewModel.Hint))
			{
				Hud.Instance.ShowToast("Please enter a hint as a clue for the players");
				return;
			}

            try
            {
                var success = await ViewModel.SaveTreasure();

                if (success)
                {
                    await Navigation.PopModalAsync();
                }
                else
                {
                    Hud.Instance.ShowToast($"There was an error - shrug");
                }
            }
            catch(Exception ex)
            {
                Hud.Instance.ShowToast(ex.Message);
            }
		}
	}
}