using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelDetails : MonoBehaviour {
    public int ID;
    private Vector3 fixedPosition = Vector3.zero;
	// Use this for initialization
	void Start () {
        iTween.Init(gameObject);
        fixedPosition = transform.localPosition;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void Shake() {
        iTween.ShakeRotation(gameObject, new Vector3(0, 60, 0), 0.5f);
    }
    public void ReturnToFixedValues() {

        transform.localPosition = fixedPosition;
    }
    public void StoreFixedPosition() {
        fixedPosition = transform.localPosition;

    }
}
