using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    private Transform camera;

    void Start ()
    {
        camera = Camera.main.transform;
        transform.LookAt(camera.position);
    }

    void Update ()
    {
        transform.LookAt(camera.position);
	}
}
