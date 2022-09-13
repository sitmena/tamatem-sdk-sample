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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace AuthenticationScope
{

    /* Replaced with events for easier access through out the project*/
    public interface DataRequestsProcess {
        // void loginSucceeded(JObject result);
        // void loginFailed();
        // void getUserResult(string result);
        // void purchasedItemsResults(string result);
        // void redeemedItemsResults(string result);
        // void redeeemInventoryResult(string result);
        // void connectPlayerDataResult(string result);
    }
     

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

        private DateTime _JanFirst1970 = new DateTime(1970, 1, 1);

        private string accessTokenValue = null;
        private string AccessToken {
            get {
                return accessTokenValue;
            }
             set {
                accessTokenValue = value;
             }
        }
        private long expiryValue = 0;
        private long Expiry {
            get {
                return expiryValue;
            }
             set {
                expiryValue = value + GetTime();
             }
        }
        private string refreshTokenValue = null;
        private string RefreshToken {
            get {
                return refreshTokenValue;
            }
             set {
                refreshTokenValue = value;
             }
        }

        private String getServerApiUrl() {
            if (isDevelopment) {
                return "https://tamatem.dev.be.starmena-streams.com/api/";
            }
            return "https://tamatem.prod.be.starmena-streams.com/api/";
        }


        /****
        EVENTS
        ****/
        public delegate void UserLoggedInDelegate(JObject result);
        public static UserLoggedInDelegate UserLoggedInEvent;
        public delegate void UserLogInFailedDelegate();
        public static UserLogInFailedDelegate UserLoginFailedEvent;
        public delegate void GetUserDataDelegate(string result);
        public static GetUserDataDelegate UserDataEvent;
        public delegate void GetPurchasedItemsDelegate(string result);
        public static GetPurchasedItemsDelegate GetPurchasedItemsEvent;
        public delegate void GetRedeemedItemsDelegate(string result);
        public static GetRedeemedItemsDelegate GetRedeemedItemsEvent;
        public delegate void RedeemItemDelegate(string result);
        public static RedeemItemDelegate RedeemItemEvent;
        public delegate void SaveUserDataDelegate(string result);
        public static SaveUserDataDelegate SavePlayerDataEvent;


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

            /* Load cached data from file*/
            LoadAuthData();
        }

        void Update() {
            while (jobs.Count > 0) {
                jobs.Dequeue().Invoke();
            }
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
            if(tokenModel == null){
                return;
            }
            Debug.Log("User Logged in iOS");
            Debug.Log("Message received: " + tokenModel);

            mono.AddJob(() => {
                var result = JObject.Parse(tokenModel);
                mono.updateUserParameters(result);
                // mono.dataRequestsInterface.loginSucceeded(result);
            });
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

        internal void redeemInventoryItem(int inventoryId) {
            Debug.Log("redeemInventory");
            if(!IsloggedIn()) {
                return;
            }

            Debug.Log("add redeemInventory job");
            AddJob(() => {
                // Will run on main thread, hence issue is solved
                StartCoroutine(RedeemInventoryItem(inventoryId));
            });
        }

        internal void updateUserParameters(JObject result) {

            AccessToken = result["access_token"].ToObject<string>();
            RefreshToken = result["refresh_token"].ToObject<string>();
            Expiry = result["expires_in"].ToObject<long>() * 1000; // we need to store it in milliseconds instead of seconds

            /* Cache data to file*/
            SaveAuthData(AccessToken,RefreshToken,Expiry);
        }

        private long GetTime()
        {
            return (long)((DateTime.Now.ToUniversalTime() - _JanFirst1970).TotalMilliseconds);
        }

        internal bool IsloggedIn()
        {
           if (AccessToken == null || Expiry == 0 || GetTime() > Expiry) {
                return false;
           } else {
                return true;
           }
        }

        internal void logout()
        {
            string _accessToken = null;
            string _refreshToken = null;
            long _expireTime = 0;

            AccessToken = _accessToken;
            RefreshToken = _refreshToken;
            Expiry = _expireTime;
            SaveAuthData(_accessToken,_refreshToken,_expireTime);
        }

        internal IEnumerator GetUser() {
             using (UnityWebRequest www = UnityWebRequest.Get(getServerApiUrl() + "player/")){
                www.SetRequestHeader("Authorization", "Bearer " + AccessToken);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) {
                    // dataRequestsInterface.getUserResult(null);
                    UserDataEvent?.Invoke(null);
                    Debug.Log(www.error);
                }
                else {
                    // dataRequestsInterface.getUserResult(www.downloadHandler.text);
                    UserDataEvent?.Invoke(www.downloadHandler.text);
                    Debug.Log(www.downloadHandler.text);
                }
             }
        }

        internal IEnumerator PurchasedInventory() {
             using (UnityWebRequest www = UnityWebRequest.Get(getServerApiUrl() + "inventory-item/")){
                www.SetRequestHeader("Authorization", "Bearer " + AccessToken);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) {
                    // dataRequestsInterface.purchasedItemsResults(null);
                    GetPurchasedItemsEvent?.Invoke(null);
                    Debug.Log(www.error);
                }
                else {
                    // dataRequestsInterface.purchasedItemsResults(www.downloadHandler.text);
                    GetPurchasedItemsEvent?.Invoke(www.downloadHandler.text);
                    Debug.Log(www.downloadHandler.text);
                }
             }
        }

        internal IEnumerator FilterInventory(bool isRedeemed) {
             using (UnityWebRequest www = UnityWebRequest.Get(getServerApiUrl() + "inventory-item/?is_redeemed=" + isRedeemed)){
                www.SetRequestHeader("Authorization", "Bearer " + AccessToken);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) {
                    // dataRequestsInterface.redeemedItemsResults(null);
                    GetRedeemedItemsEvent?.Invoke(null);
                    Debug.Log(www.error);
                }
                else {
                    // dataRequestsInterface.redeemedItemsResults(www.downloadHandler.text);
                    GetRedeemedItemsEvent?.Invoke(www.downloadHandler.text);
                    Debug.Log(www.downloadHandler.text);
                }
             }
        }

        internal IEnumerator RedeemInventoryItem(int inventoryId) {
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
                // dataRequestsInterface.redeeemInventoryResult(null);
                RedeemItemEvent?.Invoke(null);
                Debug.Log(www.error);
            }
            else {
                // dataRequestsInterface.redeeemInventoryResult(www.downloadHandler.text);
                RedeemItemEvent?.Invoke(www.downloadHandler.text);
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
                // dataRequestsInterface.connectPlayerDataResult(null);
                SavePlayerDataEvent?.Invoke(null);
                Debug.Log(www.error);
            }
            else {
                // dataRequestsInterface.connectPlayerDataResult(www.downloadHandler.text);
                SavePlayerDataEvent?.Invoke(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.text);
            }
        }


        /* Save to a Binary file */
        private void SaveAuthData(string _accessToken,string _refreshToken,long _expireTime,string authfile = "/Tdata.dat")
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + authfile);
            AuthenticationData data = new AuthenticationData();
         
            data.at = _accessToken;
            data.rt = _refreshToken;
            data.et = _expireTime;

            bf.Serialize(file, data);
            file.Close();
        }

        /* Load from the Binary file */
        private void LoadAuthData(string authfile = "/Tdata.dat")
        {
            if(File.Exists(Application.persistentDataPath + authfile))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + authfile, FileMode.Open);
                AuthenticationData data = (AuthenticationData)bf.Deserialize(file);
                file.Close();
    
                AccessToken = data.at;
                RefreshToken = data.rt;
                Expiry = data.et;
            }
            else
            {
                AccessToken = null;
                RefreshToken = null;
                Expiry = 0;
            }
        }
    }


    /* Used to store auth data locally*/
    [System.Serializable]
    class AuthenticationData
    {
        public string at = "";
        public string rt = "";
        public long et = 0;
    }

    #if UNITY_ANDROID

    class AndroidPluginCallback : AndroidJavaProxy
    {
        private AuthenticationBehaviour mono;

        public AndroidPluginCallback(AuthenticationBehaviour mon) : base ("com.tamatem.auth.TamatemAuth$AuthorizationCallback") {
            mono = mon;
        }

        void onSuccess(string obj)
        {
            if(obj == null){
                return;
            }
            Debug.Log("Results retreived successfully!!");
            Debug.Log("Token retreived from Unity: " + obj);

            mono.AddJob(() => {
                var result = JObject.Parse(obj);
                mono.updateUserParameters(result);
                AuthenticationBehaviour.UserLoggedInEvent?.Invoke(result);
            });
        }

        void onFail()
        {
            Debug.Log("Failed to retreive token");
            mono.AddJob(() => {
                AuthenticationBehaviour.UserLoginFailedEvent?.Invoke();
            });
        }
    }

    #endif
}