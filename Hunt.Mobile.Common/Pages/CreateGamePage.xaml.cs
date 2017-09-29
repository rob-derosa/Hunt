using System;

namespace Hunt.Mobile.Common
{
	public partial class CreateGamePage : BaseContentPage<CreateGameViewModel>
	{
		public Action OnGameCreated { get; set; }

		public CreateGamePage()
		{
			InitializeComponent();
		}

		async void ContinueClicked(object sender, EventArgs e)
		{
			await ViewModel.CreateGameAsync();

			if(App.Instance.CurrentGame != null)
			{
				try
				{
					await Navigation.PopModalAsync();
					OnGameCreated?.Invoke();
				}catch{}
			}
		}
	}
}