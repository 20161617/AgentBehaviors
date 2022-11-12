using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{

    public Camera Camera;

    private Vector2 mousePos;

    // Start is called before the first frame update
    void Start()
    {
        Camera = GameObject.Find("Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {    
            mousePos = Camera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePos;
        }
    }
}
