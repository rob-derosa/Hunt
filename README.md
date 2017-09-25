# Hunt

### Overview
Hunt is a virtual scavenger hunt app where players can join a game, select a team and solve hints to acquire treasure. The team with the most points wins.

### Front-end Patterns
* Each ContentPage is bound to a ViewModel, which is also used as the BindingContext.
* All outbound requests are routed as tasks through TaskRunner.RunProtected to handle common failure scenarios
* Game document writes are routed through SaveGameSafe to handle version conflicts resiliently


### Technologies Utilized
* #### Front-end
  * Xamarin Forms
  * Mobile Center
    * Build
    * Test
    * Analytics
    * Push
    * Crashes
    * Distribution
  * 3rd Party SDKs
    * SkiaSharp
    * Lottie
    * PullToRefreshPage
    * ImageCircle
    * CrossMedia
    * CrossConnectivity
    * ZXing
    * XFGloss
    
* #### Back-end
  * C# Functions
  * Cosmos / Document DB
  * Blob Storage
  * Application Insights
  * Mobile Center Push
  * Service Bus
  * ARM Templates


[![Deploy to Azure](https://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)
