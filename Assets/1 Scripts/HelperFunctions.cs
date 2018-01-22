using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions{

    public static Vector3 FLTToUnityXYZ(Vector3 FLTVector) {
        Vector3 returnVector = new Vector3(-FLTVector.y, FLTVector.z, FLTVector.x);
        return returnVector;
    }

    public static Vector3 UnityXYZToFLT(Vector3 UnityVector) {
        Vector3 returnVector = new Vector3(UnityVector.z,-UnityVector.x,UnityVector.y);

        return returnVector;

    }
}
