package com.icefoxz.letsmove;

import android.content.ContentUris;
import android.content.Context;
import android.os.Build;
import android.os.Environment;
import android.provider.DocumentsContract;
import android.app.Activity;
import android.content.ContentValues;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.provider.MediaStore;
import com.unity3d.player.UnityPlayer;
import android.util.Log;

public class GalleryCameraHelper {

    private static Uri imageUri;

    public static void openCamera() {
        Activity activity = UnityPlayer.currentActivity;
        Intent intent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
        if (intent.resolveActivity(activity.getPackageManager()) != null) {
            ContentValues values = new ContentValues();
            values.put(MediaStore.Images.Media.TITLE, "New Picture");
            values.put(MediaStore.Images.Media.DESCRIPTION, "From your Camera");
            imageUri = activity.getContentResolver().insert(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, values);
            intent.putExtra(MediaStore.EXTRA_OUTPUT, imageUri);
            activity.startActivityForResult(intent, 100);
        }
    }

    // public static void openGallery() {
    //     Activity activity = UnityPlayer.currentActivity;
    //     Intent intent = new Intent(Intent.ACTION_PICK, MediaStore.Images.Media.EXTERNAL_CONTENT_URI);
    //     activity.startActivityForResult(intent, 101);
    // }
    public static void openGallery() {
        Activity activity = UnityPlayer.currentActivity;
        Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
        intent.setType("image/*");
        activity.startActivityForResult(intent, 101);
    }

    public static void onActivityResult(int requestCode, int resultCode, Intent data) {
        // Log.d("GalleryCameraHelper", "onActivityResult");
        // Log.d("GalleryCameraHelper", "Request code: " + requestCode);
        // Log.d("GalleryCameraHelper", "Result code: " + resultCode);
        // Log.d("GalleryCameraHelper", "Intent data: " + (data != null ? data.toString() : "null"));

        if (requestCode == 100 && resultCode == Activity.RESULT_OK) {
            String imagePath = "";
            if (imageUri != null) {
                String[] filePathColumn = {MediaStore.Images.Media.DATA};
                imagePath = getImagePath(UnityPlayer.currentActivity, imageUri, filePathColumn);
                // ...
                Log.d("GalleryCameraHelper", "Image path: " + imagePath);
            }
            UnityPlayer.UnitySendMessage("MonoService", "OnImagePathReceived", imagePath);
        } else if (requestCode == 101 && resultCode == Activity.RESULT_OK && data != null) {
            Uri selectedImage = data.getData();
            String[] filePathColumn = {MediaStore.Images.Media.DATA};
            String imagePath = "";

            if (selectedImage != null) {
                imagePath = getImagePath(UnityPlayer.currentActivity, selectedImage, filePathColumn);
            }
            // ...
            Log.d("GalleryCameraHelper", "Image path: " + imagePath);
            UnityPlayer.UnitySendMessage("MonoService", "OnImagePathReceived", imagePath);
        }
    }

    // private static String getImagePath(Activity activity, Uri selectedImage, String[] filePathColumn) {
    //     String imagePath = "";
    //     Cursor cursor = activity.getContentResolver().query(selectedImage, filePathColumn, null, null, null);
    //     if (cursor != null) {
    //         cursor.moveToFirst();
    //         int columnIndex = cursor.getColumnIndex(filePathColumn[0]);
    //         imagePath = cursor.getString(columnIndex);
    //         cursor.close();
    //     }
    //     // ...
    //     // Log.d("GalleryCameraHelper", "Image path: " + imagePath);
    //     return imagePath;
    // }
    private static String getImagePath(Activity activity, Uri selectedImage, String[] filePathColumn) {
        String imagePath = "";
        boolean isKitKat = Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT;
        
        if (isKitKat && DocumentsContract.isDocumentUri(activity, selectedImage)) {
            if ("com.android.externalstorage.documents".equals(selectedImage.getAuthority())) {
                String[] split = DocumentsContract.getDocumentId(selectedImage).split(":");
                String type = split[0];
                
                if ("primary".equalsIgnoreCase(type)) {
                    imagePath = Environment.getExternalStorageDirectory() + "/" + split[1];
                }
            } else if ("com.android.providers.downloads.documents".equals(selectedImage.getAuthority())) {
                Uri contentUri = ContentUris.withAppendedId(
                        Uri.parse("content://downloads/public_downloads"), Long.valueOf(DocumentsContract.getDocumentId(selectedImage)));
                imagePath = getDataColumn(activity, contentUri, null, null);
            } else if ("com.android.providers.media.documents".equals(selectedImage.getAuthority())) {
                String[] split = DocumentsContract.getDocumentId(selectedImage).split(":");
                String type = split[0];
                
                Uri contentUri = null;
                if ("image".equals(type)) {
                    contentUri = MediaStore.Images.Media.EXTERNAL_CONTENT_URI;
                }
                
                String selection = "_id=?";
                String[] selectionArgs = new String[] {split[1]};
                
                imagePath = getDataColumn(activity, contentUri, selection, selectionArgs);
            }
        } else if ("content".equalsIgnoreCase(selectedImage.getScheme())) {
            imagePath = getDataColumn(activity, selectedImage, null, null);
        } else if ("file".equalsIgnoreCase(selectedImage.getScheme())) {
            imagePath = selectedImage.getPath();
        }
        
        return imagePath;
    }
    
    private static String getDataColumn(Context context, Uri uri, String selection, String[] selectionArgs) {
        Cursor cursor = null;
        String column = MediaStore.Images.Media.DATA;
        String[] projection = {column};
    
        try {
            cursor = context.getContentResolver().query(uri, projection, selection, selectionArgs, null);
            if (cursor != null && cursor.moveToFirst()) {
                int columnIndex = cursor.getColumnIndexOrThrow(column);
                return cursor.getString(columnIndex);
            }
        } finally {
            if (cursor != null) {
                cursor.close();
            }
        }
        return null;
    }    
}