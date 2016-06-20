using UnityEngine;
using System.Collections;
using System;

public class TransformLineConnector : MonoBehaviour
{
    private LineRenderer _renderer = null;
    private static event Action<bool> updatevis;
    private static object initlock = new object();
    private bool visstate;
    private static bool lastvisstate = false;

    // Use this for initialization
    void Start()
    {
        _renderer = GetComponent<LineRenderer>();
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
    void Update()
    {
        if (transform.parent != transform.root && transform.parent != null)
	    {
	        _renderer.enabled = visstate && (transform.parent != transform.root);
            if (_renderer.enabled)
	        {
                _renderer.SetPosition(0, transform.parent.position);
	            _renderer.SetPosition(1, transform.position);
	        }
	    }
	}
}
