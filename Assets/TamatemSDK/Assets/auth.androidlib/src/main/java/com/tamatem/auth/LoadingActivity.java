package com.tamatem.auth;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;

import androidx.browser.customtabs.CustomTabsIntent;
import androidx.constraintlayout.widget.ConstraintLayout;

import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Base64;
import java.util.regex.Pattern;

public class LoadingActivity extends Activity {

    private ProgressBar progressBar;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        ConstraintLayout container = new ConstraintLayout(this);
        container.setLayoutParams(new ConstraintLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT));
        progressBar = new ProgressBar(this);
        ConstraintLayout.LayoutParams progressParams = new ConstraintLayout.LayoutParams(
                ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        progressParams.startToStart = ConstraintLayout.LayoutParams.PARENT_ID;
        progressParams.endToEnd = ConstraintLayout.LayoutParams.PARENT_ID;
        progressParams.topToTop = ConstraintLayout.LayoutParams.PARENT_ID;
        progressParams.bottomToBottom = ConstraintLayout.LayoutParams.PARENT_ID;
        container.addView(progressBar, progressParams);

        setContentView(container);
        progressBar.setVisibility(View.GONE);

        TamatemAuth tamatemAuth = TamatemAuth.getInstance();
        String cypheredString = null;
        try {
            cypheredString = Base64.getUrlEncoder().encodeToString(
                            MessageDigest.getInstance("SHA-256").
                                    digest(tamatemAuth.codeVerifier.getBytes(StandardCharsets.UTF_8))).
                    replace("=", "");
        } catch (NoSuchAlgorithmException e) {
            e.printStackTrace();
        }
        String authURLStr =
                TamatemAuth.getInstance().getServerUrl() + "authorize?client_id=" + tamatemAuth.clientId +
                        "&response_type=code&redirect_uri=" + tamatemAuth.redirectUri +
                        "&code_challenge=" + cypheredString +
                        "&code_challenge_method=S256";
        CustomTabsIntent.Builder builder = new CustomTabsIntent.Builder();
        CustomTabsIntent customTabsIntent = builder.setUrlBarHidingEnabled(false).build();
        customTabsIntent.launchUrl(this, Uri.parse(authURLStr));
    }

    @Override
    public void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        handleLoginIntent(intent);
    }

    private void handleLoginIntent(Intent intent) {
        Uri data = intent.getData();
        if (data != null) {
            progressBar.setVisibility(View.VISIBLE);
            String code = data.getQueryParameter("code");
            String redirectUri = data.toString().split(Pattern.quote("?"))[0];
            new ApiController().login(code, redirectUri, TamatemAuth.getInstance().codeVerifier,
                    new TamatemAuth.AuthorizationCallback() {
                        @Override
                        public void onSuccess(String results) {
                            TamatemAuth.AuthorizationCallback authorizationCallback = TamatemAuth.getInstance().authorizationCallback;
                            if (authorizationCallback != null) {
                                authorizationCallback.onSuccess(results);
                            }
                            finish();
                        }

                        @Override
                        public void onFail() {
                            TamatemAuth.AuthorizationCallback authorizationCallback = TamatemAuth.getInstance().authorizationCallback;
                            if (authorizationCallback != null) {
                                authorizationCallback.onFail();
                            }
                            finish();
                        }
                    });
        }
    }
}
