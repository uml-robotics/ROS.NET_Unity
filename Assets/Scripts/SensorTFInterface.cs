using UnityEngine;
using System.Collections;
using Messages;
using Ros_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

public class SensorTFInterface <M> : ROSMonoBehavior where M : IRosMessage, new()
{
    private static TfVisualizer _tfvisualizer;
    private static object vislock = new object();
    public bool ChildOfTF = false;
    public String Topic; //use this on inspector
    internal string _Topic { get{ return !Topic.StartsWith("/") ? "/" + Topic : Topic;} } //use this in code

    //the topic the base and child class will be subscribing too
    //also the topic that the TF will be associated with
    public string NameSpace = "/agent1";
    public void setNamespace(string _NameSpace)
    {
        NameSpace = _NameSpace;
    }

    public TfVisualizer tfvisualizer //get tfVisualizer from root to lookup iframe
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

    private String TFName;//currently being used to lookup the TF

    internal Transform TF //this will be the transform the topic is associated with 
    {
        get
        {   
            if(TFName == null)
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

    private NodeHandle nh;

    private Subscriber<M>  subscriber;

    

    internal void Start()
    {
        rosmanager.StartROS(this, () => {
            nh = new NodeHandle();
            subscriber = nh.subscribe<M>(NameSpace + _Topic, 1, callBack);
        });

    }

    private void callBack(M msg) //figures out the frameid of the sensor 
    {
        
        if (msg.HasHeader && TFName == null)
        {
            FieldInfo fi = msg.GetType().GetFields().First((a)=>{ return a.FieldType.Equals(typeof(Messages.std_msgs.Header)); });
            if (fi != null)
            { 
                TFName = ((Messages.std_msgs.Header)fi.GetValue(msg)).frame_id;
                return;
            }
        }
        
    }

    internal void Update()
    {
       
        if (ChildOfTF && TF != transform)
        {
            transform.parent = TF;
        }
        else
        {
            transform.position = TF.position;
            transform.rotation = TF.rotation;
        }
       
    }



}
