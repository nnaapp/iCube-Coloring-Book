using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BookLib
{
    public class SliderInterface : MonoBehaviour
    {
        [SerializeField] private ImgTools img;
        [SerializeField] private PlayerController control;
        private Slider slider;

        private void Start()
        {
            if (!img)
                img = GameObject.FindWithTag("Image").GetComponent<ImgTools>();

            slider = GetComponent<Slider>();

            OnChange();
        }

        public void OnChange()
        {
            img.SetSize((int)slider.value);
            control.SetSize((int)slider.value);
        }
    }
}