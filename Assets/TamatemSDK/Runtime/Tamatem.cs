using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace AuthenticationScope
{
    public interface DataRequestsProcess {
        void loginSucceeded(JObject result);
        void loginFailed();
        void getUserResult(string result);
        void purchasedItemsResults(string result);
        void redeemedItemsResults(string result);
        void redeeemInventoryResult(string result);
        void connectPlayerDataResult(string result);
    }

    public class Tamatem : MonoBehaviour, DataRequestsProcess
    {
        //TODO: please change the below constants to match your game's configurations
        private const string GAME_CLIENT_ID = "pi4dEipJyFLDbO9DOYWFlolNpOgzjjYI2oq0qVJz";
        private const string GAME_SCHEME = "game1";
        private const string GAME_REDIRECT_URI = "game1://oauth-callback";
        private const bool GAME_DEVELOPMENT_ENV = true;

        public Text InfoText;
        public InputField DataPlayerInputField;

        private AuthenticationBehaviour getAuthenticationBehaviour() {
            return AuthenticationBehaviour.getInstance();
        }

        public void authenticateUser() {
            InfoText.text = "start login process";
            AuthenticationBehaviour instance = getAuthenticationBehaviour();
            if(instance == null) {
                InfoText.text = "no instance found";
                return;
            }
            instance.setParameters(this, GAME_CLIENT_ID, GAME_SCHEME, GAME_REDIRECT_URI, GAME_DEVELOPMENT_ENV);
            instance.InitializeAuth();
        }

        public void getUserInfo() {
            AuthenticationBehaviour instance = getAuthenticationBehaviour();
            if(instance == null || instance.GetAccessToken() == null) {
                InfoText.text = "You need to login first";
                if(instance == null) {
                    InfoText.text = "no instance found";
                }
               return;
            }

            InfoText.text = "Loading user info ...";
            instance.getUserDataFromServer();
        }

        public void getPurchasedItems() {
            AuthenticationBehaviour instance = getAuthenticationBehaviour();
            if(instance == null || instance.GetAccessToken() == null) {
                InfoText.text = "You need to login first";
                if(instance == null) {
                    InfoText.text = "no instance found";
                }
               return;
            }

            InfoText.text = "Loading purchased items ...";
            instance.getPurchasedItems();
        }

        public void getRedeemedItems() {
            AuthenticationBehaviour instance = getAuthenticationBehaviour();
            if(instance == null || instance.GetAccessToken() == null) {
                InfoText.text = "You need to login first";
                if(instance == null) {
                InfoText.text = "no instance found";
            }
               return;
            }

            InfoText.text = "Loading redeemed items ...";
            instance.getRedeemedItems();
        }

        public void redeemInventory() {
            AuthenticationBehaviour instance = getAuthenticationBehaviour();
            if(instance == null || instance.GetAccessToken() == null) {
                InfoText.text = "You need to login first";
                if(instance == null) {
                    InfoText.text = "no instance found";
                }
               return;
            }

            InfoText.text = "Redeeming items ...";
            // TODO: change the following inventory id based on your logic and preference for the non-redeemed inventories.
            instance.redeemInventory(61);
        }

        public void connectPlayerData() {
            AuthenticationBehaviour instance = getAuthenticationBehaviour();
            if(instance == null || instance.GetAccessToken() == null) {
                InfoText.text = "You need to login first";
                if(instance == null) {
                    InfoText.text = "no instance found";
                }
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

        public void loginSucceeded(JObject result) {
            //TODO: Handle your login data here
            InfoText.text = "User Logged In Successfully";
        }

        public void loginFailed() {
            //TODO: Handle being failed to login here
            InfoText.text = "Failed to login";
        }

        public void purchasedItemsResults(string result) {
            if(result == null) {
                InfoText.text = "Failed to retrieve purchased items";
            } else {
                InfoText.text = result;
            }
        }

        public void redeemedItemsResults(string result) {
            if(result == null) {
                InfoText.text = "Failed to retrieve redeemed items";
            } else {
                InfoText.text = result;
            }
        }

        public void redeeemInventoryResult(string result) {
            if(result == null) {
                InfoText.text = "Failed to redeem item";
            } else {
                InfoText.text = result;
            }
        }

        public void connectPlayerDataResult(string result) {
            if(result == null) {
                InfoText.text = "Failed to connect player data";
            } else {
                InfoText.text = result;
            }
        }

        public void getUserResult(string result) {
            if(result == null) {
                InfoText.text = "Failed to get user info";
            } else {
                InfoText.text = result;
            }
        }
    }
}