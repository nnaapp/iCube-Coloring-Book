using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeRender : MonoBehaviour
{
    private LineRenderer lrender;
    //[SerializeField] private PlayerController control;
    [SerializeField] private Transform LeftAnchor;
    [SerializeField] private Transform RightAnchor;
    [SerializeField] private int LineSize;
    void Start()
    {
        lrender = this.GetComponent<LineRenderer>();

        /*if (control == null)
            control = GameObject.FindGameObjectWithTag("PlayerController").GetComponent<PlayerController>();
            if (control == null)
                Debug.Log("Player Controller not assigned and could not be found automatically, check inspector for irregular behavior.");
        */
        lrender.startColor = lrender.endColor = Color.black;
        lrender.startWidth = LineSize / 10000f;
        lrender.endWidth = LineSize / 10000f;
        lrender.positionCount = 2;
        lrender.useWorldSpace = true;
    }

    void Update()
    {
        lrender.SetPosition(0, LeftAnchor.position);
        lrender.SetPosition(1, RightAnchor.position);
    }
}