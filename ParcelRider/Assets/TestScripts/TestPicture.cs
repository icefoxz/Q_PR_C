using System;
using System.IO;
using System.Net.Http;
using AOT.Controllers;
using AOT.Core;
using AOT.Utl;
using UnityEngine;
using UnityEngine.UI;

public class TestPicture : MonoBehaviour
{
    private ImageController ImageController { get; set; }
    [SerializeField] private MonoService _monoService;
    [SerializeField] private Image _image;
    [SerializeField] private string _uploadImageApi;
    void Start()
    {
        ImageController = new ImageController(_monoService);
    }

    public void TakePicture()
    {
        ImageController.OpenCamera(texture2D =>
        {
            _image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        });
    }

    public void TakePictureFromGallery()
    {
        ImageController.OpenGallery(texture2D =>
        {
            _image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        });
    }

    public async void UploadImage()
    {
        Debug.Log("Uploading image...");
        var sourceTexture = _image.sprite.texture;

        // Create a new texture with a format that can be encoded.
        var newTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);

        // Get the pixel data from the compressed texture and assign it to the new texture.
        newTexture.SetPixels(sourceTexture.GetPixels());
        newTexture.Apply();

        // Now you can encode the new texture to JPG or PNG.
        var jpgData = newTexture.EncodeToJPG();
        var content = new StreamContent(new MemoryStream(jpgData));
        var response = await Http.SendAsync(new Uri(_uploadImageApi), HttpMethod.Post, content);
        Debug.Log(response.isSuccess ? "Image Up success!" : "Image Up error!");
    }
}
