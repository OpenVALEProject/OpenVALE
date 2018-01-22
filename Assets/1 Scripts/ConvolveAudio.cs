using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ConvolveAudio : MonoBehaviour
{
    public AudioSource audioSource;

    private AudioClip clp;
    private AudioClip filteredClip;
    private float[] leftFilter;
    private float[] rightFilter;

    public void Init()
    {
        if (clp == null)
            clp = audioSource.clip;
    }

    private void Start()
    {
        Init();
    }

    public void setHRTFString(string text)
    {
        Init(); 

        // prepare the filter arrays
        string[] nums = text.Split(',');
        leftFilter = new float[320];
        rightFilter = new float[320];
        for (int i = 0; i < leftFilter.Length; i++)
        {
            leftFilter[i] = float.Parse(nums[i].Trim());
        }
        for (int i = 0; i < rightFilter.Length; i++)
        {
            rightFilter[i] = float.Parse(nums[leftFilter.Length + i].Trim());
        }

        // get data from clip
        float[] data = new float[(clp.samples + leftFilter.Length) * clp.channels];
        clp.GetData(data, 0);

        // ensure data is stereo
        if (clp.channels != 2)
        {
            Debug.Log("ConvolveAudio filter requires stereo input!");
            Debug.Log(clp.channels);
            return;
        }

        // get an array of the left channel of the audio data
        float[] monoData = new float[clp.samples];  // if data.Length is odd, this will potentially cause clicks
        for (int i = 0; i < monoData.Length; i++)
        {
            monoData[i] = data[i * 2];  // L - channel
        }

        // zero out the data array
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 0;
        }

        for (int n = 0; n < monoData.Length + leftFilter.Length - 1; n++)
        {
            int kmin;
            if (n >= leftFilter.Length - 1)
                kmin = n - (leftFilter.Length - 1);
            else
                kmin = 0;

            int kmax;
            if (n < monoData.Length - 1)
                kmax = n;
            else
                kmax = monoData.Length - 1;

            for (int k = kmin; k < kmax; k++)
            {
                // filter the audio, writing to data array
                data[n * 2] += monoData[k] * leftFilter[n - k];       // L
                data[n * 2 + 1] += monoData[k] * rightFilter[n - k];  // R
            }
        }

        // need a new audio clip so the data can be 320 samples longer (data length 640 more, but stereo)
        if (filteredClip == null)
        {
            filteredClip = AudioClip.Create("HRTF Filtered", data.Length, 2, clp.frequency, false);
            audioSource.clip = filteredClip;
        }

        filteredClip.SetData(data, 0);

        // play the new clip
        audioSource.Play();
    }
}
