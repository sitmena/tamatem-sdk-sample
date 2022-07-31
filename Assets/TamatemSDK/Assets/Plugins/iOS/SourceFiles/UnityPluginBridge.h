//
//  UnityPluginBridge.h
//  KeyCSDK
//
//  Created by Bassam Mohtaseb on 26/06/2022.
//

#ifdef __cplusplus
extern "C" {
#endif

    void framework_Authenticate(char* clientID, char* scheme, char* redirectURI, bool isDevelopment);
    typedef void (*DelegateCallbackFunction)(char* tokenModel);
    void framework_setDelegate(DelegateCallbackFunction callback);
    
#ifdef __cplusplus
}
#endif
