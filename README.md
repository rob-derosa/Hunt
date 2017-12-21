# Hunt

### Supported Platforms

Platform | Build | Install
--- | --- | ---
iOS 10.0 and up | [![Build status](https://build.mobile.azure.com/v0.1/apps/e282924c-141c-4f66-8575-093291d8b5c1/branches/master/badge)](https://mobile.azure.com) | [latest release](https://install.mobile.azure.com/orgs/hunt-app/apps/hunt/distribution_groups/public)
Android 5.0 and up | [![Build status](https://build.mobile.azure.com/v0.1/apps/61439d5f-9b86-461b-8128-738730c45b6b/branches/master/badge)](https://mobile.azure.com) | [latest release](https://install.mobile.azure.com/orgs/hunt-app/apps/hunt-android/distribution_groups/public)

### Description
Hunt is a virtual scavenger hunt app where players can join a game, select a team and solve hints to acquire treasure. The team with the most points wins.

### Overview
The purpose of Hunt is to facilitate a virtual scavenger hunt between multiple teams. Players on each team will need to solve a riddle/hint and photograph the answer/object. If the photograph contains the correct object, the team will be awarded the full amount of points for that treasure.

For example, if the hint is "What has a neck, no head yet still wears a cap?" The teams would need to determine that the answer is a bottle and photograph a bottle. The photograph will be analyzed for the tag `bottle` and if it exists, the team is awarded the points.

The first team to acquire all the treasures will win the game. If no team acquires all the treasure, the team with the most points will win when the time runs out. Alternatively, the Coordinator of the game can end the game early, in which case the team with the most points will win.

### Video

[https://www.youtube.com/watch?v=pXjBQD9a3AE](https://www.youtube.com/watch?v=pXjBQD9a3AE)

### Screenshots
<img src="https://huntappstorage.blob.core.windows.net/assets/screenshots/iOS/login_page_completed.jpg" alt="Registration page" Width="210" /> <img src="https://huntappstorage.blob.core.windows.net/assets/screenshots/iOS/loading.jpg" alt="Loading profile" Width="210" /> <img src="https://huntappstorage.blob.core.windows.net/assets/screenshots/iOS/dashboard.jpg" alt="Dashboard page" Width="210" /> <img src="https://huntappstorage.blob.core.windows.net/assets/screenshots/iOS/add_treasure_complete.jpg" alt="Coordinator adding new treasure" Width="210" /> <img src="https://huntappstorage.blob.core.windows.net/assets/screenshots/iOS/coordinator_game_not_started.jpg" alt="Coordinator view of pre-game" Width="210" /> <img src="https://huntappstorage.blob.core.windows.net/assets/screenshots/iOS/share_game.jpg" alt="Share game invite" Width="210" /> <img src="https://huntappstorage.blob.core.windows.net/assets/screenshots/iOS/teams.jpg" alt="Teams list" Width="210" /> <img src="https://huntappstorage.blob.core.windows.net/assets/screenshots/iOS/player_game_started.jpg" alt="Player view of started game" Width="210" />


### Rules
  
* Roles
  * Users can be involved in only one game at a time as either the Coordinator or a Player
    * Coordinators are responsible for
      * creating the game by configuring:
        * the amount of teams
        * the amount of players per teams
        * the amount of time the game lasts (from 5 - 180min)
      * adding treasure to the game
      * starting the game
      * ending the game early, if necessary
    * Players are responsible for
      * joining games via a 6-char entry code or scanning the QR code
      * choosing a team
      * acquring treasure once the game has started
* Treasure
  * A treasure consists of a hint and a tagged photograph supplied by the Coordinator.
  * There must be at least 2 treasures added to the game before it can be started.
  * Photos taken by the Coordinator are analyzed by
    * [Computer Vision](https://azure.microsoft.com/en-us/services/cognitive-services/computer-vision/) and a set of associated tags is presented, of which up to 2 can be selected. Generic treasures are acquired if the player snaps a pucture of the object and the analyzation result contains at least one of the tags assigned to the treasure. 
    * [Custom Vision](https://azure.microsoft.com/en-us/services/cognitive-services/custom-vision-service/) where non-generic objects (landmarks, retail items, etc), are photographed 5 - 10 times from different angles which generates a trained model. Custom Vision treasures are acquired if the player snaps a picture of the object and the analyzation result is of .7 degree of probability of higher.
  * If teams solve the hint and take a photo of a similar object with any matching tags, the treasure is acquired.
  * As a Coordinator, when adding treasure, the hint should not give away the object - the intent is to make the players solve a riddle or puzzle and then take a photograph of the answer.
    

### Front-end Patterns
* Each ContentPage derives from `BaseContentPage<T>` where `T` is of type `BaseViewModel` and serves as the `BindingContext`.
* All outbound requests are routed as new tasks through `TaskRunner.RunProtected` to handle common failure scenarios such as device network connectivity changes, back-end server outages, etc.
* Game document writes are further routed through `ViewModel.SaveGameSafe` proxy method to handle version conflicts resiliently with minimal code.
* Most images/icons are SVG files embedded in the `Hunt.Mobile.Common` project assembly once and rendered at runtime via the custom` SvgImage` control. Fill color and vector scale can be specified per instance.
* Animations are made possible with the Lottie Animation library from the folks at Airbnb. Animations are also vector and stored in a small .json file.
* Almost all UI code is shared, include the custom HUD and Toast elements.
* The Forms navigation stack is utlizied, however, every page has `SetHasNavigationBar` set to `false`. A custom `NavigationToolbar` is used instead to better control the UI.
* Content for each page is declared in XAML under the `BaseContentPage.RootContent` node instead of the typical `BaseContentPage.Content` so HUD and Toast can appear at a greater Z-index.
* Supports iOS, Android, Phone, Tablet, Landscape, Portrait

### Back-end Patterns
* Games are saved as documents in DocumentDB. Games contain the teams, players, treasures and acquired treasures in a single document. Whenever the game is updated, players of the game are notified via silent push notification which triggers a game refresh.
* Version conflicts are raised if the associated document timestamp is out of order - it is up to the client to handle this exception and resolve the conflict. See `ViewModel.SaveGameSafe`.
* All images are stored in blob storage and passed to the Computer Vision or Custom Vision via the blob URL.


### Technologies Utilized
* #### Front-end
  * [Xamarin.Forms](http://xamarin.com/forms)
  * [App Center](http://mobile.azure.com)
    * Build
    * Test
    * Analytics
    * Crashes
    * Distribution
  * 3rd Party SDKs
    * [SkiaSharp](https://github.com/mono/SkiaSharp) (vector and SVG render kit)
    * [Lottie](https://github.com/martijn00/LottieXamarin) (animations)
    * [PullToRefreshPage](https://github.com/jamesmontemagno/Xamarin.Forms-PullToRefreshLayout) (pull to refresh on non-ListView pages)
    * [ImageCircle](https://github.com/jamesmontemagno/Xamarin.Plugins/tree/master/ImageCircle) (better than a square)
    * [CrossMedia](https://github.com/jamesmontemagno/MediaPlugin) (capturing camera photos)
    * [CrossConnectivity](https://github.com/jamesmontemagno/Xamarin.Plugins/tree/master/Connectivity) (determining device network connectivity)
    * [ZXing](https://github.com/Redth/ZXing.Net.Mobile) (QR code generation/scanning)
    * [XFGloss](https://github.com/tbaggett/xfgloss) (styled switches and sliders, background gradient)
    
* #### Back-end
  * [Functions (C#)](https://azure.microsoft.com/en-us/services/functions)
  * [Cosmos: Document DB](https://azure.microsoft.com/en-us/services/cosmos-db/)
  * [Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/)
  * [Application Insights](https://azure.microsoft.com/en-us/services/application-insights/)
  * [Notification Hubs](https://azure.microsoft.com/en-us/services/notification-hubs/)
  * Cognitive Services
    * [Custom Vision](https://azure.microsoft.com/en-us/services/cognitive-services/custom-vision-service/) 
    * [Computer Vision](https://azure.microsoft.com/en-us/services/cognitive-services/computer-vision/)
    * [Content Moderator](https://azure.microsoft.com/en-us/services/cognitive-services/content-moderator/)
  * [Service Bus (timed brokered message to end games)](https://azure.microsoft.com/en-us/services/service-bus/)
  * Unit tested on commit
  * [Deployment via ARM Templates](https://azure.microsoft.com/en-us/resources/templates/)
  * [Azure CDN](https://azure.microsoft.com/en-us/services/cdn/)
  
* #### Planning and Build
  * VSTS Work item tracking
  * VSTS Code repository
  * VSTS Build for backend + unit testing

#### Server Architecture
<div style="text-align:center"><center><img src="https://github.com/rob-derosa/Hunt/blob/master/Resources/Assets/Design/architecture.png?raw=true" alt="Azure Architecture"/></center></div>


### Presentations and Hackathons

Hunt was designed and is intended specifically for use with audiences at presentations, hackathons, community meetups, on-site meetings and speaking engagements.

#### Interactive Option
To add some fun to a presentation, one option is to engage the audience in a quick game of hunt. Prior to the presentation, plant 2 or 3 well-known objects in the room somewhere inconspicuous, like a Coke bottle and a sneaker. Begin the talk by asking the audience if they want to play a game.

Display http://aka.ms/hunt on a projector and invite people to download the app. As you give an overview of Hunt, project your phone's screen so everyone can see ([Reflector](http://www.airsquirrels.com/reflector/) is a good option here as it can broadcast multiple screens simultaneously across iOS and Android using Apple Airplay or Google Chromecast). Create a quick 10-15min game seeded with some players and treasures. Then share the game alphanumeric entry and QR code so participants can join a team.

Start the game and let the teams attempt to find the objects and acquire the treasure. Consider rewarding the winning team with free Azure credit.].

#### Mock Data
* If you do not have your own Gravatar account, you can use one of a dozen Game of Thrones characters by entering their _firstname@hbo.com_ (i.e. _arya@hbo.com_)
  * supported accounts: ned, sansa, arya, jon, joffrey, cersei, jamie, theon, yara, euron, etc
* When creating a new game, the mobile app has several options for seeding data into an empty game. The following options are available and allow the game to be put into different states depending on the goal of the demo.
  * User can choose to be the Coordinator or a Player
    * if coordinator is chosen, the user can add additional treasure and manully start the game
    * if player is chosen, the user is put on House Stark and the game will be started automatically
  * Games can be seeded with players that join teams - about half the game slots will fill with random mock players
  * Games can be seeded with 3 pre-configured treasures (bottle, shoe, dog)
  * If both players and treasures are seeded, the team the player joins can be seeded with 2 acquired treasure so it only takes one more acqusition to end the game.

#### Azure Deployment
To make it easy for new developers to stand up their own personal Hunt backend, ARM templates are used so that by clicking a button and selecting a few options (like subscription, resource group and region), developers can deploy their own pre-configured instance of all services needed by Hunt to function.

[![Deploy to Azure](https://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)
