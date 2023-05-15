using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public Camera Cam;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.mouseScrollDelta.y < 0)
        {
            if (Cam.orthographicSize < 100)
                Cam.orthographicSize += 10;
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            if (Cam.orthographicSize > 10)
                Cam.orthographicSize -= 10;
        }
    }
}
