using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Color32 color;
    [SerializeField] private float size;
    [SerializeField] private int alpha;
    private const float LINE_BASE = 3500f;
    private const float BRUSH_BASE = 125f;
    private const float LINE_DIVISOR = 10000f;
    private const float BRUSH_DIVISOR = 10000f;
    private const float LINE_SIZE_OFFSET = 0.78f;

    void Awake()
    {
        //color = GameObject.FindGameObjectWithTag("DefaultColor").GetComponent<Button>().GetColor();
        //size = GameObject.FindGameObjectWithTag("DefaultSize").GetComponent<Button>().GetSize();
        color = Color.black;
        alpha = 255;
        size = 20;
    }

    public void SetColor(Color32 c)
    {
        color = c;
    }

    public void SetSize(int s)
    {
        size = s;
    }

    public void SetAlpha(int a)
    {
        alpha = a;
    }

    public Color32 GetColor()
    {
        return color;
    }

    public float GetLineSize()
    {
        return (LINE_BASE / LINE_DIVISOR) * (size * LINE_SIZE_OFFSET / 1000f);
    }

    public float GetBrushSize()
    {
        return size;
    }

    public int GetAlpha()
    {
        return alpha;
    }
}