using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackoutScript : MonoBehaviour
{
    public delegate void OnBlackedoutEvent();
    public event OnBlackedoutEvent OnBlackedout;

    public float duration = 1f;
    public Material mat;

    private float currentDuration = 0f;
    private Color initialColor;
    private Color finalColor;
    private MeshRenderer meshRender;
    private bool blackingOut = false;
    private bool undoBlackout = false;

    public void Blackout()
    {
        currentDuration = 0f;
        meshRender.enabled = true;
        blackingOut = true;
    }

    public void UndoBlackout()
    {
        currentDuration = 1f;
        undoBlackout = true;
    }

    void Start ()
    {
        // perform checks
        if (duration <= 0)
            Debug.LogError("Blackout duration should be a positive number!");

        // initialization steps
        meshRender = gameObject.GetComponent<MeshRenderer>();
        meshRender.enabled = false;
    }
	
	void Update ()
    {
		if (blackingOut)
        {
            float alpha = Mathf.Clamp01(currentDuration / duration);
            mat.SetColor("_Color", new Color(0, 0, 0, alpha));
            currentDuration += Time.deltaTime;
            if (alpha == 1)
            {
                blackingOut = false;
                OnBlackedout();
            }
        }
        else if (undoBlackout)
        {
            float alpha = Mathf.Clamp01(currentDuration / duration);
            mat.SetColor("_Color", new Color(0, 0, 0, alpha));
            currentDuration -= Time.deltaTime;
            if (alpha == 0)
            {
                undoBlackout = false;
                meshRender.enabled = false;
            }
        }
	}
}
