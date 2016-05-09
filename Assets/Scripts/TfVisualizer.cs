using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using XmlRpc_Wrapper;
using gm = Messages.geometry_msgs;
using Messages.tf;
using tf.net;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ros_CSharp;

public class TfVisualizer : MonoBehaviour
{
    private NodeHandle nh = null;
    private Subscriber<Messages.tf.tfMessage> tfsub;
    private Text textmaybe;
    private Queue<Messages.tf.tfMessage> transforms = new Queue<Messages.tf.tfMessage>();

    private volatile int lasthz = 0;
    private volatile int count = 0;
    private DateTime last = DateTime.Now;

    public Transform Template;
    public Transform Root;

    public string FixedFrame;

    private Dictionary<string, Transform> tree = new Dictionary<string, Transform>();

	// Use this for initialization
	void Start ()
    {
        if (Template != null)
        {
            Template.gameObject.SetActive(false);
        }
	    Root.GetComponentInChildren<TextMesh>().text = FixedFrame;
        tree[FixedFrame] = Root;

	    if (ROSManager.StartROS())
	    {
            nh = new NodeHandle();
            tfsub = nh.subscribe<Messages.tf.tfMessage>("/tf", 0, tf_callback, false);
	    }
    }

    private void tf_callback(tfMessage msg)
    {
        lock (transforms)
        {
            transforms.Enqueue(msg);
            DateTime now = DateTime.Now;
            count++;
            if (now.Subtract(last).TotalMilliseconds > 1000)
            {
                lasthz = count;
                count = 0;
                last = now;
            }
        }
    }

    private bool IsVisible(string child_frame_id)
    {
        //TODO rviz style checkboxes?
        return true;
    }

    // Update is called once per frame
	void Update ()
	{
	    Queue<Messages.tf.tfMessage> tfs = null;
	    lock (transforms)
	    {
	        if (transforms.Count > 0)
	        {
	            tfs = new Queue<tfMessage>(transforms);
	            transforms.Clear();
	        }
	    }
	    emTransform tf = null;
	    while (tfs != null && tfs.Count > 0)
	    {
	        tf = new emTransform(tfs.Dequeue().transforms[0]);
            if (!tf.frame_id.StartsWith("/"))
                tf.frame_id = "/"+tf.frame_id;
            if (!tf.child_frame_id.StartsWith("/"))
                tf.child_frame_id = "/"+tf.child_frame_id;
	        if (IsVisible(tf.child_frame_id))
	        {
	            Vector3 pos = new Vector3((float) -tf.origin.x, (float) tf.origin.y, (float) tf.origin.z);
	            Quaternion rot = new Quaternion((float) tf.basis.x, (float) tf.basis.y, (float) tf.basis.z, (float) tf.basis.w);
                /*if (rot != Quaternion.identity)
                    DebugText.WriteLine(""+tf.child_frame_id+" "+tf.basis);*/
	            if (!tree.ContainsKey(tf.child_frame_id))
	            {
	                Transform newframe = (Transform) Instantiate(Template, pos, rot);
	                tree[tf.child_frame_id] = newframe;
	                tree[tf.child_frame_id].gameObject.GetComponentInChildren<TextMesh>().text = tf.child_frame_id;
	            }
	            if (tree.ContainsKey(tf.frame_id))
	            {
	                tree[tf.child_frame_id].SetParent(tree[tf.frame_id], false);
	                tree[tf.child_frame_id].gameObject.SetActive(true);
	            }
	            else
	                tree[tf.child_frame_id].gameObject.SetActive(false);
	            tree[tf.child_frame_id].localPosition = pos;
	            tree[tf.child_frame_id].localRotation = rot;
	        }
	    }
	}
}
