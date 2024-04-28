using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevertButton : MonoBehaviour
{
    [SerializeField] private RevType type = RevType.Redo;
    [SerializeField] private BookLib.ImgTools img;

    enum RevType
    {
        Undo,
        Redo
    }
    void Start()
    {
        if (!img)
        {
            img = GameObject.FindWithTag("Image").GetComponent<BookLib.ImgTools>();
        }
    }

    public void OnClick()
    {
        if (type == RevType.Undo) 
            img.Undo(); 
        else if (type == RevType.Redo)
            img.Redo();
    }
}
