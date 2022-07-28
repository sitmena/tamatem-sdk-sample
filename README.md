# tamatem-sdk-sample

After cloning this repo, please follow the following steps to get the project working:

1. Go to Assets/Scenes and double click on SampleScene.
2. Now go to the folder of the assets and double click on `tamatem-sdk.unitypackage`, which will extract the needed files.
3. From the Heirarchy panel, remove TamatemPrefab (if it exists) and add it again from the project panel.
4. Now go inside the `Main Camera` from Heirarchy panel then drag the InfoText component to the Info Text field in Tamatem script inside the Inspector panel and DataPlayerInput into the InputField.
5. Click on the LoginButton from Heirarchy panel, drag and drop TamatemPrefab to the OnClick in the Inspector panel and choose `authenticateUser` function from *Tamatem* script.
6. Repeat step number 5 for the UserButton, PurchasedButton, RedeemedButton, RedeemInvButton and ConnectDataButton then choose `getUserInfo`, `getPurchasedItems`, `getRedeemedItems`, `redeemInventory` and `connectPlayerData` functions respectively.
7. Now run your app on Android or iOS to enjoy the full functionality.
