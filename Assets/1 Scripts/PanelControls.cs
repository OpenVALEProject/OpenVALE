using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PanelControls : MonoBehaviour {
    public List<GameObject> facePanels;
    private List<Vector3> panelPositions = new List<Vector3>();
    public float degreesSeperation = 15.0f;
    public float panelDistance = 2.07f;
    private List<Vector3> possibleRingLocations= new List<Vector3>();
    public GameObject rotator;
    private Camera mainCamera;
	// Use this for initialization
	void Start () {
        foreach (GameObject g in facePanels) {
            panelPositions.Add(g.transform.position);
        }
        
        mainCamera = Camera.main;
	}
    void Update()
    {
        rotator.transform.position = new Vector3(0f, -.08f,0f);
        rotator.transform.rotation = Quaternion.Euler(0, 0, 0);
        rotator.transform.Rotate(Vector3.up, mainCamera.transform.rotation.eulerAngles.y);
   
    }
    public GameObject getPanelByID(int id) {
        foreach (GameObject g in facePanels) {
            if (g.GetComponent<PanelDetails>().ID == id) {
                return g;

            }

        }
        return null;

    }
    public GameObject SetPanelFormatCentered(int id) {
        GameObject returnObject = null;
        foreach (GameObject g in facePanels)
        {
            g.GetComponent<PanelDetails>().StoreFixedPosition();
            if (g.GetComponent<PanelDetails>().ID == id)
            {
                returnObject = g;
                g.SetActive(true);
            }
            else {
                g.SetActive(false);
            }

        }
        return returnObject;
    }
    public void SetPanelFormatStandard() {
        foreach (GameObject g in facePanels)
        {
            g.GetComponent<PanelDetails>().ReturnToFixedValues();
            g.SetActive(true);
        }

    }





}
