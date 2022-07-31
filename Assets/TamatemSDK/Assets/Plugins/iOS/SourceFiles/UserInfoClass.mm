//
//  TempClass.m
//  KeyCSDK
//
//  Created by Bassam Mohtaseb on 25/06/2022.
//

#import <Foundation/Foundation.h>
#import <AuthenticationServices/ASWebAuthenticationSession.h>
#import <KeyCSDK/KeyCSDK-Swift.h>
#import "UserInfoClass.h"
#import "UnityPlugin-Bridging-Header.h"

@implementation UserInfoClass

id __delegate = nil;

+(void)_Authenticate:(char*)clientID: (char*)scheme: (char*)redirectURI: (bool)isDevelopment
{
    [[UnityPlugin shared ] authenticateWithClientID:[NSString stringWithUTF8String: clientID]
                                             scheme:[NSString stringWithUTF8String: scheme]
                                        redirectURI:[NSString stringWithUTF8String: redirectURI]
                                        isDevelopment: isDevelopment];
}
+(void)setDelegate:(id<UserInfoDelegate>)delegate {
    __delegate = delegate;
}

@end
