using UnityEngine;
using System.Collections.Generic;
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
            currentPos = new tf.net.emVector3(currentMsg.pose.pose.position.x, currentMsg.pose.pose.position.y, currentMsg.pose.pose.position.z).UnityPosition;
            currentQuat = new tf.net.emQuaternion(currentMsg.pose.pose.orientation).UnityRotation;
        }
    }

    // Use this for initialization
    protected override void Start () {
        base.Start();
        oldArrowLength = ArrowLength;
        arrowGO = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
   protected override void Update()
   {
       GameObject arrow = null;
        lock (currentMsg)
        {
            //TODO make Angle tolerance better reflect Rviz. UPDATE Rviz has some weird ass voodoo scalling for their angle tolerance
            if ((lastPos != null && (currentPos - lastPos).magnitude > PositionTolerance) || (lastQuat != null && Mathf.Pow(Mathf.DeltaAngle(currentQuat.eulerAngles.y, lastQuat.eulerAngles.y), 2) / 14400 > AngleTolerance))
            {
                arrow = Instantiate(arrowGO, arrowGO.transform.parent, false);
                arrow.SetActive(true); 
                arrow.transform.rotation = (currentQuat * Quaternion.Euler(90, 0, 0));
                arrow.transform.position = arrow.transform.TransformVector(new Vector3(0f, ArrowLength, 0f)) + currentPos;
                arrow.transform.localScale = new Vector3(ArrowLength, ArrowLength, ArrowLength);

                foreach (MeshRenderer mesh in arrow.GetComponentsInChildren<MeshRenderer>())
                {
                    mesh.material.color = Color;
                }

                lastPos = currentPos;
                lastQuat = currentQuat;
            }
        }
        
        lock (Arrows)
        {
            //base.Update() Odometry does not need it's transform handled
            while (Arrows.Count > Keep)
            {
                Destroy(Arrows.Dequeue());
            }

            if (oldArrowLength != ArrowLength)
            {
                foreach (GameObject ar in Arrows)
                {
                    ar.transform.localScale = new Vector3(ArrowLength, ArrowLength, ArrowLength);
                }
            }

            if (arrow == null)
                return;

            Arrows.Enqueue(arrow);
        }
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
            }
    }
}
