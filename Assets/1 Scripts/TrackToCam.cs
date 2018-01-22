using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackToCam : MonoBehaviour
{
    public Transform cam;

    void Update ()
    {
        SetPosition();
    }

    public void SetPosition()
    {
        if (cam == null)
            return;

        transform.position = cam.position;
    }
}
