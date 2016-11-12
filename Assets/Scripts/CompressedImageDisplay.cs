//#define WITH_HEADER
using System;
using System.Threading;
using Messages.nav_msgs;
using Messages.sensor_msgs;
using Microsoft.Win32.SafeHandles;
using Ros_CSharp;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class CompressedImageDisplay : ROSMonoBehavior
{
    private string _topic;
    public string topic { get { return _topic; }
        set { if (value == null || value.Length == 0) return;
            _topic = value;
            image_topic = _topic;
            if (nh != null) {
                if (mapsub != null) {
                    mapsub.unsubscribe();
                    mapsub.shutdown();
                }
                mapsub = nh.subscribe<CompressedImage>(topic, 1, mapcb);
            } } }
    public string image_topic;

    private NodeHandle nh = null;
    private Subscriber<CompressedImage> mapsub;
    
    private uint pwidth=2, pheight=2;

    private MeshRenderer rend = null;
    private Texture2D mapTexture = null;

    private CompressedImage lastimage = null;

    private AutoResetEvent textureMutex = new AutoResetEvent(false);

    private float aspectRatio = 1.0f;
    public float GetAspectRatio()
    {
        return aspectRatio;
    }

	// Use this for initialization
    private void Start()
    {
        rend = GetComponent<MeshRenderer>();
        rosmanager.StartROS(this, () =>
                                                           {
                                                               nh = new NodeHandle();
                                                               if (image_topic != null && image_topic.Length > 0)
                                                                   topic = image_topic;
                                                               else
                                                                   topic = topic;
                                                           });
    }

    private void mapcb(CompressedImage msg)
    {
        lastimage = msg;
        textureMutex.Set();
    }

    // Update is called once per frame
	void Update () {
        if (image_topic != topic)
            topic = image_topic;
	    if (textureMutex.WaitOne(0))
	    {
	        if (mapTexture == null)
                mapTexture = new Texture2D(2,2);

	        mapTexture.LoadImage(lastimage.data);
	        aspectRatio = 1.0f*mapTexture.width/mapTexture.height;
            rend.material.mainTexture = mapTexture;
	    }
	}
}
