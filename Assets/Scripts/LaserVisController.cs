using UnityEngine;
using System.Collections;
using Messages.sensor_msgs;
using Ros_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

public class LaserVisController : SensorTFInterface
{
    SortedList<uint, LaserScan> toDraw = new SortedList<uint, LaserScan>();
    List<GameObject> recycle = new List<GameObject>();
    List<GameObject> active = new List<GameObject>();


    private GameObject points; //will become child(0), used for cloning
    private NodeHandle nh = null;
    private Subscriber<LaserScan> subscriber;


    public float pointSize = 1;
    //curently not in use
    private uint maxRecycle = 100;
    public float Decay_Time = 0f;

  
    public bool Debug_Messages = false;

    // Use this for initialization
    void Start()
    {

        ROSManager.GetComponent<ROSManager>().StartROS(() => {
            nh = new NodeHandle();
            subscriber = nh.subscribe<LaserScan>(topic, 1, scancb);
        });


        //get the TEMPLATE view (our only child 
        points = transform.GetChild(0).gameObject;
        points.hideFlags |= HideFlags.HideAndDontSave;
        points.SetActive(false);
        points.name = "Points";
       
    }

    private void scancb(LaserScan argument)
    {
   
        //toDraw.Add(argument.header.seq, argument);
        if(TFName == null || !TFName.Equals(argument.header.frame_id))
        {
            TFName = argument.header.frame_id;
        }
        addToDraw(argument);
    }

    // Update is called once per frame
    void Update()
    {
        if (Decay_Time < 0.0001f)
        {
           
            while (countToDraw() > 1)
            {
                remFirstFromToDraw();
            }
     
            while(countActive() > 1)
            {
                remFirstFromActive().GetComponent<LaserScanView>().recycle();
            }          
        }

        while (countToDraw() > 0)
        {
            GameObject newone = null;
            bool need_a_new_one = true;
 
            if (countRecycle() > 0)
            {
                need_a_new_one = false;
                newone = remFirstFromRecycle();
                /*
                if (Decay_Time < 0.0001f) //something fucky about this
                    clearRecycle();
                    */
            }


            if (need_a_new_one)
            {
                newone = Instantiate(points.transform).gameObject;
                newone.transform.SetParent(null, false);

                //newone.hideFlags |= HideFlags.HideAndDontSave;

                newone.GetComponent<LaserScanView>().Recylce += (oldScan) =>
                {
                    remFromActive(oldScan);
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
            newone.GetComponent<LaserScanView>().SetScan(Time.fixedTime, oldest.Value, gameObject, TF);
            addToActive(newone);

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
        KeyValuePair<uint, LaserScan> something = default(KeyValuePair<uint, LaserScan>);
        lock (toDraw)
        {
            scanSeqPairOut = toDraw.FirstOrDefault();
            if (!scanSeqPairOut.Equals(something))
            {
                toDraw.Remove(scanSeqPairOut.Key);
            }
        }
        return scanSeqPairOut;
    }

    int countToDraw()
    {
        int count;

        lock (toDraw)
        {
            count = toDraw.Count;
        }

        return count;
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
            gameObjOut = recycle.FirstOrDefault().gameObject;
            if (!gameObjOut.Equals(default(GameObject)))
            {
                recycle.RemoveAt(0);
            }
        }
        return gameObjOut;
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

    int countRecycle()
    {
        int count;

        lock (recycle)
        {
            count = recycle.Count;
        }

        return count;
    }
    #endregion

    #region Active interface
    void addToActive (GameObject gameObjIn)
    {
        lock(active)
        {
            active.Add(gameObjIn);
        }
    }

    GameObject getFromActive (int index)
    {
        GameObject gameObjOut;
        lock(active)
        {
            gameObjOut = active.ElementAt(index);
        }
        return gameObjOut;
    }

    GameObject remFirstFromActive()
    {
        GameObject gameObjOut;
        lock (active)
        {
            gameObjOut = active.ElementAt(0);
            active.RemoveAt(0);
        }
        return gameObjOut;
    }

    void remFromActive(GameObject gameObjToRem)
    {
        lock (active)
        {
            active.Remove(gameObjToRem);
        }
    }

    int countActive()
    {
        int count;
        lock (active)
            count = active.Count();

        return count;
    }

    #endregion
}