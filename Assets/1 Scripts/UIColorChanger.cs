using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIColorChanger : MonoBehaviour {
    public Color newColor;
    private Color baseColor;
	// Use this for initialization
	void Start () {
        baseColor = GetComponent<Image>().color;
	}

    public void ChangeToNewColor() {
        GetComponent<Image>().color = newColor;
        
    }
    public void ChangeToBaseColor() {
        GetComponent<Image>().color = baseColor;

    }

}
