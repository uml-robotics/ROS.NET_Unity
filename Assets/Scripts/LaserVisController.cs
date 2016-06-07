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
    SortedList<uint, LaserScan> toDraw = new SortedList<uint, LaserScan>();
    List<GameObject> recycle = new List<GameObject>();
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

        if (toDraw.Count > 2 && Debug_Messages)
        {
            DebugText.WriteLine("First Element: " + toDraw.ElementAt(0).Key.ToString());
            DebugText.WriteLine("Last Element: " + toDraw.Last().Key.ToString());
        }
        //toDraw.Add(argument.header.seq, argument);
        addToDraw(argument);
    }

    // Update is called once per frame
    void Update()
    {


        if (Decay_Time < 0.0001f)
        {
           
            while (toDraw.Count > 1)
            {
                remFirstFromToDraw();
            }
            
            if (transform.childCount > 1)
            {
                //transform.GetChild(1).gameObject.SetActive(false);
                //transform.GetChild(1).gameObject.hideFlags |= HideFlags.HideAndDontSave;

                //recycle.Add(transform.GetChild(1).gameObject);
                addToRecycle(transform.GetChild(1).gameObject);
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

            int recycle_count;

            lock (recycle)
            {
                recycle_count = recycle.Count;
            }


            if (recycle_count > 0)
            {
                need_a_new_one = false;
                newone = remFirstFromRecycle();
                if (Decay_Time < 0.0001f)
                    clearRecycle();
            }


            if (need_a_new_one)
            {
                newone = Instantiate(points.transform).gameObject;
                newone.transform.SetParent(transform, false);
                newone.hideFlags |= HideFlags.HideAndDontSave;
                //newone = ((instantiate a copy of the template))
                newone.GetComponent<LaserScanView>().Recylce += (oldScan) =>
                {
                    addToRecycle(oldScan);
                };

                newone.GetComponent<LaserScanView>().IDied += (deadScan) =>
                {
                    
                    remFromRecycle(deadScan);
                    deadScan.transform.SetParent(null); //disconnect from parent
                    Destroy(deadScan); //destroy object

                };
            }

            KeyValuePair<uint, LaserScan> oldest = remFirstFromToDraw();
            newone.GetComponent<LaserScanView>().SetScan(Time.fixedTime, oldest.Value);


        }

    }


    /**
        Recycle and ToDraw interface(s) for adding and removing elements safely
    **/

    #region ToDraw interface
    void addToDraw(LaserScan scanIn)
    {

        lock (toDraw)
        {
            toDraw.Add(scanIn.header.seq, scanIn);
        }
    }

    KeyValuePair<uint, LaserScan> remFirstFromToDraw()
    {
        KeyValuePair<uint, LaserScan> scanSeqPairOut;
        lock (toDraw)
        {
            scanSeqPairOut = toDraw.First();
            toDraw.Remove(scanSeqPairOut.Key);
        }
        return scanSeqPairOut;
    }
    #endregion

    #region Recycle interface
    void addToRecycle(GameObject gameObjIn)
    {
        lock (recycle)
        {
            recycle.Add(gameObjIn);
        }
    }

    GameObject remFirstFromRecycle()
    {
        GameObject gameObjOut;
        lock (recycle)
        {
            //recycle.Add(gameObjIn);
            gameObjOut = recycle.First();
            recycle.RemoveAt(0);
        }
        return gameObject;
    }

    void remFromRecycle(GameObject gameObjOut)
    {
        lock (recycle)
        {
            recycle.Remove(gameObject);
        }

    }

    void clearRecycle()
    {
        lock(recycle)
            recycle.Clear();

    }
    #endregion
}