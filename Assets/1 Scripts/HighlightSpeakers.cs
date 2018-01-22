using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightSpeakers : MonoBehaviour
{
    public List<Vector3> alfSpkrLocs;
    public GameObject alfSpkrHighlight;
    public int numSpkrs = 10;
    public GameObject highlightParent;
    public float updateInterval = 0.1f;
    public Color baseColor;
    public Color baseEmitColor;
    public Color highlightedColor;
    public Color highlightedEmitColor;
    public float alphaFadeoutAngle = 30.0f;
    public float maxAlpha = 1.0f;
    public float distanceToSphere = 16.0f;
    public bool isEnabled = true;

    private List<GameObject> highlights;

    void Start ()
    {
        highlights = new List<GameObject>();

        for (int i = 0; i < numSpkrs; i++)
            highlights.Add(Instantiate(alfSpkrHighlight, highlightParent.transform) as GameObject);

        if (numSpkrs == 1)
        { // optimized solution for when the surrounding speaker highlights aren't needed
            highlights[0].GetComponent<Renderer>().material.SetColor("_Color", highlightedColor);
            highlights[0].GetComponent<Renderer>().material.SetColor("_EmissionColor", highlightedEmitColor);
        }

        StartCoroutine(UpdateHighlights());
	}

    // update location and alpha of highlights
    IEnumerator UpdateHighlights()
    {
        while (true)
        {
            if (enabled)
                highlightParent.SetActive(true);

            if (!isEnabled)
            {
                highlightParent.SetActive(false);
            }
            else if (numSpkrs == 1)
            {
                // optimized solution for when the surrounding speaker highlights aren't needed
                highlights[0].transform.localPosition = FindClosestSpkrLoc() * distanceToSphere;
            }
            else
            {
                alfSpkrLocs.Sort(CompareVectors);

                for (int i = 0; i < highlights.Count; i++)
                {
                    highlights[i].transform.localPosition = alfSpkrLocs[i] * distanceToSphere;

                    // current highlight
                    if (i == 0)
                    {
                        highlights[i].GetComponent<Renderer>().material.SetColor("_Color", highlightedColor);
                        highlights[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", highlightedEmitColor);
                        continue;
                    }

                    // surrounding highlights
                    baseColor.a = Mathf.Lerp(maxAlpha, 0, Vector3.Angle(transform.forward, alfSpkrLocs[i]) / alphaFadeoutAngle);
                    highlights[i].GetComponent<Renderer>().material.SetColor("_Color", baseColor);
                    highlights[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", baseEmitColor);
                }
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private Vector3 FindClosestSpkrLoc()
    {
        float minAngle = 180f;
        Vector3 minVector = new Vector3();
        float currentAngle;

        for (int i = 0; i < alfSpkrLocs.Count; i++)
        {
            currentAngle = Vector3.Angle(transform.forward, alfSpkrLocs[i]);
            if (currentAngle < minAngle)
            {
                minVector = alfSpkrLocs[i];
                minAngle = currentAngle;
            }
        }

        return minVector;
    }

    public int CompareVectors(Vector3 v1, Vector3 v2)
    {
        float ang1 = Vector3.Angle(v1, transform.forward);
        float ang2 = Vector3.Angle(v2, transform.forward);
        return (int) (ang1 - ang2);
    }

    public Vector3 GetActivePos()
    {
        return highlights[0].transform.position;
    }
}
