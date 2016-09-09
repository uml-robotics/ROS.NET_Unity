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
#if LOG_TO_FILE
    private static object loggerlock = new object();
    private static StreamWriter logwriter = null;
#endif

    private static AutoResetEvent roslock = new AutoResetEvent(true);

    private static void _startROS()
    {
        if (!ROS.isStarted() && roslock.WaitOne())
        {
            // Make sure we are still the first caller to initialize ros, in case we blocked waiting for the lock
            if (!ROS.isStarted())
            {
                ROS.Init(new string[0], "unity_test_" + DateTime.Now.Ticks);
                tf.net.Transformer.LateInit();
            }
            roslock.Set();
        }
    }

    /// <summary>
    /// Call ROS.Init if it hasn't been called, and informs callers whether to try to make a nodehandle and pubs/subs
    /// </summary>
    /// <returns>Whether ros.net initialization can continue</returns>
    public void StartROS(MonoBehaviour caller, Action whensuccessful)
    {
        XmlRpcUtil.SetLogLevel(XmlRpcUtil.XMLRPC_LOG_LEVEL.ERROR);
        if (whensuccessful != null)
        {
            Action whatToDo = () =>
            {
#if UNITY_EDITOR
                    if (EditorApplication.isPlaying && !EditorApplication.isPaused)
                    {
#endif
                        _startROS();
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
            else
            {
                whatToDo();
            }
        }
        else
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
#endif
                _startROS();
#if UNITY_EDITOR
            }
#endif
        }
#if LOG_TO_FILE
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
                    break;
                }
            }
        }
#endif
        Application.logMessageReceived += Application_logMessageReceived;
    }

    static void  Application_logMessageReceived(string condition, string stackTrace, LogType type)
    {
#if LOG_TO_FILE
        if (type == LogType.Log)
            logwriter.WriteLine("{0}\t\t{1}\n", type.ToString(), condition);
        else
            logwriter.WriteLine("{0}\t\t{1}\n{2}\n", type.ToString(), condition, stackTrace.Split('\n').Aggregate("",(a,b)=>a+"\t"+b+"\n"));
#endif
#if UNITY_EDITOR
        if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
        {
            StopROS();
        }
#endif
    }

    public static void StopROS()
    {
        if (ROS.isStarted() && !ROS.shutting_down && roslock.WaitOne())
        {
            if (ROS.isStarted() && !ROS.shutting_down)
            {
                Debug.Log("ROSManager is shutting down");
                ROS.shutdown();
            }
            roslock.Set();
        }
        ROS.waitForShutdown();
#if LOG_TO_FILE
        lock (loggerlock)
        {
            Application.logMessageReceived -= Application_logMessageReceived;
            if (logwriter != null)
            {
                logwriter.Close();
                logwriter = null;
            }
        }
#endif
    }

    void OnApplicationQuit()
    {
        StopROS();
    }
}
