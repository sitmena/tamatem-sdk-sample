package com.tamatem.auth;

import androidx.annotation.NonNull;

import com.google.gson.Gson;
import com.google.gson.internal.LinkedTreeMap;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ApiController {

    void login(String code, String redirectUri, String codeVerifier, TamatemAuth.AuthorizationCallback authorizationCallback) {

        LoginRequest loginRequest = new LoginRequest("authorization_code", code, codeVerifier,
                TamatemAuth.getInstance().clientId, redirectUri,
                "code", "customer:read");
        Call<Object> callObject = new DataClient().getClient().create(LoginService.class)
                .attemptLogin(loginRequest);
        callObject.enqueue(new Callback<Object>() {
            @Override
            public void onResponse(@NonNull Call<Object> call, @NonNull Response<Object> response) {
                if (response.isSuccessful()) {
                    String results = new Gson().toJson(((LinkedTreeMap) response.body()).get("results"));
                    authorizationCallback.onSuccess(results);
                    return;
                }
                authorizationCallback.onFail();
            }

            @Override
            public void onFailure(@NonNull Call<Object> call, @NonNull Throwable throwable) {
                authorizationCallback.onFail();
            }
        });
    }
}
