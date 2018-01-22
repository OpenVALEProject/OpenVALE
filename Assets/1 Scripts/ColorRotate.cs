using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorRotate : MonoBehaviour {

    public Material mat;
    public float speed = 1.0f;
    public float min = 0.0f;
    public float max = 1.0f;

    private bool direction;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        Color col = mat.GetColor("_EmissionColor");

        if (col.r >= max)
            direction = false;
        else if (col.r <= min)
            direction = true;

        col.r = (direction == true ? col.r + 0.01f * speed : col.r - 0.01f * speed);
        mat.SetColor("_EmissionColor", col);
    }
}
