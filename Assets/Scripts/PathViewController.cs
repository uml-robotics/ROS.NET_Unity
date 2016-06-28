using UnityEngine;
using System.Collections.Generic;
using gm = Messages.geometry_msgs;
using Messages.nav_msgs;
using Ros_CSharp;

public class PathViewController : SensorTFInterface<Path> {

    private NodeHandle nh = null;
    private Subscriber<Path> subscriber = null;
    private Path currentMsg = new Path();
    private List<GameObject> path = new List<GameObject>();
    private int pointCount{ get { return currentMsg.poses == null ? 0 : currentMsg.poses.Length;} }

    void callBackm(Path msg)
    {
        lock(currentMsg)
        {
            currentMsg = msg;
            //Debug.Log("Array Size: " + msg.poses.Length.ToString());
        }   
    }

	// Use this for initialization
	new void Start () {
        base.Start();


        rosmanager.StartROS(this, () => {
            nh = new NodeHandle();
            subscriber = nh.subscribe<Path>(NameSpace + Topic, 1, callBackm);
        });
    }
	
	// Update is called once per frame
	new void Update () {
        base.Update();
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
