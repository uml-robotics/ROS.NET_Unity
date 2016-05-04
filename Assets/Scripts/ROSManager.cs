using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ros_CSharp;
using UnityEngine;
using UnityEditor;
using XmlRpc_Wrapper;

/// <summary>
/// Forces ones call to ROS.Init by multiple ROS things in the process
/// ONLY starts ROS.NET outside of the editor OR if the editor is playing the scene
/// </summary>
public class ROSManager : MonoBehaviour
{
    /// <summary>
    /// Call ROS.Init if it hasn't been called, and informs callers whether to try to make a nodehandle and pubs/subs
    /// </summary>
    /// <returns>Whether ros.net initialization can continue</returns>
    public static bool StartROS()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
#endif
            if (!ROS.isStarted())
            {
                ROS.Init(new string[0], "unity_test_" + DateTime.Now.Ticks);
            }
            XmlRpc_Wrapper.XmlRpcUtil.SetLogLevel(XmlRpcUtil.XMLRPC_LOG_LEVEL.ERROR);
            return true;
#if UNITY_EDITOR
        }
#endif
        return false;
    }

    public static void StopROS()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
#endif
            ROS.shutdown();
            ROS.waitForShutdown();
#if UNITY_EDITOR
        }
#endif
    }

    void OnApplicationQuit()
    {
        StopROS();
    }
}
