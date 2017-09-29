using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xamarin.UITest;

namespace Hunt.Mobile.UITest
{
	public partial class Tests
	{
		[Test]
		public void CreateNewGame()
		{
			Reigster("bronn@hbo.com", "Bronn");

			app.Tap("createGameButton");
			app.Screenshot("And I click the 'Create Game' button");

			app.ScrollDownTo("isCoordinatorSwitch");

			//var propertyName = "setOn";
			//if(TestEnvironment.Platform == TestPlatform.TestCloudAndroid)
				//propertyName = "isChecked";
			//app.Query(c => c.Switch("isCoordinatorSwitch").Invoke(propertyName, true));

			app.Tap("isCoordinatorSwitch");

			app.ScrollDownTo("continueButton", "createGameScrollView");
			app.Screenshot("Then I can configure and save");
			app.Tap("continueButton");
			app.Screenshot("And the game should save");

			app.WaitForElement("leaveGameButton");
			app.Screenshot("Then I should see the Game Details page");

			app.Tap("backButton");
			app.Screenshot("Dashboard when I have a game");
			app.Tap("goToGameButton");

			app.Tap("shareGameButton");
			app.Screenshot("Share game invite page");
			app.Tap("closeButton");

			app.Tap("teamsButton");

			Thread.Sleep(3000); //Give a few seconds for the images to load
			app.Screenshot("Teams page");
			app.Tap("backButton");

			app.Tap("addTreasureButton");
			app.Screenshot("Add Treasure page");
			app.Tap("backButton");

			app.Tap("treasureRow");
			Thread.Sleep(3000); //Give a few seconds for the images to load
			app.Screenshot("Treasure details page");

			app.Tap("heroImage");
			app.Screenshot("Full size image in portrait");
			app.SetOrientationLandscape();
			app.Screenshot("Full size image in landscape");
			app.Tap("closeButton");
			app.SetOrientationPortrait();
			app.Tap("backButton");

			app.Tap("leaveGameButton");
			app.Screenshot("Leave game confirmation");
			app.Tap("Yes");
			app.Screenshot("Dashboard page after leave game");

			app.Tap("logoutButton");
			app.Screenshot("Log out confirmation");
			app.Tap("Yes");
			app.Screenshot("Registration page after log out");
		}
	}
}
