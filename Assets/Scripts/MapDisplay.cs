//#define WITH_HEADER
using System;
using System.Threading;
using Messages.nav_msgs;
using Microsoft.Win32.SafeHandles;
using Ros_CSharp;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class MapDisplay : ROSMonoBehavior
{
    public string map_topic;
    public string map_metadata_topic;

    private NodeHandle nh = null;
    private Subscriber<OccupancyGrid> mapsub;
    private Subscriber<MapMetaData> metadatasub;

    private Vector3 mapPos = new Vector3();
    private Quaternion mapRot = Quaternion.identity;
    private Vector3 mapScale = new Vector3();

    private float width, height;
    private uint pwidth, pheight;

    private byte[] imageData = new byte[0];
    private Texture2D mapTexture = null;

    private AutoResetEvent textureMutex = new AutoResetEvent(false);

	// Use this for initialization
    private void Start()
    {
        rosmanager.StartROS(this,() => {
            nh = new NodeHandle();
            mapsub = nh.subscribe<OccupancyGrid>(map_topic, 1, mapcb);
            metadatasub = nh.subscribe<MapMetaData>(map_metadata_topic, 1, metadatacb);
        });
    }
    private void SetDimensions(uint w, uint h, float res, Messages.geometry_msgs.Point position, Messages.geometry_msgs.Quaternion orientation)
    {
        pwidth = w;
        pheight = h;
        width = w * res;
        height = h * res;
        mapPos = new Vector3((float)position.x + (width / 2f), (float)position.y + (height / 2f), (float)position.z);
        //mapRot = new Quaternion((float) orientation.x, (float) orientation.y, (float) orientation.z, (float) orientation.w);
        mapScale = new Vector3(height, width, 1f);
    }

    private void mapcb(OccupancyGrid msg)
    {
        SetDimensions(msg.info.width, msg.info.height, msg.info.resolution, msg.info.origin.position, msg.info.origin.orientation);
        createARGB(msg.data, ref imageData);
        textureMutex.Set();
    }

    private void metadatacb(MapMetaData msg)
    {
        SetDimensions(msg.width, msg.height, msg.resolution, msg.origin.position, msg.origin.orientation);
    }

    // Update is called once per frame
	void Update () {
        transform.localPosition = mapPos;
        transform.localRotation = mapRot;
        transform.localScale = mapScale;
	    if (textureMutex.WaitOne(0))
	    {
	        if (mapTexture == null || mapTexture.width != pwidth || mapTexture.height != pheight)
	        {
	            mapTexture = new Texture2D((int) pwidth, (int) pheight, TextureFormat.ARGB32, false, true);
                GetComponent<MeshRenderer>().material.mainTexture = mapTexture;
                mapTexture.LoadRawTextureData(imageData);
	        }
	        mapTexture.Apply();
	    }
	}


#region occupancy grid to ARGB32
    private void createARGB(sbyte[] map, ref byte[] image)
    {
        if (image == null || (image.Length / 4) != map.Length)
            image = new byte[(4 * map.Length)];
        for (int i = 0, j = 0; i < image.Length && j < map.Length; i += 4, j++)
        {
            image[i] = 0xFF;
            switch (map[j])
            {
                case -1:
                    image[i + 1] = image[i + 2] = image[i + 3] = 211;
                    break;
                case 100:
                    image[i + 1] = image[i + 2] = image[i + 3] = 105;
                    break;
                case 0:
                    image[i + 1] = image[i + 2] = image[i + 3] = 255;
                    break;
                default:
                    image[i + 1] = 255;
                    image[i + 2] = image[i + 3] = 0;
                    break;
            }
        }
    }
#endregion
}
