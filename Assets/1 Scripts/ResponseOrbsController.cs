using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponseOrbsController : MonoBehaviour {
    public List<GameObject> ResponseOrbs = new List<GameObject>();
    public float degreesSeperation = 15.0f;
    public float panelDistance = 2.07f;
    public GameObject highlighter;
    // Use this for initialization
    void Start () {
        float currentDegrees = 0.0f;
        float x;
        float z;
        Vector3 tempLocation;
        int iterator = 0;
        while (currentDegrees < 360)
        {
            
            x = Mathf.Sin(Mathf.Deg2Rad * currentDegrees);
            z = Mathf.Cos(Mathf.Deg2Rad * currentDegrees);
            tempLocation = new Vector3(x, 0.0f, z);
            tempLocation = tempLocation.normalized * panelDistance;
            
            ResponseOrbs[iterator].transform.localPosition = tempLocation;
            ResponseOrbs[iterator].GetComponent<ResponseOrb>().ID = iterator;

            //possibleRingLocations.Add(tempLocation);

            currentDegrees += degreesSeperation;
            iterator++;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public GameObject returnNearestOrb(Vector3 location) {
        float currentDistance = 500;
        GameObject currentNearest = ResponseOrbs[0];
        currentDistance = (currentNearest.transform.position - location).sqrMagnitude;
        float testDistance = 0;
        foreach (GameObject g in ResponseOrbs) {
            testDistance = (g.transform.position - location).sqrMagnitude;
            if (testDistance < currentDistance) {
                currentDistance = testDistance;
                currentNearest = g;
            }

        }
        return currentNearest;

    }
    public GameObject HighlightOrbByPosition(Vector3 position) {

        if (position.sqrMagnitude == 0f) {
            highlighter.SetActive(false);
            return null;
        }
        float currentDistance = 500;
        GameObject currentNearest = ResponseOrbs[0];
        currentDistance = (currentNearest.transform.position - position).sqrMagnitude;
        float testDistance = 0;
        foreach (GameObject g in ResponseOrbs)
        {
            testDistance = (g.transform.position - position).sqrMagnitude;
            if (testDistance < currentDistance)
            {
                currentDistance = testDistance;
                currentNearest = g;
            }

        }
        highlighter.SetActive(true);
        highlighter.transform.position = currentNearest.transform.position;
        return currentNearest;


        //return null;
    }
}
