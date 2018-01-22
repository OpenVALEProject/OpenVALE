using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRotator : MonoBehaviour {
    public GameObject rotator;
    private Camera mainCamera;
    public Text messagePanel;
    private float visibleStartTime;
    private float visibleLength = -1;
    

    // Use this for initialization
    void Start () {
		mainCamera = Camera.main;
        messagePanel.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (!messagePanel.enabled)
            return;
        //if (Time.time - visibleStartTime > visibleLength) {
        //    messagePanel.enabled = false;
        //    return;
        //
        if (visibleLength > 0) {
            if (Time.time - visibleStartTime > visibleLength)
            {
                messagePanel.enabled = false;
                visibleLength = -1;
            }
        }
        //rotator.transform.position = new Vector3(mainCamera.transform.position.x, 0.18f, mainCamera.transform.position.z);
        rotator.transform.position = new Vector3(UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.CenterEye).x, .18f, UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.CenterEye).z);
        //if (mainCamera.transform.rotation.eulerAngles.y > transform.rotation.eulerAngles.y + 10)
       // {
        rotator.transform.rotation = Quaternion.Euler(0, 0, 0);
        rotator.transform.Rotate(Vector3.up, mainCamera.transform.rotation.eulerAngles.y );
        //}
        //if (mainCamera.transform.rotation.eulerAngles.y < transform.rotation.eulerAngles.y - 10)
        //{
        //    rotator.transform.rotation = Quaternion.Euler(0, 0, 0);
        //    rotator.transform.Rotate(Vector3.up, mainCamera.transform.rotation.eulerAngles.y - 9);
       // }
    }
    public void setMessage(string message, bool visible =true, float length = -1) {
        messagePanel.text = message;
        messagePanel.enabled = visible;
        visibleStartTime = Time.time;
        visibleLength = length;
    }
    public void hideMessage() {
        messagePanel.enabled = false;
    }
}
