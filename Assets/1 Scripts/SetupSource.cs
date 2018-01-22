using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConvolveAudio))]
public class SetupSource : MonoBehaviour
{
    public TextAsset hrtfFilters;
    private ConvolveAudio ca;

    void Start()
    {
        if (ca == null)
        {
            ca = gameObject.GetComponent<ConvolveAudio>();
        }
    }

    public void setHRTF(int rowIndex)
    {
        // TODO - the HRTF is selected by file...use a random location (rowIndex)

        Start();

        string[] lines = hrtfFilters.text.Split('\n');

        if (rowIndex >= lines.Length)
        {
            Debug.LogWarning("Index out of range...setting to 0");
            rowIndex = Random.Range(0, lines.Length - 1);
        }
        ca.setHRTFString(lines[rowIndex]);
    }
}
