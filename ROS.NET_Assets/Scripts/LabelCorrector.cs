using UnityEngine;
using System.Collections;
using System;

public class LabelCorrector : MonoBehaviour
{
    private static event Action<bool> updatevis;
    private bool visstate;
    private static bool lastvisstate = false;
    private static object initlock = new object();


    // Use this for initialization
    void Start ()
    {
        lock (initlock)
            updatevis += _update;
        visstate = lastvisstate;
    }

    public static void update(bool state)
    {
        lastvisstate = state;
        if (updatevis != null)
            updatevis(state);
    }

    private void _update(bool state)
    {
        visstate = state;
    }

    // Update is called once per frame
    void Update ()
    {
        gameObject.GetComponentInChildren<MeshRenderer>(true).enabled = visstate;
        transform.LookAt(Camera.main.transform, Camera.main.transform.up);
	    transform.Rotate(new Vector3(0f, 1f, 0f), 180);
	}
}
