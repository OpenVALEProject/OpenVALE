using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginInfo : MonoBehaviour
{
    private int id;
    private int pin;

    public int Id
    {
        get
        {
            return id;
        }

        set
        {
            id = value;
        }
    }

    public int Pin
    {
        get
        {
            return pin;
        }

        set
        {
            pin = value;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
