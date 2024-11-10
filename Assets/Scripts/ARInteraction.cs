using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARInteraction : MonoBehaviour
{
    private AndroidUdpManager androidUdpManager;
    void Start()
    {
        androidUdpManager = GetComponent<AndroidUdpManager>();
    }

    public void SendPhoto()
    {
        Texture2D photo = TakePhoto();
        androidUdpManager.SendImage(photo);
    }

    public Texture2D TakePhoto()
    {
        Camera camera = Camera.main;
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);

        var currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        camera.targetTexture = renderTexture;

        camera.Render();

        Texture2D photo = new Texture2D(renderTexture.width, renderTexture.height);
        photo.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        photo.Apply();

        camera.targetTexture = null;
        RenderTexture.active = currentRT;

        return photo;
    }

}
