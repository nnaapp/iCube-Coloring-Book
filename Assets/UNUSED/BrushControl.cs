using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrushControl : MonoBehaviour
{
    private Vector3 mousePos;
    private Vector3 mousePosPrev;
    private GameObject brush;
    private SpriteRenderer sprender;
    private LineRenderer lrender;
    [SerializeField] private RawImage img;
    [SerializeField] private DrawingCamera drawingCam;
    [SerializeField] private PlayerController control;
    [SerializeField] private Camera cam;
    bool blockMouse;
    void Start()
    {
        brush = gameObject;
        sprender = GetComponent<SpriteRenderer>();
        lrender = GetComponent<LineRenderer>();
        lrender.positionCount = 2;
        lrender.useWorldSpace = true;
        mousePos = Vector3.zero;
        mousePosPrev = Vector3.zero;
        blockMouse = false;
    }
    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 posDiff = new Vector2(Mathf.Abs(mousePos.x - mousePosPrev.x), Mathf.Abs(mousePos.y - mousePosPrev.y));
        RaycastHit2D hit = Physics2D.Raycast(mousePos, posDiff, 0.00001f, LayerMask.GetMask("Static UI"));

        if (Input.GetMouseButtonDown(0) && hit)
        {
            sprender.enabled = false;
            blockMouse = true;
        }
        /*else if (Input.GetMouseButton(0) && !blockMouse)
        {
            brush.transform.position = mousePos;
            float brushScale = control.GetBrushSize();
            brush.transform.localScale = new Vector2(brushScale, brushScale);
            sprender.color = control.GetColor();
            lrender.startColor = control.GetColor();
            lrender.endColor = control.GetColor();
            lrender.startWidth = lrender.endWidth = control.GetLineSize();
            sprender.enabled = true;
            lrender.SetPosition(0, mousePosPrev);
            lrender.SetPosition(1, mousePos);
        }
        else
        {
            sprender.enabled = false;
        }*/

        if (Input.GetMouseButtonUp(0) && blockMouse)
        {
            blockMouse = false;
        }

        mousePosPrev = mousePos;
    }
}
