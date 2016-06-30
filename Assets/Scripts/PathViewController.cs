using UnityEngine;
using System.Collections.Generic;
using gm = Messages.geometry_msgs;
using Messages.nav_msgs;
using Ros_CSharp;

public class PathViewController : SensorTFInterface<Path> {

    private Path currentMsg = new Path();
    private List<GameObject> path = new List<GameObject>();
    private int pointCount{ get { return currentMsg.poses == null ? 0 : currentMsg.poses.Length;} }

    protected override void Callback(Path msg)
    {
        lock(currentMsg)
        {
            currentMsg = msg;
            //Debug.Log("Array Size: " + msg.poses.Length.ToString());
        }   
    }
    //TODO look for another way to make line or recyle points
    //Add color
    //clean up

    // Use this for initialization
    protected override void Start () {
        base.Start();
    }
	
	// Update is called once per frame
	protected override void Update () {
        lock (currentMsg)
        {

            while (path.Count > pointCount)
            {
                Destroy(path[0]);
                path.RemoveAt(0);
            }

            while(path.Count < pointCount)
            {
                path.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            }

            for(int index = 0; index < pointCount; ++index)
            {
                path[index].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                path[index].transform.position = new Vector3((float) -currentMsg.poses[index].pose.position.y, (float)currentMsg.poses[index].pose.position.z, (float)currentMsg.poses[index].pose.position.x);
            }
       
            if(currentMsg.poses != null)
            foreach(gm.PoseStamped Pose in currentMsg.poses)
            {
                    Debug.Log("X: " + -Pose.pose.position.y + ", Y: " + Pose.pose.position.z + ", Z: " + Pose.pose.position.x);
            }
        }
    }


}
