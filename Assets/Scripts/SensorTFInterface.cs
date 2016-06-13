using UnityEngine;
using System.Collections;
using Messages;
using Ros_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;


public class SensorTFInterface : MonoBehaviour
{
    public String topic;
    public ROSManager ROSManager;

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
            ROSManager.gameObject.GetComponent<TfVisualizer>().queryTransforms(strTemp, out tfTemp);
            return tfTemp == null ? transform : tfTemp;

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
