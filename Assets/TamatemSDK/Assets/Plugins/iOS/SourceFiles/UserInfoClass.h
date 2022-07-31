//
//  TempClass.h
//  KeyCSDK
//
//  Created by Bassam Mohtaseb on 25/06/2022.
//

#import <Foundation/Foundation.h>

@interface UserInfoClass : NSObject
+(void)_Authenticate:(char*)clientID: (char*)scheme: (char*)redirectURI: (bool)isDevelopment;
+(void)setDelegate:(id<UserInfoDelegate>)delegate;
@end
