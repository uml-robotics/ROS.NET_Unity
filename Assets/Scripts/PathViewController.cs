using UnityEngine;
using System.Collections.Generic;
using gm = Messages.geometry_msgs;
using Messages.nav_msgs;
using Ros_CSharp;
using System.Linq;

public class PathViewController : SensorTFInterface<Path> {

    private Path currentMsg = new Path();
    private List<GameObject> path = new List<GameObject>();
    private int pointCount{ get { return currentMsg.poses == null ? 0 : currentMsg.poses.Length;} }
    private const float pointSize = 0.05f;
    public Vector3 Offset = new Vector3(0, 0, 0);
    public Color Color = new Color(0, 0, 1, 1);

    protected override void Callback(Path msg)
    {
        lock(currentMsg)
        {
            currentMsg = msg;
        }   
    }

    // Use this for initialization
    protected override void Start () {
        base.Start();
    }
	
	// Update is called once per frame
	protected override void Update () {
        lock (currentMsg)
        {
            lock(path)
            {

                while (path.Count > pointCount)
                {
                    Destroy(path[0]);
                    path.RemoveAt(0);
                }

                while (path.Count < pointCount)
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.hideFlags |= HideFlags.HideInHierarchy;
                    path.Add(go);
                }

                for (int index = 0; index < pointCount; ++index)
                {
                    path[index].transform.localScale = new Vector3(pointSize, pointSize, pointSize);
                    tf.net.emVector3 pos = new tf.net.emVector3((float)currentMsg.poses[index].pose.position.x, currentMsg.poses[index].pose.position.y, currentMsg.poses[index].pose.position.z);
                    path[index].transform.position = pos.UnityPosition + Offset;
                    path[index].GetComponent<MeshRenderer>().material.color = Color;
                }
            }
        }
    }

    void OnDisable()
    {
        lock(path)
        while (path.Count > 0)
        {
            if (path[0] != null)
            {
                    Destroy(path[0]);
                    path.RemoveAt(0);
            }
            else
            {
                path.RemoveAt(0);
            }

        }
    }


}
