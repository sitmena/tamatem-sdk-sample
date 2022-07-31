//
//  UnityPlugin.swift
//  KeyCSDK
//
//  Created by sanad barjawi on 15/05/2022.
//

//exposing function to the .mm file
import Foundation
import KeyCSDK
@objc public protocol UserInfoDelegate
{
    func onSuccess(tokenModel: NSString?)
}
@objc final public class UnityPlugin : NSObject {
    
    @objc public static var shared = UnityPlugin()
    private var authManager: AuthPOCManager = AuthPOCManager()
    private var userInfoDel: UserInfoDelegate?
    
    @objc public func setCallback(delegate: UserInfoDelegate) {
        self.userInfoDel = delegate;
    }
    
    @objc public func authenticate(clientID: String,
                                   scheme: String,
                                   redirectURI: String,
                                   isDevelopment: Bool) {
        authManager.auth(clientID: clientID, scheme: scheme, redirectURI: redirectURI, isDevelopment: isDevelopment) { [weak self] result in
            guard let `self` = self else {
                return
            }
            switch result {
            case .success(let tokenModel):
                self.userInfoDel?.onSuccess(tokenModel: String(decoding: tokenModel, as: UTF8.self) as NSString)
            case .failure(_):
                self.userInfoDel?.onSuccess(tokenModel: nil)
            }
        }
    }
}
