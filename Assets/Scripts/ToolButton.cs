using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BookLib
{
    public class ToolButton : MonoBehaviour
    {
        [SerializeField] private ImgTools.Tools toolType;
        [SerializeField] private ImgTools img;
        private Image buttonTex;

        private void Start()
        {
            if (!img)
                img = GameObject.FindWithTag("Image").GetComponent<ImgTools>();

            buttonTex = gameObject.GetComponent<Image>();
        }

        private void Update()
        {
            if (img.GetTool() == toolType)
                buttonTex.color = new Color32(255, 0, 0, 255);
            else
                buttonTex.color = new Color32(255, 255, 255, 255);
        }

        public void OnClick()
        {
            img.SetTool(toolType);
        }
    }
}