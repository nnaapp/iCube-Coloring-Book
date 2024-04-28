using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorIndicator : MonoBehaviour
{
    [SerializeField] private BookLib.ImgTools img;

    void Start()
    {
        if (!img)
            img = GameObject.FindWithTag("Image").GetComponent<BookLib.ImgTools>();
    }

    public void SetColor(Color32 color)
    {
        gameObject.GetComponent<Image>().color = color;
    }
}
