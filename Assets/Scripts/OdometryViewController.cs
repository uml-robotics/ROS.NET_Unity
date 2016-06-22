using UnityEngine;
using System.Collections.Generic;
using System;
using Ros_CSharp;
using Messages.nav_msgs;
using Messages.geometry_msgs;

public class OdometryViewController : SensorTFInterface {

    public double positionTolerance = 0.1d; //Distance from last arrow
    public double angleTolerance = 0.1d; //Angular distance from the last arrow
    public int keep = 100; //number of arrows to keep
    public float arrowLength = 0.4f; //length of arrow
    public bool debugMsgs = false;

    private NodeHandle nh = null;
    private Subscriber<Odometry> subscriber;

    private Odometry currentMsg = new Odometry(); //Odometry must be initialized since a null can not be locked
    private UnityEngine.Vector3 currentPos;
    private UnityEngine.Vector3 lastPos;
    private UnityEngine.Quaternion currentQuat;
    private UnityEngine.Quaternion lastQuat;

    private Queue<GameObject> Arrows = new Queue<GameObject>();
    private GameObject arrowGO; //arrow gameobject used to represent orientation of object

    private void callBack(Odometry scan)
    {
        lock(currentMsg)
        {
            currentMsg = scan;
            currentPos = RosPointToVector3( scan.pose.pose.position);
            currentQuat = RosToUnityQuat(scan.pose.pose.orientation);
        }
    }

    // Use this for initialization
    void Start () {
        TFName = "agent1/base_link";// TF used for debugging purposses
        rosmanager.StartROS(this, () => {
            nh = new NodeHandle();
            subscriber = nh.subscribe<Odometry>(topic, 1, callBack);
        });

        arrowGO = transform.GetChild(0).gameObject;
        arrowGO.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        while (Arrows.Count > keep)
        {
            Destroy(Arrows.Dequeue());
        }

        lock (currentMsg)
        {
           
            if (currentMsg.pose == null)
                return;

            if (lastPos == null || lastQuat == null)
            {
                lastQuat = currentQuat;
                lastPos = currentPos;
                return;
            }

            if (debugMsgs)
            {
                //UnityEngine.Debug.Log("Scan: " + ((currentMsg.twist == null || currentMsg.pose == null) ? "Empty" : (" x: " + currentMsg.pose.pose.position.x.ToString() + " y: " + currentMsg.pose.pose.position.y.ToString() + " z: " + currentMsg.pose.pose.position.z.ToString())));
                //UnityEngine.Debug.Log("Scan: " + ((currentMsg.twist == null || currentMsg.pose == null) ? "Empty" : (" delta x: " + posDelta(lastPos, currentPos).x.ToString() + " delta y: " + posDelta(lastPos, currentPos).y.ToString() + " delta z: " + posDelta(lastPos, currentPos).z.ToString())));
                UnityEngine.Debug.Log("Trans rot: " + (TF == null ? "Nothing" : TF.localRotation.eulerAngles.ToString()));
                UnityEngine.Debug.Log("before rot: " + ((currentMsg.twist == null || currentMsg.pose == null) ? "Empty" : (" x: " + currentQuat.x.ToString() + " y: " + currentQuat.y.ToString() + " z: " + currentQuat.z.ToString() + " w: " + currentQuat.w.ToString())));
                UnityEngine.Debug.Log("aftter rot: " + currentQuat.eulerAngles.ToString());

            }
            
            //TODO make Angle tolerance better reflect Rviz. 
            if ( (currentPos - lastPos).magnitude > (positionTolerance + arrowLength) || (Mathf.Abs(((currentQuat.eulerAngles - lastQuat.eulerAngles).y * Mathf.Deg2Rad)) * arrowLength) > angleTolerance )
            {
                GameObject arrow = Instantiate(arrowGO);
                arrow.transform.position = currentPos;
                arrow.transform.rotation = (currentQuat * UnityEngine.Quaternion.Euler(90, 0, 0));
                arrow.SetActive(true);
                UnityEngine.Transform shaft = arrow.transform.Find("shaft");
                UnityEngine.Transform head = arrow.transform.Find("head");
                shaft.localScale = new UnityEngine.Vector3(shaft.localScale.x, arrowLength - 0.1f, shaft.localScale.z); //arrow head is roughly 0.1f long
                head.localPosition = new UnityEngine.Vector3(head.localPosition.x, arrowLength - 0.1f, head.localPosition.z);
                arrow.hideFlags |= HideFlags.HideInHierarchy;
                Arrows.Enqueue(arrow);
                lastPos = currentPos;
                lastQuat = currentQuat;
            }
        }
        
    }
    
    UnityEngine.Quaternion RosToUnityQuat(Messages.geometry_msgs.Quaternion Ros_Quat)
    {
        return new UnityEngine.Quaternion((float)Ros_Quat.y, -(float)Ros_Quat.z, (float)Ros_Quat.x, (float)Ros_Quat.w); //tempfix: swapping y and x seemed to fix orientation issues
    }

    UnityEngine.Vector3 RosPointToVector3(Messages.geometry_msgs.Point ROS_Point)
    {
        //return new UnityEngine.Vector3((float)ROS_Point.x, (float)ROS_Point.z, (float)ROS_Point.y);
        return new UnityEngine.Vector3(-(float)ROS_Point.y, (float)ROS_Point.z, (float)ROS_Point.x); //tempfix: y and x appear to be swapped in message or orientation in scene is off?
    }
}
