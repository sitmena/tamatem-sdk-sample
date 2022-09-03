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

        private String getServerApiUrl() {
            if (isDevelopment) {
                return "https://tamatem.dev.be.starmena-streams.com/api/";
            }
            return "https://tamatem.prod.be.starmena-streams.com/api/";
        }

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

        internal static AuthenticationBehaviour getInstance(DataRequestsProcess dataRequestsProcess, String gameClientID, String gameScheme, String gameRedirectURI, bool isDevelopment) {
            if(_instance != null) {
                _instance.dataRequestsInterface = dataRequestsProcess;
                _instance.clientID = gameClientID;
                _instance.scheme = gameScheme;
                _instance.redirectURI = gameRedirectURI;
                _instance.isDevelopment = isDevelopment;
            }
            return _instance;
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
            if(!IsloggedIn()) {
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
            if(!IsloggedIn()) {
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
            if(!IsloggedIn()) {
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
            if(!IsloggedIn()) {
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
            if(!IsloggedIn()) {
                return;
            }

            Debug.Log("add redeemInventory job");
            AddJob(() => {
                // Will run on main thread, hence issue is solved
                StartCoroutine(RedeemInventory(inventoryId));
            });
        }

        internal void updateUserParameters(JObject result) {

            AccessToken = result["access_token"].ToObject<string>();
            RefreshToken = result["refresh_token"].ToObject<string>();
            Expiry = result["expires_in"].ToObject<long>();
        }

        private DateTime _JanFirst1970 = new DateTime(1970, 1, 1);

        private const string TamatemAccessToken = "TAMATEM_SDK_ACCESS_TOKEN_KEY";
        private string AccessToken {
            get {
                return PlayerPrefs.GetString(TamatemAccessToken);
            }
             set {
                PlayerPrefs.SetString(TamatemAccessToken, value);
             }
        }
        private const string TamatemExpiry = "TAMATEM_SDK_EXPIRY_KEY";
        private long Expiry {
            get {
                var bytes = System.Convert.FromBase64String(PlayerPrefs.GetString(TamatemExpiry));
                return System.BitConverter.ToInt64(bytes, 0);
            }
             set {
                var bytes = System.BitConverter.GetBytes(value + GetTime());
                var millisInString = System.Convert.ToBase64String(bytes);
                PlayerPrefs.SetString(TamatemExpiry, millisInString);
             }
        }
        private const string TamatemRefreshToken = "TAMATEM_SDK_REFRESH_TOKEN_KEY";
        private string RefreshToken {
            get {
                return PlayerPrefs.GetString(TamatemRefreshToken);
            }
             set {
                PlayerPrefs.SetString(TamatemRefreshToken, value);
             }
        }

        private long GetTime()
        {
            return (long)((DateTime.Now.ToUniversalTime() - _JanFirst1970).TotalMilliseconds);
        }

        internal bool IsloggedIn()
        {
           if (AccessToken == null || Expiry == 0 || GetTime() < Expiry) {
                return false;
           } else {
                return true;
           }
        }

        internal IEnumerator GetUser() {
             using (UnityWebRequest www = UnityWebRequest.Get(getServerApiUrl() + "player/")){
                www.SetRequestHeader("Authorization", "Bearer " + AccessToken);
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
             using (UnityWebRequest www = UnityWebRequest.Get(getServerApiUrl() + "inventory-item/")){
                www.SetRequestHeader("Authorization", "Bearer " + AccessToken);
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
             using (UnityWebRequest www = UnityWebRequest.Get(getServerApiUrl() + "inventory-item/?is_redeemed=" + isRedeemed)){
                www.SetRequestHeader("Authorization", "Bearer " + AccessToken);
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
            www.url = getServerApiUrl() + "inventory/redeem/" + inventoryId + "/";
            www.method = UnityWebRequest.kHttpVerbPUT;
            www.downloadHandler = new DownloadHandlerBuffer();
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(data));
            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + AccessToken);
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
            www.url = getServerApiUrl() + "player/set-game-data/";
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.downloadHandler = new DownloadHandlerBuffer();
            www.uploadHandler = new UploadHandlerRaw(dataBytes);
            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + AccessToken);
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
            mono.AddJob(() => {
                mono.dataRequestsInterface.loginFailed();
            });
        }
    }
}