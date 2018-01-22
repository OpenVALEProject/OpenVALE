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
        /*
        float currentDegrees = 0.0f;
        float x;
        float z;
        Vector3 tempLocation;
        while (currentDegrees < 360) {
            x = Mathf.Sin(Mathf.Deg2Rad * currentDegrees);
            z = Mathf.Cos(Mathf.Deg2Rad * currentDegrees);
            tempLocation = new Vector3(x, 0.0f, z);
            tempLocation = tempLocation.normalized * panelDistance;
            possibleRingLocations.Add(tempLocation);
            currentDegrees += degreesSeperation;
        }
        */
        mainCamera = Camera.main;
	}
    void Update()
    {
        


        rotator.transform.position = new Vector3(UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.CenterEye).x, -.08f, UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.CenterEye).z);
        //rotator.transform.position = new Vector3(Camera.main.transform.position.x,-.12f, Camera.main.transform.position.z);

        //if (mainCamera.transform.rotation.eulerAngles.y > transform.rotation.eulerAngles.y + 10) {
        rotator.transform.rotation = Quaternion.Euler(0, 0, 0);
        rotator.transform.Rotate(Vector3.up, mainCamera.transform.rotation.eulerAngles.y);
        //}
        //if (mainCamera.transform.rotation.eulerAngles.y < transform.rotation.eulerAngles.y - 10)
        //{
        //   rotator.transform.rotation = Quaternion.Euler(0, 0, 0);
        //   rotator.transform.Rotate(Vector3.up, mainCamera.transform.rotation.eulerAngles.y -9);
        //}
    }
    public GameObject getPanelByID(int id) {
        foreach (GameObject g in facePanels) {
            //Debug.Log(id);
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
