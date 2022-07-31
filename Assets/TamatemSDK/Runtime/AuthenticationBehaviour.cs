using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Dynamic;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;

namespace AuthenticationScope
{
    public class AuthenticationBehaviour : MonoBehaviour
    {

        private static AuthenticationBehaviour _instance;
        private static AuthenticationBehaviour mono;
        internal DataRequestsProcess dataRequestsInterface;
        private Queue<Action> jobs = new Queue<Action>();
        private String clientID;
        private String scheme;
        private String redirectURI;
        private bool isDevelopment;

        void Awake(){
            if (_instance == null){
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            } else {
                Destroy(this);
            }
        }

        void Start() {
            mono = this;
        }

        internal static AuthenticationBehaviour getInstance() {
            return _instance;
        }

        internal void setParameters(DataRequestsProcess dataRequestsProcess, String gameClientID, String gameScheme, String gameRedirectURI, bool isDevelopment) {
            _instance.dataRequestsInterface = dataRequestsProcess;
            _instance.clientID = gameClientID;
            _instance.scheme = gameScheme;
            _instance.redirectURI = gameRedirectURI;
            _instance.isDevelopment = isDevelopment;
        }

        #if UNITY_IOS
            [DllImport("__Internal")]
            private static extern void framework_Authenticate(string clientID, string scheme, string redirectURI, bool isDevelopment);
            [DllImport("__Internal")]
            private static extern void framework_setDelegate(DelegateCallbackFunction callback);
        #endif

        public delegate void DelegateCallbackFunction(string tokenModel);

        [MonoPInvokeCallback(typeof(DelegateCallbackFunction))]
        public static void onSuccess(string tokenModel) {
            Debug.Log("User Logged in iOS");
            Debug.Log("Message received: " + tokenModel);

            var result = JObject.Parse(tokenModel);
            mono.updateUserParameters(result);
            mono.AddJob(() => {
                mono.dataRequestsInterface.loginSucceeded(result);
            });
        }

        void Update() {
            while (jobs.Count > 0) {
                jobs.Dequeue().Invoke();
            }
        }

        internal void AddJob(Action newJob) {
            jobs.Enqueue(newJob);
        }

