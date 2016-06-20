using UnityEngine;
using System.Collections;
using System;
using Ros_CSharp;
using Messages.nav_msgs;
using Messages.geometry_msgs;

public class OdometryViewController : SensorTFInterface {

    public double angleTolerance = 0.1d;
    public double positionTolerance = 0.1d;
    public bool debugMsgs = false;

    private NodeHandle nh = null;
    private Subscriber<Odometry> subscriber;

    private Odometry currentMsg = new Odometry();
    private Point currentPos;
    private Messages.geometry_msgs.Vector3 currentTwist;

    private Odometry lastMsg = new Odometry();
    private Point lastPos;
    private Messages.geometry_msgs.Vector3 lastTwist;

    private GameObject arrowGO;

    private void callBack(Odometry scan)
    {
        lock(currentMsg)
        {
                currentMsg = scan;
                currentPos = scan.pose.pose.position;
                currentTwist = scan.twist.twist.linear;
                    
        }
    }

    // Use this for initialization
    void Start () {
        TFName = "agent1/base_link";
        rosmanager.StartROS(this, () => {
            nh = new NodeHandle();
            subscriber = nh.subscribe<Odometry>(topic, 1, callBack);
        });

        arrowGO = transform.GetChild(0).gameObject;
        arrowGO.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        lock (currentMsg)
        {
            UnityEngine.Quaternion rot = new UnityEngine.Quaternion();
            if (currentMsg.pose.pose != null)
                rot = new UnityEngine.Quaternion((float)currentMsg.pose.pose.orientation.x + 0.5f, -(float)currentMsg.pose.pose.orientation.z, (float)currentMsg.pose.pose.orientation.y + 0.5f, (float)currentMsg.pose.pose.orientation.w);

            if (debugMsgs)
            {
               UnityEngine.Vector3 qn;
                UnityEngine.Debug.Log("Scan: " + ((currentMsg.twist == null || currentMsg.pose == null) ? "Empty" : (" x: " + currentMsg.pose.pose.position.x.ToString() + " y: " + currentMsg.pose.pose.position.y.ToString() + " z: " + currentMsg.pose.pose.position.z.ToString())));
                UnityEngine.Debug.Log("Scan: " + ((currentMsg.twist == null || currentMsg.pose == null) ? "Empty" : (" delta x: " + posDelta(lastPos, currentPos).x.ToString() + " delta y: " + posDelta(lastPos, currentPos).y.ToString() + " delta z: " + posDelta(lastPos, currentPos).z.ToString())));
                UnityEngine.Debug.Log("Trans rot: " + (TF == null ? "Nothing" : TF.rotation.ToString()));
                UnityEngine.Debug.Log("Scan: " + ((currentMsg.twist == null || currentMsg.pose == null) ? "Empty" : (" x: " + currentMsg.pose.pose.orientation.x.ToString() + " y: " + currentMsg.pose.pose.orientation.y.ToString() + " z: " + currentMsg.pose.pose.orientation.z.ToString() + " w: " + currentMsg.pose.pose.orientation.w.ToString())));
               // qn = new UnityEngine.Quaternion(-(float)currentMsg.pose.pose.orientation.x, -(float)currentMsg.pose.pose.orientation.y, (float)currentMsg.pose.pose.orientation.z, (float)currentMsg.pose.pose.orientation.w).eulerAngles;
                
                //UnityEngine.Debug.Log("Thing rot" + qn.ToString());
                
            }

            if (lastPos == null)
            {
                lastPos = currentPos;
                return;
            }
               

            if((posDelta(lastPos, currentPos).x + posDelta(lastPos, currentPos).y) > positionTolerance)
            {
                GameObject arrow = Instantiate(arrowGO);
                arrow.transform.position = new UnityEngine.Vector3((float) -currentPos.y, (float) currentPos.z, (float) currentPos.x);
                arrow.transform.rotation.eulerAngles.Set(360 - rot.eulerAngles.x, 360 - rot.eulerAngles.z, rot.eulerAngles.y);

                arrow.SetActive(true);
                lastPos = currentPos;
            }


        }
        
    }

    Point posDelta(Point pointA, Point pointB)
    {
        Point result = new Point();
        result.x = Math.Abs( Math.Abs(pointA.x) - Math.Abs(pointB.x));
        result.y = Math.Abs(Math.Abs(pointA.y) - Math.Abs(pointB.y));
        result.z = Math.Abs(Math.Abs(pointA.z) - Math.Abs(pointB.z));
        return result;

    }
}
