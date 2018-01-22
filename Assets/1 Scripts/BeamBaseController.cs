using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamBaseController : MonoBehaviour
{
    public Transform lookAtTarget;
    public Transform locationTarget;
	
	void Update ()
    {
        transform.LookAt(lookAtTarget);
        transform.position = locationTarget.position;
	}
}
