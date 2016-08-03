using UnityEngine;
using System.Collections.Generic;
using Messages.sensor_msgs;
using Messages.nav_msgs;


public class RobotCameraView : SensorTFInterface<CompressedImage>
{
    public string OdomTopic = "odom_ground_truth";
    public float ScreenSize = 0.05f;

    private Object odometryMutex = new Object();
    private Vector3 currentPos;
    private Quaternion currentQuat;
    private GameObject cameraView;
    // Use this for initialization
    protected override void Start () {
        base.Start();

        nh.subscribe<Odometry>(NameSpace + "/" + OdomTopic, 1, odomCallback);
        
        cameraView = GameObject.CreatePrimitive(PrimitiveType.Plane);
        cameraView.transform.parent = transform;

        //transform camera to be oriented properly
        cameraView.transform.localScale = new Vector3(ScreenSize, ScreenSize, ScreenSize);
        cameraView.transform.localPosition += new Vector3(0, .01f, ScreenSize * 10);
        cameraView.transform.localEulerAngles = new Vector3(0, 180, 0);

        CompressedImageDisplay cid = cameraView.AddComponent<CompressedImageDisplay>();
        cid.topic = this.NameSpace + "/" + this.Topic;
	}

    //for getting the position of the object we're following
    private void odomCallback(Odometry msg)
    {
        lock(odometryMutex)
        currentPos = RosPointToVector3(msg.pose.pose.position);
        currentQuat = RosToUnityQuat(msg.pose.pose.orientation);
    }
    //must overide TFsensorInterface, should change this to be optional
    protected override void Callback(CompressedImage msg)
    {
        return;
    }

    //set position of transform based on odom topic
    protected override void  Update () {
        //base.Update();//not necessary since we're not using the CompressImage topic's Tf (since it doesn't have a propper TF)
        lock (odometryMutex)
        transform.position = currentPos;
        transform.rotation = currentQuat;

	}

    //for doing conversions
    Quaternion RosToUnityQuat(Messages.geometry_msgs.Quaternion Ros_Quat)
    {
        return new tf.net.emQuaternion(Ros_Quat).UnityRotation;
    }

    Vector3 RosPointToVector3(Messages.geometry_msgs.Point ROS_Point)
    {
        return new tf.net.emVector3(ROS_Point.x, ROS_Point.y, ROS_Point.z).UnityPosition;
    }
}


