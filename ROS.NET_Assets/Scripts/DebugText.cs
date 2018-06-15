using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugText : MonoBehaviour
{
    private Text debugtext = null;
    private static DebugText _instance = null;

    private Queue<string> lines = new Queue<string>();
    private volatile Queue<string> additions = new Queue<string>();

    [CanBeNull]
    public static DebugText Instance
    {
        get { return _instance; }
    }

    // Use this for initialization
	void Start ()
	{
	    _instance = this;
        debugtext = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
	    lock (additions)
	    {
            bool changedlines = false;
	        while (additions.Count > 0)
	        {
                if (lines.Count > 10)
                    lines.Dequeue();
	            lines.Enqueue(additions.Dequeue());
                changedlines = true;
	        }
            if (changedlines && debugtext != null)
	            debugtext.text = lines.Aggregate("", (a, b) => a + b, (a) => a);
	    }
	}

    public static void Write(string fmt, params object[] args)
    {
        if (Instance != null)
        {
            lock (Instance.additions)
            {
                Instance.additions.Enqueue(string.Format(fmt, args));
            }
        }
    }

    public static void WriteLine(string fmt, params object[] args)
    {
        Write(fmt + "\n", args);
    }
}
