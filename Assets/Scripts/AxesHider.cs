using UnityEngine;
using System.Collections;
using System;

public class AxesHider : MonoBehaviour {

    private static event Action<bool> updatevis;
    private bool visstate;
    private static bool lastvisstate = false;
    private static object initlock = new object();
    private bool mylastvisstate = true;

    // Use this for initialization
    void Start()
    {
        lock(initlock)
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

    void Update()
    {
        if (mylastvisstate != visstate)
        {
            mylastvisstate = visstate;
            MeshRenderer[] rends = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in rends)
            {
                m.enabled = visstate;
            }
        }
    }
}
