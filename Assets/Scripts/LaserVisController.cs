using UnityEngine;
using System.Collections;
using Messages.sensor_msgs;
using Ros_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

public class LaserVisController : MonoBehaviour
{
    private NodeHandle nh = null;
    private Subscriber<LaserScan> scansub;
    SortedList<uint , LaserScan> toDraw = new SortedList<uint, LaserScan>();
    List<GameObject> recycle = new List< GameObject>();
    private GameObject points;

    public string scan_topic;
    public ROSManager ROSManager;

    public float pointSize = 1;
    public uint maxRecycle = 100;
    public float Decay_Time = 0f;
    public bool Debug_Messages = false;

    // Use this for initialization
    void Start()
    {
        ROSManager.GetComponent<ROSManager>().StartROS(() => {
            nh = new NodeHandle();
            scansub = nh.subscribe<LaserScan>(scan_topic, 1, scancb);
        });

        //get the TEMPLATE view (our only child 
        points = transform.GetChild(0).gameObject;
        points.hideFlags |= HideFlags.HideAndDontSave;
        points.SetActive(false);
        points.name = "Points";
    }

    private void scancb(LaserScan argument)
    {
        /*
            Debug.Log("Penis");
            Debug.Log(argument.ranges[0].ToString());
            Debug.Log(argument.ranges[1].ToString());
            Debug.Log(argument.ranges[2].ToString());
            Debug.Log("angle_max: " + argument.angle_max.ToString());
            Debug.Log("angle_min: " + argument.angle_min.ToString());
            Debug.Log("angle_inc: " + argument.angle_increment.ToString());
            int angles = (int)((Math.Abs(argument.angle_min) + Math.Abs(argument.angle_max)) / argument.angle_increment);
            Debug.Log("angles: " + angles.ToString());
            Debug.Log("Ranges_size: " + argument.ranges.Length.ToString());
            */
        lock (toDraw)
            if(toDraw.Count > 2 && Debug_Messages)
            {
                DebugText.WriteLine("First Element: " + toDraw.ElementAt(0).Key.ToString());
                DebugText.WriteLine("Last Element: " + toDraw.Last().Key.ToString());
            }
            toDraw.Add(argument.header.seq , argument);
    }

    // Update is called once per frame
    void Update()
    {
        
        lock (toDraw)
        {
            if (Decay_Time < 0.0001f)
            {
                while (toDraw.Count > 1)
                {
                    toDraw.RemoveAt(0);
                }

                if (transform.childCount > 1)
                {
                    //transform.GetChild(1).gameObject.SetActive(false);
                    //transform.GetChild(1).gameObject.hideFlags |= HideFlags.HideAndDontSave;
                    lock (recycle)
                        recycle.Add(transform.GetChild(1).gameObject);
                }
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform c = transform.GetChild(i);
                    // eat our siblings
                    if (i > 1)
                    {
                        c.SetParent(null); //removes child from parents transform list
                        Destroy(c.gameObject); //removes the game object permantently from the scene
                    }
                }
            }
            while (toDraw.Count > 0)
            {
                GameObject newone = null;
                bool need_a_new_one = true;
                lock (recycle)
                {
                    if (recycle.Count > 0)
                    {
                        need_a_new_one = false;
                        newone = recycle.ElementAt(0);
                        recycle.RemoveAt(0);
                        if (Decay_Time < 0.0001f)
                            recycle.Clear();
                    }
                
                    if (need_a_new_one)
                    {
                        newone = Instantiate(points.transform).gameObject;
                        newone.transform.SetParent(transform, false);
                        newone.hideFlags |= HideFlags.HideAndDontSave;
                        //newone = ((instantiate a copy of the template))
                        newone.GetComponent<LaserScanView>().Recylce += (oldScan) =>
                        {
                            lock (recycle)
                                recycle.Add(oldScan);
                        };

                        newone.GetComponent<LaserScanView>().IDied += (deadScan) =>
                        {
                            lock (recycle)
                            {
                               
                                    recycle.Remove(deadScan); //attempt to remove if in recycle
                                    deadScan.transform.SetParent(null); //disconnect from parent
                                    Destroy(deadScan); //destroy object
                            
                            }
                        };
                    }
                
                        KeyValuePair<uint, LaserScan> oldest = toDraw.First();
                        toDraw.Remove(oldest.Key);
                        newone.GetComponent<LaserScanView>().SetScan(Time.fixedTime, oldest.Value);
                    
                }
            }
        }
    }
}