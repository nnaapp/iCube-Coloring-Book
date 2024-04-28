using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextButton : MonoBehaviour
{
    [SerializeField] private Direction direction = Direction.Forward;
    [SerializeField] private BookLib.ImgTools img;

    enum Direction
    { 
        Forward,
        Backward
    }

    void Start()
    {
        if (!img)
            img = GameObject.FindWithTag("Image").GetComponent<BookLib.ImgTools>();
    }

    public void OnClick()
    {
        if (direction == Direction.Forward)
            img.NextStencil();
        else if (direction == Direction.Backward)
            img.PreviousStencil();
    }
}
