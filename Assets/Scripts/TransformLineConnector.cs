using UnityEngine;
using System.Collections;

public class TransformLineConnector : MonoBehaviour
{
    private LineRenderer _renderer = null;

	// Use this for initialization
	void Start ()
	{
	    _renderer = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if (transform.parent != transform.root)
	    {
	        _renderer.enabled = (transform.parent != transform.root);
            if (_renderer.enabled)
	        {
	            Vector3 world = transform.TransformPoint(0f, 0f, 0f);
	            Vector3 relativetoparentframe = transform.root.TransformVector(world);
	            _renderer.SetPosition(1, relativetoparentframe);
	        }
	    }
	}
}
