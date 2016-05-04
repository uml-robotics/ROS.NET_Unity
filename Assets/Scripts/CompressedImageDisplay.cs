//#define WITH_HEADER
using System;
using System.Threading;
using Messages.nav_msgs;
using Messages.sensor_msgs;
using Microsoft.Win32.SafeHandles;
using Ros_CSharp;
using UnityEngine;
using UnityEditor;
using System.Collections;

public class CompressedImageDisplay : MonoBehaviour
{
    public string map_topic;

    private NodeHandle nh = null;
    private Subscriber<CompressedImage> mapsub;
    
    private uint pwidth=2, pheight=2;

    private MeshRenderer renderer = null;
    private Texture2D mapTexture = null;

    private CompressedImage lastimage = null;

    private AutoResetEvent textureMutex = new AutoResetEvent(false);

	// Use this for initialization
    private void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        if (ROSManager.StartROS())
        {
            nh = new NodeHandle();
            mapsub = nh.subscribe<CompressedImage>(map_topic, 1, mapcb);
        }
    }

    private void mapcb(CompressedImage msg)
    {
        lastimage = msg;
        textureMutex.Set();
    }

    // Update is called once per frame
	void Update () {
	    if (textureMutex.WaitOne(0))
	    {
	        if (mapTexture == null)
                mapTexture = new Texture2D(2,2);
	        mapTexture.LoadImage(lastimage.data);
            //DebugText.WriteLine("Texture size = " + mapTexture.width + "x" + mapTexture.height + ", format=" + mapTexture.format);
            renderer.material.mainTexture = mapTexture;
	    }
	}
}
