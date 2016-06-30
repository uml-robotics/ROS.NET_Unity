using UnityEngine;
using System.Collections.Generic;
using Ros_CSharp;
using Messages.nav_msgs;

public class OdometryViewController : SensorTFInterface<Odometry> {

    public double PositionTolerance = 0.1d; //Distance from last arrow
    public double AngleTolerance = 0.1d; //Angular distance from the last arrow
    public int Keep = 100; //number of arrows to keep
    public float ArrowLength = 0.4f; //length of arrow
    public Color Color = new Color(1, 0, 0, 1);
    private float oldArrowLength;

    private Odometry currentMsg = new Odometry(); //Odometry must be initialized since a null can not be locked
    private Vector3 currentPos;
    private Vector3 lastPos;
    private Quaternion currentQuat;
    private Quaternion lastQuat;

    private Queue<GameObject> Arrows = new Queue<GameObject>();
    private GameObject arrowGO; //arrow gameobject used to represent orientation of object

    protected override void Callback(Odometry scan)
    {
        lock(currentMsg)
        {
            currentMsg = scan;
            currentPos = RosPointToVector3( scan.pose.pose.position);
            currentQuat = RosToUnityQuat(scan.pose.pose.orientation);
        }
    }

    // Use this for initialization
    protected override void Start () {
        base.Start();
        oldArrowLength = ArrowLength;
        arrowGO = transform.GetChild(0).gameObject;
        arrowGO.SetActive(false);
    }

    // Update is called once per frame
   protected override void Update() {
        lock(Arrows)
        {
            //base.Update() Odometry does not need it's transform handled
            while (Arrows.Count > Keep)
            {
                Destroy(Arrows.Dequeue());
            }

            if (oldArrowLength != ArrowLength)
            {
                foreach (GameObject arrow in Arrows)
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
                if ((currentPos - lastPos).magnitude > (PositionTolerance) || (Mathf.Pow(Mathf.DeltaAngle(currentQuat.eulerAngles.y, lastQuat.eulerAngles.y), 2) / 14400 > AngleTolerance))
                {
                    GameObject arrow = Instantiate(arrowGO);
                    arrow.transform.rotation = (currentQuat * Quaternion.Euler(90, 0, 0));
                    arrow.transform.position = arrow.transform.TransformVector(new Vector3(0f, ArrowLength, 0f)) + currentPos;
                    arrow.SetActive(true);
                    arrow.transform.localScale = new Vector3(ArrowLength, ArrowLength, ArrowLength);
                    arrow.hideFlags |= HideFlags.HideInHierarchy;

                    foreach (MeshRenderer mesh in arrow.GetComponentsInChildren<MeshRenderer>())
                    {

                        mesh.material.color = Color;
                    }

                    Arrows.Enqueue(arrow);
                    lastPos = currentPos;
                    lastQuat = currentQuat;
                }
            }
        }
    }
    
    Quaternion RosToUnityQuat(Messages.geometry_msgs.Quaternion Ros_Quat)
    {
        return new tf.net.emQuaternion(Ros_Quat).UnityRotation;
    }

    Vector3 RosPointToVector3(Messages.geometry_msgs.Point ROS_Point)
    {
        return new tf.net.emVector3(ROS_Point.x, ROS_Point.y, ROS_Point.z).UnityPosition;
    }

    void OnDisable()
    {
        lock (Arrows)
        while (Arrows.Count > 0)
        {
            if (Arrows.Peek() != null)
            {
                Destroy(Arrows.Dequeue());
            }
            else
            {
                Arrows.Dequeue();
            }

        }
    }
}
