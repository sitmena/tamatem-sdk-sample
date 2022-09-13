using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace AuthenticationScope
{
    

    public class Tamatem : MonoBehaviour, DataRequestsProcess
    {
        //TODO: please change the below constants to match your game's configurations
        public string GAME_CLIENT_ID = "qD3T2xJueA94SjLX2pmJexJrjNwtyC6JFZuCKzqm";
        public string GAME_SCHEME = "wanas";
        public string GAME_REDIRECT_URI = "wanas://oauth-callback";
        public bool GAME_DEVELOPMENT_ENV = false;

        public Text InfoText;
        public InputField DataPlayerInputField;

        private AuthenticationBehaviour AuthenticationBehaviourInstance,instance;

        private AuthenticationBehaviour getAuthenticationBehaviour() {
            if (AuthenticationBehaviourInstance == null) {
                AuthenticationBehaviourInstance = AuthenticationBehaviour.getInstance(this, GAME_CLIENT_ID, GAME_SCHEME, GAME_REDIRECT_URI, GAME_DEVELOPMENT_ENV);
            }
            return AuthenticationBehaviourInstance;
        }

        void Start()
        {
            instance = getAuthenticationBehaviour();
        }

        void OnEnable()
        {
            AuthenticationBehaviour.UserLoggedInEvent += loginSucceeded;
            AuthenticationBehaviour.UserLoginFailedEvent += loginFailed;
            AuthenticationBehaviour.UserDataEvent += getUserResult;
            AuthenticationBehaviour.GetPurchasedItemsEvent += purchasedItemsResults;
            AuthenticationBehaviour.GetRedeemedItemsEvent += redeemedItemsResults;
            AuthenticationBehaviour.RedeemItemEvent += redeeemInventoryResult;
            AuthenticationBehaviour.SavePlayerDataEvent += connectPlayerDataResult;
        }

        void OnDisable()
        {
            AuthenticationBehaviour.UserLoggedInEvent -= loginSucceeded;
            AuthenticationBehaviour.UserLoginFailedEvent -= loginFailed;
            AuthenticationBehaviour.UserDataEvent -= getUserResult;
            AuthenticationBehaviour.GetPurchasedItemsEvent -= purchasedItemsResults;
            AuthenticationBehaviour.GetRedeemedItemsEvent -= redeemedItemsResults;
            AuthenticationBehaviour.RedeemItemEvent -= redeeemInventoryResult;
            AuthenticationBehaviour.SavePlayerDataEvent -= connectPlayerDataResult;
        }

        //Callback add your game logic in here */

        void loginSucceeded(JObject result) {
            //TODO: Handle your login data here
            InfoText.text = "User Logged In Successfully";
        }

        void loginFailed() {
            //TODO: Handle being failed to login here
            InfoText.text = "Failed to login";
        }

        void purchasedItemsResults(string result) {
            if(result == null) {
                InfoText.text = "Failed to retrieve purchased items";
            } else {
                InfoText.text = result;
            }
        }

        void redeemedItemsResults(string result) {
            if(result == null) {
                InfoText.text = "Failed to retrieve redeemed items";
            } else {
                InfoText.text = result;
            }
        }

        void redeeemInventoryResult(string result) {
            if(result == null) {
                InfoText.text = "Failed to redeem item";
            } else {
                InfoText.text = result;
            }
        }

        void connectPlayerDataResult(string result) {
            if(result == null) {
                InfoText.text = "Failed to connect player data";
            } else {
                InfoText.text = result;
            }
        }

        void getUserResult(string result) {
            if(result == null) {
                InfoText.text = "Failed to get user info";
            } else {
                InfoText.text = result;
            }
        }

        /* Call these functions to tryout the flow*/


        public void authenticateUser() {
            InfoText.text = "start login process";
            if(instance == null) {
                InfoText.text = "no instance found";
                return;
            }
            instance.InitializeAuth();
        }

        public void logoutUser() {
            InfoText.text = "start logout process";
            if(checkInstance() == false)
            {
                return;
            }
            instance.logout();
            InfoText.text = "User logged out successfully";
        }

        public void getUserInfo() {
            if(checkInstance() == false)
            {
                return;
            }

            InfoText.text = "Loading user info ...";
            instance.getUserDataFromServer();
        }

        public void getPurchasedItems() {
            if(checkInstance() == false)
            {
                return;
            }
            InfoText.text = "Loading purchased items ...";
            instance.getPurchasedItems();
        }

        public void getRedeemedItems() {
            if(checkInstance() == false)
            {
                return;
            }

            InfoText.text = "Loading redeemed items ...";
            instance.getRedeemedItems();
        }

        public void redeemInventoryItem(int itemID) {
            if(checkInstance() == false)
            {
                return;
            }

            InfoText.text = "Redeeming items ...";
            // TODO: change the following inventory id based on your logic and preference for the non-redeemed inventories.
            instance.redeemInventoryItem(itemID);
        }

        public void connectPlayerData() {
            
            if(checkInstance() == false)
            {
                return;
            }
            string json = "";

            if(DataPlayerInputField == null){
                json = "{\"game_player_data\":{\"exampleKey1\":\"exampleValue1\",\"exampleKey2\":\"exampleValue2\"}}";
            } else {
                json = DataPlayerInputField.GetComponent<InputField>().text;
                InfoText.text = json;
                if(json == null || json == "") {
                    json = DataPlayerInputField.placeholder.GetComponent<Text>().text;
                }
            }

            InfoText.text = "Connecting Player Data: " + json;
            instance.connectData(json);
        }

        private bool checkInstance()
        {
            if(instance == null || !instance.IsloggedIn()) {
               InfoText.text = "You need to login first";
               return false;
            }
            else
            {
               return true;
            }
        }

        
    }
}