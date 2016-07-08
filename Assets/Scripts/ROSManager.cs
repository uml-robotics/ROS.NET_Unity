using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Ros_CSharp;
using UnityEngine;
using XmlRpc_Wrapper;
#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
#endif
public class ROSMonoBehavior : MonoBehaviour
{
    
    protected static readonly ROSManager rosmanager;
    static ROSMonoBehavior()
    {
        rosmanager = new ROSManager();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playmodeStateChanged = () =>
        {
            string state = "";
            if (EditorApplication.isPlaying)
                state += "playing";
            if (EditorApplication.isPaused)
                state += " paused";
            if (EditorApplication.isCompiling)
                state += " compiling";
            state = state.Trim(' ');
            Debug.LogWarning("PlayMode == " + state);
            if (!EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                ROS.Unfreeze();
                if (ROS.ok || ROS.isStarted())
                    ROSManager.StopROS();
            }
            else if (EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPaused)
                {
                    ROS.Unfreeze();
                    rosmanager.StartROS(null, null);
                }
                else
                {
                    ROS.Freeze();
                }
            }
        };
#endif
    }
}

/// <summary>
/// Forces ones call to ROS.Init by multiple ROS things in the process
/// ONLY starts ROS.NET outside of the editor OR if the editor is playing the scene
/// </summary>
public class ROSManager
{
    private static object loggerlock = new object();
    private static StreamWriter logwriter = null;

    /// <summary>
    /// Call ROS.Init if it hasn't been called, and informs callers whether to try to make a nodehandle and pubs/subs
    /// </summary>
    /// <returns>Whether ros.net initialization can continue</returns>
    public void StartROS(MonoBehaviour caller, Action whensuccessful)
    {
        if (whensuccessful != null)
        {
            Action whatToDo = () =>
            {
#if UNITY_EDITOR
                if (EditorApplication.isPlaying && !EditorApplication.isPaused)
                {
#endif
                    if (!ROS.isStarted())
                    {
                        ROS.Init(new string[0], "unity_test_" + DateTime.Now.Ticks);
                        XmlRpcUtil.SetLogLevel(XmlRpcUtil.XMLRPC_LOG_LEVEL.ERROR);
                        tf.net.Transformer.LateInit();
                    }
                    if (whensuccessful != null)
                        whensuccessful();
#if UNITY_EDITOR
                }
#endif
            };
            if (caller != null)
            {
                MasterChooserController MasterChooser = caller.transform.root.GetComponentInChildren<MasterChooserController>(true);
                if (MasterChooser == null || !MasterChooser.checkNeeded())
                {
                    whatToDo();
                }
                else if (!MasterChooser.ShowIfNeeded(whatToDo))
                {
                    Debug.LogError("Failed to test for applicability, show, or handle masterchooser input");
                }
            }
        }
        else if (EditorApplication.isPlaying && !EditorApplication.isPaused && !ROS.isStarted())
        {
            ROS.Init(new string[0], "unity_test_" + DateTime.Now.Ticks);
            XmlRpcUtil.SetLogLevel(XmlRpcUtil.XMLRPC_LOG_LEVEL.ERROR);
            tf.net.Transformer.LateInit();
        }

        lock (loggerlock)
        {
            if (logwriter == null)
            {
                string validlogpath = null;
                string filename = "unity_test_" + DateTime.Now.Ticks + ".log";
                foreach (string basepath in new[] { Application.dataPath, "/sdcard/ROS.NET_Logs/" })
                {
                    if (Directory.Exists(basepath))
                    {
                        try
                        {
                            if (Directory.GetFiles(basepath) == null)
                            {
                                continue;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e);
                        }
                    }
                    try
                    {
                        logwriter = new StreamWriter(Path.Combine(basepath, filename));
                        logwriter.AutoFlush = true;
                        logwriter.WriteLine("Opened log file for writing at " + DateTime.Now);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }
                    Application.logMessageReceived += Application_logMessageReceived;
                    break;
                }
            }
        }
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
