package com.icefoxz.parcelrider;

import android.content.Intent;
import android.util.Log;
import com.unity3d.player.UnityPlayerActivity;
import com.icefoxz.parcelrider.GalleryCameraHelper; // 添加此导入语句

public class CustomUnityPlayerActivity extends UnityPlayerActivity {
    // ...

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        
        // 添加日志记录
        //Log.d("CustomUnityPlayerActivity", "onActivityResult");
        //Log.d("CustomUnityPlayerActivity", "Request code: " + requestCode);
        //Log.d("CustomUnityPlayerActivity", "Result code: " + resultCode);
        //Log.d("CustomUnityPlayerActivity", "Intent data: " + (data != null ? data.toString() : "null"));

        GalleryCameraHelper.onActivityResult(requestCode, resultCode, data);
    }
}
