using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroInput : MonoBehaviour
{
    bool isGyroEnabled;
    Vector3 myRot;

    private void Start()
    {
        // disable this component if VR is enabled
        if (UnityEngine.XR.XRSettings.enabled)
            enabled = false;

        // save initial gyro state so we can revert in OnApplicationQuit()
        isGyroEnabled = Input.gyro.enabled;

        // make sure gyro is enabled
        if (!isGyroEnabled)
            Input.gyro.enabled = true;
    }

    void Update ()
    {
        // apply attitude orientation from gyro with necessary conversions
        myRot = Input.gyro.attitude.eulerAngles;
        myRot.x *= -1;
        myRot.y *= -1;
        transform.rotation = Quaternion.Euler(myRot);
        transform.Rotate(90, 0, 0, Space.World);
    }

    private void OnApplicationQuit()
    {
        // revert gyro enabled state
        Input.gyro.enabled = isGyroEnabled;
    }
}
