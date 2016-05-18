using System;
using System.Collections.Generic;
using Ros_CSharp;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.EventSystems;

public class MasterChooserController : MonoBehaviour
{
    private List<Action> whendone = new List<Action>();
    public Component master_uri_text;
    public Component hostname_text;

    public bool checkNeeded()
    {
        string[] args = new string[0];
        IDictionary remappings;

        try
        {
            if (RemappingHelper.GetRemappings(ref args, out remappings))
            {
                if (remappings.Contains("__master"))
                {
                    return false;
                }
            }
        }
        catch(Exception ex)
        {
        }
        return string.IsNullOrEmpty(ROS.ROS_MASTER_URI);
    }

    private bool show()
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
        return true;
    }

    private bool act()
    {
        try
        {
            ROS.ROS_MASTER_URI = master_uri_text.GetComponent<UnityEngine.UI.Text>().text;
            ROS.ROS_HOSTNAME = hostname_text.GetComponent<UnityEngine.UI.Text>().text;
            hide();
            foreach (var a in whendone)
                a();
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
        transform.root.GetComponentInChildren<EventSystem>().gameObject.SetActive(false);
    }

    public bool ShowIfNeeded(Action whendone)
    {
        if (checkNeeded())
        {
            lock(whendone)
                this.whendone.Add(whendone);
            return show();
        }
        whendone();
        return true;
    }

    public void ButtonClicked()
    {
        act();
    }
}
