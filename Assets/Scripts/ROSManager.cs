using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Ros_CSharp;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using XmlRpc_Wrapper;

/// <summary>
/// Forces ones call to ROS.Init by multiple ROS things in the process
/// ONLY starts ROS.NET outside of the editor OR if the editor is playing the scene
/// </summary>
public class ROSManager : MonoBehaviour
{
    private static object loggerlock = new object();
    private static StreamWriter logwriter = null;

    /// <summary>
    /// Call ROS.Init if it hasn't been called, and informs callers whether to try to make a nodehandle and pubs/subs
    /// </summary>
    /// <returns>Whether ros.net initialization can continue</returns>
    public static bool StartROS()
    {
        lock (loggerlock)
        {
            if (logwriter == null)
            {
                logwriter = new StreamWriter(Path.Combine(Application.dataPath, "unity_test_" + DateTime.Now.Ticks + ".log"));
                logwriter.AutoFlush = true;
                Application.logMessageReceived += Application_logMessageReceived;
            }
        }
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
#endif
            if (!ROS.isStarted())
            {
                ROS.Init(new string[0], "unity_test_" + DateTime.Now.Ticks);
                XmlRpcUtil.SetLogLevel(XmlRpcUtil.XMLRPC_LOG_LEVEL.ERROR);
            }
            return true;
#if UNITY_EDITOR
        }
#endif
        return false;
    }

    static void  Application_logMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Log)
            logwriter.WriteLine("{0}\t\t{1}\n", type.ToString(), condition);
        else
            logwriter.WriteLine("{0}\t\t{1}\n{2}\n", type.ToString(), condition, stackTrace.Split('\n').Aggregate("",(a,b)=>a+"\t"+b+"\n"));

#if UNITY_EDITOR
        if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
        {
            StopROS();
        }
#endif
    }

    public static void StopROS()
    {
        Debug.Log("ROSManager is shutting down");
        ROS.shutdown();
        ROS.waitForShutdown();
        lock (loggerlock)
        {
            Application.logMessageReceived -= Application_logMessageReceived;
            if (logwriter != null)
            {
                logwriter.Close();
                logwriter = null;
            }
        }
    }

    void OnApplicationQuit()
    {
        StopROS();
    }
}
