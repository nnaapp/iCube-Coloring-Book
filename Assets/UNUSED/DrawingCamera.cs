using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawingCamera : MonoBehaviour
{

    [SerializeField] private Camera drawingCamera;
    [SerializeField] private RawImage overlay;
    [SerializeField] private PlayerController control;
    /*private Texture2D workableTexture;
    private Color32[] map;
    private int imgWidth;
    private int imgHeight;*/

    void Start()
    {
        StartCoroutine(SetupRoutine(1));
    }

    private IEnumerator SetupRoutine(int frames)
    {
        CustomRenderTexture newRender = new CustomRenderTexture(Screen.width, Screen.height, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
        //newRender.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None;
        newRender.filterMode = FilterMode.Point;
        drawingCamera.targetTexture = newRender;
        overlay.texture = drawingCamera.targetTexture;

        drawingCamera.clearFlags = CameraClearFlags.SolidColor;

        yield return new WaitForEndOfFrame();

        drawingCamera.clearFlags = CameraClearFlags.Nothing;

        /*yield return new WaitForEndOfFrame();

        imgWidth = overlay.mainTexture.width;
        imgHeight = overlay.mainTexture.height;

        workableTexture = new Texture2D(imgWidth, imgHeight, TextureFormat.RGBA32, false);
        workableTexture.ReadPixels(new Rect(0, 0, imgWidth, imgHeight), 0, 0);

        map = workableTexture.GetPixels32();*/
    }
}
