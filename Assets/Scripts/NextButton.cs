using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextButton : MonoBehaviour
{
    [SerializeField] private BookLib.ImgTools img;
    void Start()
    {
        if (!img)
            img = GameObject.FindWithTag("Image").GetComponent<BookLib.ImgTools>();
    }

    public void OnClick()
    {
        img.NextStencil();
    }
}
