using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.UI;

public class TestPicture : MonoBehaviour
{
    private PictureController PictureController { get; set; }
    [SerializeField] private MonoService _monoService;
    [SerializeField] private Image _image;
    void Start()
    {
        PictureController = new PictureController(_monoService);
    }

    public void TakePicture()
    {
        PictureController.OpenCamera(texture2D =>
        {
            _image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        });
    }

    public void TakePictureFromGallery()
    {
        PictureController.OpenGallery(texture2D =>
        {
            _image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        });
    }
}
