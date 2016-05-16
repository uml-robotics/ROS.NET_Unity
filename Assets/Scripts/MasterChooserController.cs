using System;
using System.Collections.Generic;
using Ros_CSharp;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class MasterChooserController : MonoBehaviour
{


    public string master_uri;
    public string hostname;

    private bool checkNeeded()
    {
        string[] args = new string[0];
        IDictionary remappings;
        if (RemappingHelper.GetRemappings(ref args, out remappings))
        {
            if (remappings.Contains("__master"))
            {
                return false;
            }
        }
        
        return string.IsNullOrEmpty(ROS.ROS_MASTER_URI);
    }

    private bool show()
    {
        gameObject.SetActive(true);
        return true;
    }

    private bool act()
    {
        try
        {
            ROS.ROS_MASTER_URI = master_uri;
            ROS.ROS_HOSTNAME = hostname;
            hide();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        return false;
    }

    private void hide()
    {
        gameObject.SetActive(false);
    }

    public bool ShowIfNeeded()
    {
        if (checkNeeded())
        {
            return show();
        }
        return true;
    }

    public void MasterURIChanged(string s)
    {
        master_uri = s;
    }

    public void HostnameChanged(string s)
    {
        hostname = s;
    }

    public void ButtonClicked()
    {
        act();
    }
}
