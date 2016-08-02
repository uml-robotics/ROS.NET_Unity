using UnityEngine;
using System.Collections;
using Messages.sensor_msgs;
using Ros_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

public class LaserViewController : SensorTFInterface<LaserScan>
{

    //various collections 
    SortedList<DateTime, LaserScan> toDraw = new SortedList<DateTime, LaserScan>();
    List<GameObject> recycle = new List<GameObject>();
    List<GameObject> active = new List<GameObject>();

    private Messages.std_msgs.Time lastStamp = null; //used to check for out of date msgs
    private GameObject points; //will become child(0), used for cloning

    public float PointSize = 0.1f;
    public float DecayTime = 0f;
    public Color Color = new Color(1, 0, 0, 1);
    //curently not in use
    //private uint maxRecycle = 100;

    //construtor for setting MakeChildOfTF to true or false
    public LaserViewController() : base(false) {}

    protected override void Callback(LaserScan argument)
    {

        if(lastStamp != null && ROS.GetTime(argument.header.stamp) < ROS.GetTime(lastStamp)) 
        {
            UnityEngine.Debug.LogError("TIME IS MOVING BACKWARDS");
        }
        lastStamp = argument.header.stamp;

        lock (toDraw)
        {
            toDraw.Add(ROS.GetTime(argument.header.stamp), argument);
        }

    }


    protected override void Start()
    {
        base.Start();// must call base classes start function for it to find the propper TF for the sensor
        points = transform.GetChild(0).gameObject;
        points.hideFlags |= HideFlags.HideInHierarchy;
        points.SetActive(false);
        points.name = "Points";

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (DecayTime < 0.0001f)
        {

            lock (toDraw)
                while (toDraw.Count() > 1)
                {
                    //drop off extra toDraws while decay time is 0
                    remOldestFromToDraw();
                }

            lock(active)
                while(active.Count() > 1)
                {
                    //decay has been set to 0, clear active list leaving just 1
                    active.First().GetComponent<LaserScanView>().expire();
                }
        }

        lock(toDraw)
        {

            while (toDraw.Count() > 0)
            {
                GameObject newone = null;
                bool need_a_new_one = true;

                lock (recycle)
                    if (recycle.Count() > 0)
                    {
                        need_a_new_one = false;
                        newone = popRecycle();
                    }

                if (need_a_new_one)
                {
                    newone = Instantiate(points.transform).gameObject;
                    newone.transform.SetParent(null, false);

                    newone.hideFlags |= HideFlags.HideInHierarchy;

                    newone.GetComponent<LaserScanView>().Recylce += (oldScan) =>
                    {
                        lock (active)
                        {
                            active.Remove(oldScan);
                        }
                        lock (recycle)
                        {
                            recycle.Add(oldScan);
                        }
                    };
                 
                    
                    newone.GetComponent<LaserScanView>().IDied += (deadScan) =>
                    {
                        lock (active)
                        {
                            active.Remove(deadScan);
                        }
                        Destroy(deadScan); //destroy object
                    };
                    
                }

                KeyValuePair<DateTime, LaserScan>? oldest = remOldestFromToDraw();
                active.Add(newone);
                newone.GetComponent<LaserScanView>().SetScan(Time.fixedTime, oldest.Value.Value, gameObject, TF);

            }
        }
    }

    /**
        Recycle and ToDraw interface(s) for adding and removing elements safely
    **/

    #region ToDraw interface

    KeyValuePair<DateTime, LaserScan>? remOldestFromToDraw()
    {
        if (toDraw.Count == 0) return null;
        var min = toDraw.Keys.Min();
        var kvp = new KeyValuePair<DateTime, LaserScan>(min, toDraw[min]);
        toDraw.Remove(min);
        return new Nullable<KeyValuePair<DateTime, LaserScan>>(kvp);
    }

    #endregion

    #region Recycle interface

    GameObject popRecycle()
    {
        GameObject gameObjOut;
        gameObjOut = recycle.FirstOrDefault().gameObject;
        recycle.RemoveAt(0);
        return gameObjOut;
    }

    #endregion

    
    void OnDisable()
    {
        while(active.Count > 0)
        {
            if(active[0] != null)
            {
                active[0].GetComponent<LaserScanView>().expire();
            }else
            {
                active.RemoveAt(0);
            }
             
        }
    }
    
  
}