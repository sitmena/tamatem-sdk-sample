package com.tamatem.auth;

import okhttp3.OkHttpClient;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class DataClient {

    Retrofit getClient() {
        OkHttpClient client = new OkHttpClient.Builder().build();
        return new Retrofit.Builder().baseUrl(TamatemAuth.getInstance().getServerUrl())
                .addConverterFactory(GsonConverterFactory.create())
                .client(client)
                .build();
    }
}
