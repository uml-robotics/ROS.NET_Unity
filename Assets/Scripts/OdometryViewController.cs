using UnityEngine;
using System.Collections.Generic;
using Ros_CSharp;
using Messages.nav_msgs;

public class OdometryViewController : SensorTFInterface<Odometry> {

    public double PositionTolerance = 0.1d; //Distance from last arrow
    public double AngleTolerance = 0.1d; //Angular distance from the last arrow
    public int Keep = 100; //number of arrows to keep
    public float ArrowLength = 0.4f; //length of arrow
    private float oldArrowLength;

    private NodeHandle nh = null;
    private Subscriber<Odometry> subscriber;

    private Odometry currentMsg = new Odometry(); //Odometry must be initialized since a null can not be locked
    private Vector3 currentPos;
    private Vector3 lastPos;
    private Quaternion currentQuat;
    private Quaternion lastQuat;

    private Queue<GameObject> Arrows = new Queue<GameObject>();
    private GameObject arrowGO; //arrow gameobject used to represent orientation of object

    private void callBack(Odometry msg)
    {
        lock(currentMsg)
        {
            currentMsg = msg;
            currentPos = RosPointToVector3(msg.pose.pose.position);
            currentQuat = RosToUnityQuat(msg.pose.pose.orientation);
        }
    }

    // Use this for initialization
   new void Start () {
        oldArrowLength = ArrowLength;
        rosmanager.StartROS(this, () => {
            nh = new NodeHandle();
            subscriber = nh.subscribe<Odometry>(NameSpace + _Topic, 1, callBack);
        });

        arrowGO = transform.GetChild(0).gameObject;
        arrowGO.SetActive(false);
    }

    // Update is called once per frame
  new void Update() {
        while (Arrows.Count > Keep)
        {
            Destroy(Arrows.Dequeue());
        }

        if(oldArrowLength != ArrowLength)
        {
            foreach(GameObject arrow in Arrows)
            {
                arrow.transform.localScale = new Vector3(ArrowLength, ArrowLength, ArrowLength);
            }
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

            //TODO make Angle tolerance better reflect Rviz. UPDATE Rviz has some weird ass voodoo scalling for their angle tolerance
            //TODO update all arrows when size changes
            if ( (currentPos - lastPos).magnitude > (PositionTolerance) || (Mathf.Pow(Mathf.DeltaAngle(currentQuat.eulerAngles.y, lastQuat.eulerAngles.y), 2)/14400 > AngleTolerance ))
            {
                GameObject arrow = Instantiate(arrowGO);
                arrow.transform.rotation = (currentQuat * Quaternion.Euler(90, 0, 0));
                arrow.transform.position = arrow.transform.TransformVector(new Vector3(0f, ArrowLength, 0f)) + currentPos;
                arrow.SetActive(true);
                arrow.transform.localScale = new Vector3(ArrowLength, ArrowLength, ArrowLength);
                arrow.hideFlags |= HideFlags.HideInHierarchy;
                Arrows.Enqueue(arrow);
                lastPos = currentPos;
                lastQuat = currentQuat;
            }
        }
        
    }
    
    Quaternion RosToUnityQuat(Messages.geometry_msgs.Quaternion Ros_Quat)
    {
        return new Quaternion((float)Ros_Quat.y, -(float)Ros_Quat.z, (float)Ros_Quat.x, (float)Ros_Quat.w); //tempfix: swapping y and x seemed to fix orientation issues
    }

    Vector3 RosPointToVector3(Messages.geometry_msgs.Point ROS_Point)
    {
        //return new UnityEngine.Vector3((float)ROS_Point.x, (float)ROS_Point.z, (float)ROS_Point.y);
        return new Vector3(-(float)ROS_Point.y, (float)ROS_Point.z, (float)ROS_Point.x); //tempfix: y and x appear to be swapped in message or orientation in scene is off?
    }
}