        internal void InitializeAuth()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                using(AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                    AndroidJavaObject activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
                    activityContext.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        AndroidJavaClass tamatemClass = new AndroidJavaClass("com.tamatem.auth.TamatemAuth");
                        AndroidJavaObject authInstance = tamatemClass.CallStatic<AndroidJavaObject>("getInstance");
                        authInstance.Call("startLoginProcess", activityContext, _instance.clientID, _instance.redirectURI, _instance.isDevelopment, new AndroidPluginCallback(mono));
                    }));
                }
            #endif
            #if UNITY_IOS && !UNITY_EDITOR
                framework_setDelegate(onSuccess);
                framework_Authenticate(_instance.clientID, _instance.scheme, _instance.redirectURI, _instance.isDevelopment);
            #endif
        }

        internal void getUserDataFromServer() {
            Debug.Log("getUserDataFromServer");
            if(_accessToken == null) {
                return;
            }

            Debug.Log("add getUserDataFromServer job");
            AddJob(() => {
                // Will run on main thread, hence issue is solved
                StartCoroutine(GetUser());
            });
        }

        internal void getPurchasedItems() {
            Debug.Log("getPurchasedItems");
            if(_accessToken == null) {
                return;
            }

            Debug.Log("add getPurchasedItems job");
            AddJob(() => {
                // Will run on main thread, hence issue is solved
                StartCoroutine(PurchasedInventory());
            });
        }

        internal void getRedeemedItems() {
            Debug.Log("getRedeemedItems");
            if(_accessToken == null) {
                return;
            }

            Debug.Log("add getRedeemedItems job");
            AddJob(() => {
                // Will run on main thread, hence issue is solved
                StartCoroutine(FilterInventory(true));
            });
        }

        internal void connectData(string playerData) {
            Debug.Log("connectData");
            if(_accessToken == null) {
                return;
            }

            Debug.Log("add connectData job");
            AddJob(() => {
                // Will run on main thread, hence issue is solved
                StartCoroutine(ConnectPlayerData(playerData));
            });
        }

        internal void redeemInventory(int inventoryId) {
            Debug.Log("redeemInventory");
            if(_accessToken == null) {
                return;
            }

            Debug.Log("add redeemInventory job");
            AddJob(() => {
                // Will run on main thread, hence issue is solved
                StartCoroutine(RedeemInventory(inventoryId));
            });
        }

        internal void updateUserParameters(JObject result) {

            SetAccessToken(result["access_token"].ToObject<string>());
            SetRefreshToken(result["refresh_token"].ToObject<string>());
            SetExpiry(result["expires_in"].ToObject<long>());
        }

        private DateTime _JanFirst1970 = new DateTime(1970, 1, 1);
        private string _accessToken {get; set;}
        private long _expiry{get; set;}
        private string _refreshToken {get; set; }
        private JToken _user {get; set; }

        internal string GetAccessToken()
        {
            return _accessToken;
        }

        internal void SetAccessToken(string accessToken)
        {
            _accessToken = accessToken;
            Debug.Log("Access Token " + _accessToken);
        }

        internal string GetRefreshToken()
        {
            return _refreshToken;
        }

        internal void SetRefreshToken(string refreshToken)
        {
            _refreshToken = refreshToken;
            Debug.Log("Refresh Token " + _refreshToken);
        }
        internal long GetExpiry()
        {
            return _expiry;
        }

        internal void SetExpiry(long expiry)
        {
            _expiry = expiry + _getTime();
            Debug.Log("Expiry " + _expiry);
        }

        private long _getTime()
        {
            return (long)((DateTime.Now.ToUniversalTime() - _JanFirst1970).TotalMilliseconds + 0.5);
        }

        internal bool IsloggedIn()
        {
           if (_accessToken == null && _getTime() < _expiry)
           {
                return false;
           }
           else {
                return true;
           }
        }

        internal IEnumerator GetUser() {
             using (UnityWebRequest www = UnityWebRequest.Get("https://tamatem.dev.be.starmena-streams.com/api/player/")){
                www.SetRequestHeader("Authorization", "Bearer " + _accessToken);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) {
                    dataRequestsInterface.getUserResult(null);
                    Debug.Log(www.error);
                }
                else {
                    dataRequestsInterface.getUserResult(www.downloadHandler.text);
                    Debug.Log(www.downloadHandler.text);
                }
             }
        }

        internal IEnumerator PurchasedInventory() {
             using (UnityWebRequest www = UnityWebRequest.Get("https://tamatem.dev.be.starmena-streams.com/api/inventory-item/")){
                www.SetRequestHeader("Authorization", "Bearer " + _accessToken);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) {
                    dataRequestsInterface.purchasedItemsResults(null);
                    Debug.Log(www.error);
                }
                else {
                    dataRequestsInterface.purchasedItemsResults(www.downloadHandler.text);
                    Debug.Log(www.downloadHandler.text);
                }
             }
        }

        internal IEnumerator FilterInventory(bool isRedeemed) {
             using (UnityWebRequest www = UnityWebRequest.Get("https://tamatem.dev.be.starmena-streams.com/api/inventory-item/?is_redeemed=" + isRedeemed)){
                www.SetRequestHeader("Authorization", "Bearer " + _accessToken);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) {
                    dataRequestsInterface.redeemedItemsResults(null);
                    Debug.Log(www.error);
                }
                else {
                    dataRequestsInterface.redeemedItemsResults(www.downloadHandler.text);
                    Debug.Log(www.downloadHandler.text);
                }
             }
        }

        internal IEnumerator RedeemInventory(int inventoryId) {
            string data = "{\"is_redeemed\":true}";

            var www = new UnityWebRequest();
            www.url = "https://tamatem.dev.be.starmena-streams.com/api/inventory/redeem/" + inventoryId + "/";
            www.method = UnityWebRequest.kHttpVerbPUT;
            www.downloadHandler = new DownloadHandlerBuffer();
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(data));
            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + _accessToken);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                dataRequestsInterface.redeeemInventoryResult(null);
                Debug.Log(www.error);
            }
            else {
                dataRequestsInterface.redeeemInventoryResult(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.text);
            }
        }

        internal IEnumerator ConnectPlayerData(string gamePlayerData) {
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(gamePlayerData);

            var www = new UnityWebRequest();
            www.url = "https://tamatem.dev.be.starmena-streams.com/api/player/set-game-data/";
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.downloadHandler = new DownloadHandlerBuffer();
            www.uploadHandler = new UploadHandlerRaw(dataBytes);
            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + _accessToken);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                dataRequestsInterface.connectPlayerDataResult(null);
                Debug.Log(www.error);
            }
            else {
                dataRequestsInterface.connectPlayerDataResult(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    class AndroidPluginCallback : AndroidJavaProxy
    {
        private AuthenticationBehaviour mono;

        public AndroidPluginCallback(AuthenticationBehaviour mon) : base ("com.tamatem.auth.TamatemAuth$AuthorizationCallback") {
            mono = mon;
        }

        void onSuccess(string obj)
        {
            Debug.Log("Results retreived successfully!!");
            Debug.Log("Token retreived from Unity: " + obj);

            var result = JObject.Parse(obj);
            mono.updateUserParameters(result);
            mono.AddJob(() => {
                mono.dataRequestsInterface.loginSucceeded(result);
            });
        }

        void onFail()
        {
            Debug.Log("Failed to retreive token");
        }
    }
}