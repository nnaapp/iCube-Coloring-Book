using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderTest : MonoBehaviour
{
    [SerializeField] ComputeShader _drawComputeShader;
    [SerializeField] RenderTexture _canvasRenderTexture;

    void Start()
    {
        _canvasRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        _canvasRenderTexture.filterMode = FilterMode.Point;
        _canvasRenderTexture.enableRandomWrite = true;
        _canvasRenderTexture.Create();

        int initBackgroundKernel = _drawComputeShader.FindKernel("InitBackground");
        _drawComputeShader.SetTexture(initBackgroundKernel, "_Canvas", _canvasRenderTexture);
        _drawComputeShader.Dispatch(initBackgroundKernel, _canvasRenderTexture.width / 8,
            _canvasRenderTexture.height / 8, 1);
    }

    void Update()
    {
        int updateKernel = _drawComputeShader.FindKernel("Update");

        _drawComputeShader.SetBool("_MouseDown", Input.GetMouseButton(0));
        _drawComputeShader.SetFloats("_MousePos", Input.mousePosition.x, Input.mousePosition.y);
        _drawComputeShader.SetFloat("_BrushSize", 10);

        _drawComputeShader.SetTexture(updateKernel, "_Canvas", _canvasRenderTexture);
        _drawComputeShader.Dispatch(updateKernel, _canvasRenderTexture.width / 8,
            _canvasRenderTexture.height / 8, 1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_canvasRenderTexture, dest);
    } 
}
