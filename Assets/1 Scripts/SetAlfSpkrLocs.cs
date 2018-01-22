using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SetAlfSpkrLocs : MonoBehaviour
{
    public TextAsset locationsFile;
    public HighlightSpeakers highlightSpeakers;
    public Vector3 rotation;
    public GameObject par;
    public GameObject vis;

    public void Rotate()
    {
        // ALF_VR project - (x,y,z)=(-90,0,0)

        for (int i = 0; i < highlightSpeakers.alfSpkrLocs.Count; i++)
        {
            highlightSpeakers.alfSpkrLocs[i] = Quaternion.Euler(rotation) * highlightSpeakers.alfSpkrLocs[i];
        }
    }

    public void DeleteVisualization()
    {
        // delete all previous vis
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in par.transform)
            children.Add(child.gameObject);
        children.ForEach(child => DestroyImmediate(child));
    }

    public void AddVisualization()
    {
        DeleteVisualization();

        // add new ones
        for (int i = 0; i < highlightSpeakers.alfSpkrLocs.Count; i++)
        {
            Vector3 loc = highlightSpeakers.alfSpkrLocs[i] * 25;
            Instantiate(vis, loc, Quaternion.identity, par.transform);
        }
    }

	public void SetLocations()
    {
        // clear current locations
        highlightSpeakers.alfSpkrLocs = new List<Vector3>();

        Vector3 currentLocation = new Vector3();
        string[] lines = locationsFile.text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.Length <= 1)
                continue;

            string[] floats = line.Split(',');
            currentLocation.x = float.Parse(floats[0].Trim());
            currentLocation.y = float.Parse(floats[1].Trim());
            currentLocation.z = float.Parse(floats[2].Trim());

            // add to gameController list
            highlightSpeakers.alfSpkrLocs.Add(currentLocation);
        }
    }

    public void SetLocationsRotateVis()
    {
        SetLocations();
        Rotate();
        AddVisualization();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SetAlfSpkrLocs))]
public class SetAlfSpkrLocsGUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SetAlfSpkrLocs alfSpkrLocSetter = (SetAlfSpkrLocs)target;
        if (GUILayout.Button("Set locations"))
        {
            alfSpkrLocSetter.SetLocations();
        }

        if (GUILayout.Button("Rotate"))
        {
            alfSpkrLocSetter.Rotate();
        }

        if (GUILayout.Button("Visualize Highlighted Speakers"))
        {
            alfSpkrLocSetter.AddVisualization();
        }

        if (GUILayout.Button("Set Locations, Rotate, and Visualize"))
        {
            alfSpkrLocSetter.SetLocationsRotateVis();
        }

        if (GUILayout.Button("Delete Visualization"))
        {
            alfSpkrLocSetter.DeleteVisualization();
        }
    }
}
#endif
