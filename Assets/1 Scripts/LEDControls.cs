using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LEDControls : MonoBehaviour {

    public GameObject LED1;
    public GameObject LED2;
    public GameObject LED3;
    public GameObject LED4;
    private bool isHighlighted = false;
    public Vector3 position;

    LEDControls(double posX, double posY, double posZ) {
        position = new Vector3((float)posX, (float)posY, (float)posZ);
        transform.Translate(position);
    }
  

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void Move(double posX, double posY, double posZ) {
        position = new Vector3((float)-posY, (float)posZ, (float)posX);
        transform.Translate(2.07f * position);
        transform.LookAt(Vector3.zero);
    }
    public void HighlightLEDs(bool LED1ON, bool LED2ON, bool LED3ON, bool LED4ON,bool manualSelection = false)
    {
        if (manualSelection)
        {
            if (LED1ON || LED2ON || LED3ON || LED4ON)
                isHighlighted = true;
            else
                isHighlighted = false;


            if (LED1ON)
            {
                LED1.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                LED1.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
            }
            else
                LED1.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.1f, 0, 0));
            if (LED2ON)
                LED2.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
            else
                LED2.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.1f, 0, 0));
            if (LED3ON)
                LED3.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
            else
                LED3.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.1f, 0, 0));
            if (LED4ON)
                LED4.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
            else
                LED4.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.1f, 0, 0));

        }
        else {

            if (isHighlighted)
                return;
            if (LED1ON)
            {
                LED1.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                LED1.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
            }
            else
                LED1.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.1f, 0, 0));
            if (LED2ON)
                LED2.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
            else
                LED2.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.1f, 0, 0));
            if (LED3ON)
                LED3.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
            else
                LED3.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.1f, 0, 0));
            if (LED4ON)
                LED4.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
            else
                LED4.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.1f, 0, 0));

        }

        }
    
        
    
}
