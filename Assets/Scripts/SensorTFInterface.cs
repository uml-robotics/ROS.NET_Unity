using UnityEngine;
using System.Collections;
using Messages;
using Ros_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;


public class SensorTFInterface : ROSMonoBehavior
{
    private static TfVisualizer _tfvisualizer;
    private static object vislock = new object();
    public TfVisualizer tfvisualizer
    {
        get
        {
            lock(vislock)
            {
                if (_tfvisualizer == null)
                {
                    _tfvisualizer = transform.root.GetComponentInChildren<TfVisualizer>();
                }
            }
            return _tfvisualizer;
        }
    }

    public String topic;
    internal String TFName;
    internal Transform TF {
        get
        {
            Transform tfTemp;
            String strTemp = TFName;
            if (!strTemp.StartsWith("/"))
            {
                strTemp = "/" + strTemp;
            }
            if (tfvisualizer.queryTransforms(strTemp, out tfTemp))
                return tfTemp;
            return transform;
        } }




    //Recursively search for a tf that is a grandchild of rootTf
    Transform getTf(Transform rootTf, String TfName)
    {
        Transform TfOut;
        TfOut = rootTf.Find(TfName);

        if (TfOut != null)
        {
            return TfOut;
        }

        foreach (Transform tf in rootTf)
        {
            Debug.Log("TFNAME: " + tf.name);
            TfOut = getTf(tf, TfName);
            if (TfOut != null)
            {
                return TfOut;
            }
        }
        return TfOut;
    }

}
