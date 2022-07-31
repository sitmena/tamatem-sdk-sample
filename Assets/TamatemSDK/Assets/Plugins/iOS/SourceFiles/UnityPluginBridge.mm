//
//  UnityPluginBridge.m
//  KeyCSDK
//
//  Created by sanad barjawi on 15/05/2022.
//

#import <Foundation/Foundation.h>
#import <AuthenticationServices/ASWebAuthenticationSession.h>
#import <KeyCSDK/KeyCSDK-Swift.h>
#import "UserInfoClass.h"
#import "UnityPluginBridge.h"

DelegateCallbackFunction delegate = NULL;
@interface UnityPluginBridge : NSObject<UserInfoDelegate>
@end
static UnityPluginBridge *__delegate = nil;

char* convertNSStringToCString(const NSString* nsString)
{
    if (nsString == NULL)
        return NULL;

    const char* nsStringUtf8 = [nsString UTF8String];
    //create a null terminated C string on the heap so that our string's memory isn't wiped out right after method's return
    char* cString = (char*)malloc(strlen(nsStringUtf8) + 1);
    strcpy(cString, nsStringUtf8);

    return cString;
}

void framework_Authenticate(char* clientID, char* scheme, char* redirectURI, bool isDevelopment) {
    [UserInfoClass _Authenticate:(char *)clientID :(char *)scheme :(char *)redirectURI :(bool)isDevelopment];
}

void framework_setDelegate(DelegateCallbackFunction callback) {
    if (!__delegate) {
        __delegate = [[UnityPluginBridge alloc] init];
    }
    [UserInfoClass setDelegate:__delegate];
    [[UnityPlugin shared] setCallbackWithDelegate:__delegate];
    
    delegate = callback;
}
@implementation UnityPluginBridge
- (void)onSuccessWithTokenModel:(NSString*)tokenModel {
    if (delegate != NULL) {
        delegate(convertNSStringToCString(tokenModel));
    }
}
@end
