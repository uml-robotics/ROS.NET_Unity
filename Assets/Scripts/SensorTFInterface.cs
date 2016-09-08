using UnityEngine;
using System.Collections;
using Messages;
using Ros_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

public class SensorTFInterface<M> : ROSMonoBehavior where M : IRosMessage, new()
{
    
    private static TfVisualizer _tfvisualizer;
    private static object vislock = new object();
    public bool MakeChildOfTF = false;
    public String Topic; //the topic the base and child class will be subscribing too
                         //also the topic that the TF will be associated with
    protected string NameSpace = "";
    public void setNamespace(string _NameSpace)
    {
        NameSpace = _NameSpace;
    }

    //get tfVisualizer from root to lookup iframe
    internal TfVisualizer tfvisualizer
    {
        get
        {
            if (_tfvisualizer == null)
            {
                _tfvisualizer = transform.root.GetComponentInChildren<TfVisualizer>();
            }

            return _tfvisualizer;
        }
    }

    protected String TFName;//currently being used to lookup the TF

    //this will be the transform the topic is associated with 
    internal Transform TF
    {
        get
        {
            if (TFName == null)
            {
                return transform;
            }

            Transform tfTemp;
            String strTemp = TFName;
            if (!strTemp.StartsWith("/"))
            {
                strTemp = "/" + strTemp;
            }
            if (tfvisualizer != null && tfvisualizer.queryTransforms(strTemp, out tfTemp))
                return tfTemp;
            return transform;
        }
    }

    public SensorTFInterface() {}
    public SensorTFInterface (bool _MakeChildOfTF) { MakeChildOfTF = _MakeChildOfTF; }

    //ros stuff
    internal NodeHandle nh;
    internal Subscriber<M>  subscriber;

    //figures out the frameid of the sensor 
    private void _realCallback(M msg)
    {
        if (TFName == null && msg.HasHeader())
        {
            FieldInfo fi = msg.GetType().GetFields().First((a) => { return a.FieldType.Equals(typeof(Messages.std_msgs.Header)); });
            if (fi != null)
            {
                TFName = ((Messages.std_msgs.Header)fi.GetValue(msg)).frame_id;
            }
        }
        Callback(msg);
    }

    protected virtual void Callback(M msg)
    {
        throw new NotImplementedException();
    }

    protected virtual void Start()
    {
        if(!Topic.StartsWith("/"))
        {
            Topic = "/" + Topic;
        }
        rosmanager.StartROS(this, () => {
            nh = new NodeHandle();
            subscriber = nh.subscribe<M>(NameSpace + Topic, 1, _realCallback);
        });

    }

    protected virtual void Update()
    {
        if(MakeChildOfTF && transform.parent != TF)
        {
            transform.parent = TF;
        }

        if (!MakeChildOfTF)
        {
            transform.position = TF.position;
            transform.rotation = TF.rotation;
        }

    }

}

