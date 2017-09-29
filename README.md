# Hunt

### Overview
Hunt is a virtual scavenger hunt app where players can join a game, select a team and solve hints to acquire treasure. The team with the most points wins.


### Build Status

[![Build status](https://build.mobile.azure.com/v0.1/apps/e282924c-141c-4f66-8575-093291d8b5c1/branches/master/badge)](https://mobile.azure.com)


### Supported Platforms
* iOS 10.0 and up
* Android 5.0 and up


### Front-end Patterns
* Each ContentPage derives from BaseContentPage<T> where T is a BaseViewModel and serves as the BindingContext.
* All outbound requests are routed as new tasks through TaskRunner.RunProtected to handle common failure scenarios such as device network connectivity changes, back-end server outages, etc.
* Game document writes are further routed through SaveGameSafe proxy method to handle version conflicts resiliently with minimal code.
* Most images/icons are SVG files embedded in the Common project assembly once and rendered at runtime via the custom SvgImage control. Fill color and vector scale can be specified per instance.
* Animations are made possible with the Lottie Animation library from the folks at Airbnb. Animations are also vector and stored in a small .json file.
* Almost all UI code is shared, include the custom HUD and Toast elements.

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
  * Mobile Center Push via REST
  * Cognitive Services
    * Vision API
    * Content Moderator API
  * Service Bus
  * ARM Templates


[![Deploy to Azure](https://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)
