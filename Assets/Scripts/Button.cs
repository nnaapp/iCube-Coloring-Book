using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BookLib
{
    public class Button : MonoBehaviour
    {
        [SerializeField] private PlayerController control;
        [SerializeField] private ImgTools img;
        //private SpriteRenderer sprender;
        private Image buttonImg;
        [SerializeField] private int sizeValue;
        [SerializeField] private Color colorValue;
        [SerializeField] private int alphaValue;
        public enum ButtonType
        {
            Color,
            Alpha,
            Size
        }
        [SerializeField] private ButtonType buttonType;

        private void Start()
        {
            if (!control)
                control = GameObject.FindGameObjectWithTag("PlayerController").GetComponent<PlayerController>();
            if (!control)
                Debug.Log("Player Controller not assigned and could not be found automatically, check inspector for irregular behavior.");
            if (!img)
                img = GameObject.FindWithTag("Image").GetComponent<ImgTools>();

            //sprender = GetComponent<SpriteRenderer>();
            buttonImg = GetComponent<Image>();
            switch (buttonType)
            {
                case ButtonType.Color:
                    buttonImg.color = colorValue;
                    break;
                case ButtonType.Size:
                    buttonImg.color = Color.gray;
                    break;
                case ButtonType.Alpha:
                    buttonImg.color = Color.gray;
                    break;
                default:
                    buttonImg.color = Color.gray;
                    break;
            }
        }

        public void OnClick()
        {
            Debug.Log("test");
            try
            {
                switch (buttonType)
                {
                    case ButtonType.Color:
                        Debug.Log(colorValue + " color button pressed");
                        UpdateColor();
                        break;
                    case ButtonType.Size:
                        Debug.Log(sizeValue + " size button pressed");
                        UpdateSize();
                        break;
                    case ButtonType.Alpha:
                        Debug.Log(alphaValue + " alpha button pressed");
                        UpdateAlpha();
                        break;
                    default:
                        throw new ButtonTypeNotFoundException("No type found.");
                }
            }
            catch (ButtonTypeNotFoundException e)
            {
                Debug.LogError(e + " : " + e.Message);
                Debug.LogError("Button type not found, check heirarchy for unexpected behavior.");
            }
        }

        private void UpdateColor()
        {
            control.SetColor(colorValue);
            img.SetSelectedColor(colorValue);
        }

        private void UpdateSize()
        {
            control.SetSize(sizeValue);
            img.SetSize(sizeValue);
        }

        private void UpdateAlpha()
        {
            control.SetAlpha(alphaValue);
            img.SetAlpha(alphaValue);
            img.SetSelectedColor(img.GetSelectedColor());
        }

        public ButtonType GetButtonType()
        {
            return buttonType;
        }

        public Color32 GetColor()
        {
            return colorValue;
        }

        public float GetSize()
        {
            return sizeValue;
        }
    }
}