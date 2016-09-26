using UnityEngine;
using System.Collections.Generic;
using Messages.sensor_msgs;
using Messages.nav_msgs;

public class RobotCameraView : SensorTFInterface<CompressedImage>
{
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;
    public float ScreenSize = 0.05f;
    public string frame_id;
    private GameObject cameraView;
    private CompressedImageDisplay imageDisplay;
    // Use this for initialization
    protected override void Start ()
    {
        TFName = NameSpace + (frame_id.StartsWith("/") ? "" : "/") + frame_id;

        cameraView = transform.GetChild(0).gameObject;
        cameraView.SetActive(true);
        //transform camera to be oriented properly
        cameraView.transform.localScale = new Vector3(ScreenSize, ScreenSize, ScreenSize);
        cameraView.transform.localPosition = PositionOffset;
        cameraView.transform.localEulerAngles = RotationOffset;

        imageDisplay = cameraView.AddComponent<CompressedImageDisplay>();
        imageDisplay.image_topic = this.NameSpace + "/" + this.Topic;
	}

    //set position of transform based on odom topic
    protected override void  Update ()
    {
        MakeChildOfTF = false;
        base.Update();
        cameraView.transform.localScale = new Vector3(ScreenSize, ScreenSize, (1f / imageDisplay.GetAspectRatio()) * ScreenSize);
        cameraView.transform.localPosition = PositionOffset;
        cameraView.transform.localEulerAngles = RotationOffset;
    }
}


