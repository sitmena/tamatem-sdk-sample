# tamatem-sdk-sample

This repository has been created to ease the job of authentication and integration process with Tamatem platform just by implementing the following steps:

## Unity Integration:
All you need to do at this stage is the following:
1. Download the `tamatem-sdk.unitypackage` file on your machine.
2. Double click on the unitypackage file and import all the files inside it.
##### The following steps are needed to check that everything is in place to use it properly
3. Now go to Assets/Scenes and double click on SampleScene.
4. From the Heirarchy panel, remove TamatemPrefab (if it exists).
5. From the Project panel go to Assets folder then drag and drop the TamatemPrefab file to the SampleScene and make sure that it contains AuthenticationBehaviour.cs and Tamatem.cs scripts attached to it as components.
6. Now go inside the Main Camera from Heirarchy panel then drag the InfoText component to the Info Text field in Tamatem script inside the Inspector panel and DataPlayerInput into the InputField.
7. Click on the LoginButton from Heirarchy panel, drag and drop TamatemPrefab to the OnClick in the Inspector panel and choose authenticateUser function from Tamatem script.
8. Repeat step number 5 for the UserButton, PurchasedButton, RedeemedButton, RedeemInvButton and ConnectDataButton then choose getUserInfo, getPurchasedItems, getRedeemedItems, redeemInventory and connectPlayerData functions respectively.
9. Now run your app on Android or iOS to enjoy the full functionality.

## Android Setup:

Once your Unity project is ready and you want to setup your Android game/app, please follow the steps below:
1. Go to File -> Build Settings...
2. Click on Player Settings then make sure Android tab is selected.
3. Open Publisher Settings panel.
4. Check `Custom Main Gradle Template` and `Custom Gradle Properties Template` checkboxes.
5. Open `mainTemplate.gradle` file and paste the following lines inside the dependencies tag.
```shell script
    implementation 'androidx.constraintlayout:constraintlayout:2.1.4'
    implementation("androidx.browser:browser:1.4.0")
    implementation 'com.squareup.okhttp3:okhttp:4.10.0'
    implementation ('com.squareup.retrofit2:retrofit:2.5.0') {
        exclude module: 'okhttp'
    }
    implementation 'com.google.code.gson:gson:2.8.6'
    implementation 'com.squareup.retrofit2:converter-gson:2.3.0'
```
> **_NOTE:_** make sure these dependencies do not conflict with other libraries' dependencies.
6. Now Open `gradleTemplate.gradle` file and paste the following line at the end of file.
```shell script
    android.useAndroidX=true
```
7. Close the Player Settings screens and now you can either run the app directly using the `Build And Run` button or by checking the `Export Project` checkbox.
8. Press on the Export button and choose the folder that will contain the Android project.
9. Open the project using Android Studio to enjoy using your app.


## iOS Setup:

Once your Unity project is ready and you want to setup your iOS game/app, please follow the steps below:
1. Go to File -> Build Settings...
2. Press on "Build And Run" and choose the folder that will contain the iOS project.
3. Once its finished, it will open the project on xCode.
4. Go to "Unity-iPhone" Target and open the "Build Phase" tab.
5. Expand the "Embed Frameworks" section and press the "+" button.
6. Type "keyCSDK" to add "keyCSDK.framework".
7. Now Make sure to stop all previous running tasks and run the app again.
8. Congrats! your iOS app is ready to go.


## SDK API:

### Runtime/Tamatem.cs

#### Game Configuraton Constants
- GAME_CLIENT_ID
- GAME_SCHEME
- GAME_REDIRECT_URI
- GAME_DEVELOPMENT_ENV

These values can be changed based on your game's configrations
When setting GAME_DEVELOPMENT_ENV to true, means that it will connect the game to the dev server which is made for development purposed. Please make sure to set it to false before releasing to Production.

#### authenticateUser()
this method is to open the in-app browser to let the user login

#### getUserInfo()
this method is to get the logged-in user object
```json
{
"id": 16,
"first_name": "Moath Test",
"last_name": "Test",
"email": "tesasdt+1@teasdsa3dasdasdasdst.com",
"country": "american-samoa",
"phone_number": "+962797351170",
"date_of_birth": "2022-05-14",
"sign_up_through": "email",
"gender": "f",
"qr_code": "/media/qr-codes/PR22000006.png",
"avatar": null,
"tamatem_id": "PR22000006"
}
```

#### getPurchasedItems()
this method is to get the purchased items

#### getRedeemedItems()
this method is to get the redeemed items (you can set the is_redeemed paramter value from the `AuthenticationBehaviour.cs` script)

#### redeemInventory()
this method is to set a non-redeemed item to redeemed based on the inventory id

#### connectPlayerData()
this method is to connect player's data based on the json object that will be sent (placeholder contains a sample of the json).


### Runtime/AuthenticationBehaviour.cs
### Class TamatemSDK

#### bool IsloggedIn()
returns if user is logged in
#### void InitializeAuth()
triggers native authentication process
#### User GetUser()
returns the logged-in user based on the sent token
#### Item[] PurchasedInventory()
returns all items
#### Item[] FilterInventory(bool isRedeemed)
returns redeemed or non redeemed items based on isRedeemed
#### Object RedeemInventory(int inventoryId)
returns the status of the redeemed inventory
#### Object ConnectPlayerData(string gamePlayerData)
returns the token of the connected player's data
#### GetUser()
returns JToken containing the following