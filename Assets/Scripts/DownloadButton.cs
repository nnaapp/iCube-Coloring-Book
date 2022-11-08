using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BookLib
{
    public class DownloadButton : MonoBehaviour
    {
        [SerializeField] private ImgTools img;

        void Start()
        {
            if (!img)
                img = GameObject.FindWithTag("Image").GetComponent<ImgTools>();;
        }

        public void OnClick()
        {
            img.DownloadImage();
        }
    }
}