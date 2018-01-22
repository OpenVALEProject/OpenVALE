using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamTargetController : MonoBehaviour
{
    public Transform lookAtTarget;
    public Transform posTarget;
	
	void Update ()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - lookAtTarget.position);
        transform.position = posTarget.position;
	}
}
