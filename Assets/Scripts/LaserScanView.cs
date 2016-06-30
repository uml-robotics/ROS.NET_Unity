using UnityEngine;
using System.Collections;
using Messages.sensor_msgs;
using System;
using tf.net;

public class LaserScanView : MonoBehaviour
{
    
    private float[] distBuffer;
    private GameObject[] pointBuffer;
    private bool changed;

    private GameObject goParent;
    private Transform TF;
    private uint recycleCount = 0;
    private float lastUpdate;
    private float lastRecycle = 0;
    private float angMin, angInc, maxRange, minRange;
    
    private float decay
    {
        get { return goParent == null ? 0f : goParent.gameObject.GetComponent<LaserViewController>().DecayTime; }
    }
    
    private float pointSize
    {
        get { return goParent == null ? 0.1f : goParent.gameObject.GetComponent<LaserViewController>().PointSize; }
    }

    private Color Color
    {
        get { return goParent == null ? new Color(1, 0, 0, 1) : goParent.gameObject.GetComponent<LaserViewController>().Color; }
    }

    public delegate void RecycleCallback(GameObject me);
    public event RecycleCallback Recylce;

    
    public delegate void IDiedCallback(GameObject me);
    public event IDiedCallback IDied;
    

    public void recycle()
    {
        // gameObject.hideFlags |= HideFlags.HideAndDontSave;
        gameObject.SetActive(false);
        if (Recylce != null)
            Recylce(gameObject);
    }

    
    internal void expire()
    {
        // gameObject.hideFlags |= HideFlags.HideAndDontSave;
        gameObject.SetActive(false);
        if (IDied != null)
            IDied(gameObject);
        
    }
    


    public void SetScan(float time, LaserScan msg, GameObject _goParent, Transform _TF)
    {
        goParent = _goParent;
        TF = _TF;
        recycleCount++;
        angMin = msg.angle_min;
        angInc = msg.angle_increment;
        maxRange = msg.range_max;
        minRange = msg.range_min;
        lastUpdate = time;

        if (distBuffer == null || distBuffer.Length != msg.ranges.Length)
            distBuffer = new float[msg.ranges.Length];

        Array.Copy(msg.ranges, distBuffer, distBuffer.Length);
        changed = true;

        //deactivate old points
        if (pointBuffer != null)
        {
            foreach (GameObject point in pointBuffer)
            {
                point.SetActive(false);
            }
        }
        
        //turn GO on
        gameObject.SetActive(true);
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
            #region SHOULD I be Recycled?
            if (decay > 0.0001 && (Time.fixedTime - lastUpdate) > decay)
            {
                if(lastRecycle < 0.0001f)
                {
                    lastRecycle = Time.fixedTime;
                }
                if ((Time.fixedTime - lastRecycle) > (decay + 1f))
                {
                    expire();
                    return;
                }
                lastRecycle = Time.fixedTime;
                
                recycle();
                return;
            }
            #endregion


            if (changed)
            {
                //show if hidden (this scan was recycled)
                //hideFlags &= ~HideFlags.HideAndDontSave;

                #region RESIZE IF NEEDED, ADD+REMOVE SPHERES AS NEEDED
                //resize sphere array if different from distbuffer
                //remath all circles based on distBuffer
                if (pointBuffer != null && pointBuffer.Length != distBuffer.Length)
                {
                    int oldsize = pointBuffer.Length;
                    int newsize = distBuffer.Length;
                    if (oldsize < newsize)
                    {
                        Array.Resize(ref pointBuffer, newsize);
                        for (int i = oldsize; i < newsize; i++)
                        {
                            GameObject newsphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            newsphere.transform.SetParent(transform);
                            pointBuffer[i] = newsphere;
                        }
                    }
                    else
                    {
                        for (int i = oldsize; i >= newsize; i--)
                        {
                            pointBuffer[i].transform.SetParent(null);
                            pointBuffer[i] = null;
                        }
                        Array.Resize(ref pointBuffer, newsize);
                    }
                }
                else if (pointBuffer == null)
                {
                    pointBuffer = new GameObject[distBuffer.Length];
                    for (int i = 0; i < distBuffer.Length; i++)
                    {
                        GameObject newsphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        newsphere.transform.SetParent(transform);
                        pointBuffer[i] = newsphere;
                    }
                }
                #endregion

            #region FOR ALL SPHERES ALL THE TIME
            for (int i = 0; i < pointBuffer.Length; i++)
            {
                pointBuffer[i].SetActive(false);
                if (distBuffer[i] > (maxRange - 0.0001f) || distBuffer[i] < (minRange + 0.0001f))
                    {
                        continue;
                    }
                pointBuffer[i].transform.localScale = new Vector3(pointSize, pointSize, pointSize);
                Vector3 parentPos = TF.position;
                emQuaternion rot = new emQuaternion(0, 0, 0, 1);
                emVector3 pos = new emVector3((float)(distBuffer[i] * Math.Cos(angMin + angInc * i)), (float)(distBuffer[i] * Math.Sin(angMin + angInc * i)), 0f);
                pointBuffer[i].transform.localPosition = pos.UnityPosition;
                pointBuffer[i].transform.localRotation = rot.UnityRotation;
                pointBuffer[i].GetComponent<MeshRenderer>().material.color = Color;
                pointBuffer[i].SetActive(true);
            }
            //save position, reset pos of scan views to be their old pose until updates finish
            //gameObject.SetActive(false);
            transform.position = TF.position;
            transform.rotation = TF.rotation;
            //gameObject.SetActive(true);

            #endregion
            changed = false;
            }
        
    }
}