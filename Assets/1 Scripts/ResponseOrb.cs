using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponseOrb : MonoBehaviour {
    public int ID;
	// Use this for initialization
	void Start () {
        iTween.Init(gameObject);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void Shake()
    {
        iTween.ShakeScale(gameObject, new Vector3(.2f, .2f, .2f), 0.5f);
    }
}
