using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapseButton : MonoBehaviour
{
    private GameObject uiContainer;
    private Canvas canvas;
    private  Vector3 defaultPos;

    private void Start()
    {
        uiContainer = GameObject.FindWithTag("UIContainer");
        canvas = transform.parent.gameObject.GetComponent<Canvas>();
        defaultPos = transform.position;
    }

    public void OnClick()
    {
        if (uiContainer.activeInHierarchy)
        {
            Vector3[] worldCorners = new Vector3[4];
            canvas.GetComponent<RectTransform>().GetWorldCorners(worldCorners);
            uiContainer.SetActive(false);
            transform.position = new Vector3(worldCorners[1].x, transform.position.y, transform.position.z);
            return;
        }

        transform.position = defaultPos;
        uiContainer.SetActive(true);
        return;
    }
}
