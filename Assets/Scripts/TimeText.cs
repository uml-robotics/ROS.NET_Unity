using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Ros_CSharp;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeText : MonoBehaviour
{
    private Text timetext = null;
    private static TimeText _instance = null;
    private static DateTime startTime = DateTime.Now;
    private static Thread timerUpdater = null;

    [CanBeNull]
    public static TimeText Instance
    {
        get { return _instance; }
    }

    // Use this for initialization
    void Start()
    {
        _instance = this;
        timetext = GetComponent<Text>();
        startTime = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        if (timetext != null)
        {
            timetext.text = DateTime.Now.Subtract(startTime).ToString();
        }
    }
}
