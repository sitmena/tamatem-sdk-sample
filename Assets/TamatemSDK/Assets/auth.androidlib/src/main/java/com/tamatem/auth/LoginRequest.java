package com.tamatem.auth;

import com.google.gson.annotations.SerializedName;

import java.io.Serializable;

public class LoginRequest implements Serializable {
    @SerializedName("grant_type")
    private String grantType;
    private String code;
    @SerializedName("code_verifier")
    private String codeVerifier;
    @SerializedName("client_id")
    private String clientId;
    @SerializedName("redirect_uri")
    private String redirectUri;
    @SerializedName("response_type")
    private String responseType;
    private String scope;

    public LoginRequest(String grantType, String code, String codeVerifier, String clientId, String redirectUri, String responseType, String scope) {
        this.grantType = grantType;
        this.code = code;
        this.codeVerifier = codeVerifier;
        this.clientId = clientId;
        this.redirectUri = redirectUri;
        this.responseType = responseType;
        this.scope = scope;
    }
}
