using System;
using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.Android;

public class PictureController : ControllerBase
{
    private const string JavaGallerycamerahelperClass = "com.icefoxz.parcelrider.GalleryCameraHelper";
    private Action<Texture2D> _onPictureTaken;
    private MonoService MonoService { get; }

    public PictureController(MonoService monoService)
    {
        MonoService = monoService;
        MonoService.OnPictureTaken.AddListener(OnImagePathReceived);
    }

    public void OpenCamera(Action<Texture2D> onPictureTakenAction)
    {
        _onPictureTaken = onPictureTakenAction;
        if (Application.platform == RuntimePlatform.Android)
        {
            MonoService.StartCoroutine(OpenCameraAfterPermission());
        }
        else
        {
            Debug.LogError("OpenCamera is not supported on this platform.");
        }
    }

    private IEnumerator OpenCameraAfterPermission()
    {
        yield return RequestCameraPermission();
        using (var galleryCameraHelper = new AndroidJavaClass(JavaGallerycamerahelperClass))
        {
            galleryCameraHelper.CallStatic("openCamera");
        }
    }

    public void OpenGallery(Action<Texture2D> onPictureTakenAction)
    {
        _onPictureTaken = onPictureTakenAction;
        if (Application.platform == RuntimePlatform.Android)
        {
            MonoService.StartCoroutine(OpenGalleryAfterPermission());
        }
        else
        {
            Debug.LogError("OpenGallery is not supported on this platform.");
        }
    }

    private IEnumerator OpenGalleryAfterPermission()
    {
        yield return RequestGalleryPermission();
        using (var galleryCameraHelper = new AndroidJavaClass(JavaGallerycamerahelperClass))
        {
            galleryCameraHelper.CallStatic("openGallery");
        }
    }

    public void OnImagePathReceived(string imagePath)
    {
        if (!string.IsNullOrEmpty(imagePath))
        {
            MonoService.StartCoroutine(LoadImageFromPath(imagePath));
        }
        else
        {
            Debug.LogError("Image path is null or empty.");
        }
    }

    private IEnumerator LoadImageFromPath(string imagePath)
    {
        using (WWW www = new WWW("file://" + imagePath))
        {
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                Texture2D texture = new Texture2D(2, 2);
                www.LoadImageIntoTexture(texture);

                // 在这里处理获得的图像纹理
                _onPictureTaken?.Invoke(texture);
                Debug.Log("Image loaded successfully.");
            }
            else
            {
                Debug.LogError("Failed to load the image: " + www.error);
            }
        }
    }

    private IEnumerator RequestCameraPermission()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Camera)) yield break;
        Permission.RequestUserPermission(Permission.Camera);
        while (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            yield return null;
        }
    }

    private IEnumerator RequestGalleryPermission()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead)) yield break;
        Permission.RequestUserPermission(Permission.ExternalStorageRead);
        while (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            yield return null;
        }
    }
}
