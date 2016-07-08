using UnityEngine;
using System.Collections;
using Ros_CSharp;
using System.Collections.Generic;
using System.Xml.Linq;
using System;

public class LoadMesh : ROSMonoBehavior {

    GameObject root = null;
    public string nameSpace;
    public string Package = "";
    public string RobotDescriptionParam = "";
    private string robotdescription;
    public XDocument RobotDescription { get; private set; }
    private Dictionary<string, joint> joints = new Dictionary<string, joint>();
    private Dictionary<string, link> links = new Dictionary<string, link>();

    private TfVisualizer tfviz
    {
        get
        {
            return transform.root.GetComponent<TfVisualizer>();
        }
    }


    //ros stuff
    internal NodeHandle nh;
    void Start () {
        rosmanager.StartROS(this, () => {
            nh = new NodeHandle();
        });
        //Object thing0 = Resources.Load(Package + "\\meshes\\" + MeshName);
        //GameObject thing = Instantiate(thing0) as GameObject;
        Load();
    }
	
	// Update is called once per frame
	void Update () {

        foreach (Transform tf in transform)
        {
            Transform tff;
            if (!tfviz.queryTransforms("/" + nameSpace + "/" + tf.name, out tff))
            {
                tff = transform;
            }

            tf.transform.position = tff.position;
            tf.transform.rotation = tff.rotation;
        }

    }

    //Written by Eric M.
    public bool Load()
    {
        if (Param.get(RobotDescriptionParam, ref robotdescription))
        {
            return Parse();
        }
        return false;
    }

    //Written by Eric M. (Listener in ROS.NET samples)
    //Modified by Dalton C.
    private bool Parse(IEnumerable<XElement> elements = null)
    {
        if (elements == null)
        {
            RobotDescription = XDocument.Parse(this.robotdescription);
            if (RobotDescription != null && RobotDescription.Root != null)
            {
                return Parse(RobotDescription.Elements());
            }
            return false;
        }
        elements = RobotDescription.Root.Elements();
       
        //grab joints and links
        foreach (XElement element in elements)
        {
            if (element.Name == "link")
                handleLink(element);

            if (element.Name == "gazebo")
            {
                XElement link = element.Element("link");
                if(link != null)
                    handleLink(link);
            }

            if(element.Name == "joint")
                 handleJoint(element);
            
        }
        /*
           due to how the code hase been changed its likely not necessary to maintaine joint and link obj's 
           It seems like maintaining a hierarchy is unecessary and just producing more work. This may be 
           brought back though if it is necessary for more complex urdfs. 
        */
        //handle hierarchy 
        /*
        foreach(KeyValuePair<string, link> curLink in links)
        {
            //curLink.Value.component.transform.parent = transform;
            
            if(!joints.ContainsKey(curLink.Key)) //if true, this is likely the root element
            {
                curLink.Value.component.transform.parent = transform;
                curLink.Value.component.transform.localPosition = new Vector3(0, 0, 0);
                curLink.Value.component.gameObject.SetActive(true);
                continue;
            }
            joint curJoint = joints[curLink.Key];
            link parentLink = links[curJoint.parent];
            Transform curTrans = curLink.Value.component.transform;
            //curTrans.parent = parentLink.component.transform;
            curTrans.parent = transform;
            float[] xyz;
            if ((xyz = curLink.Value.localPosToFloat()) == null)
            {
                if ((xyz = curJoint.localPosToFloat()) == null)
                {
                    xyz = new float[] { 0, 0, 0 };
                }
            }
            curTrans.localPosition = new Vector3(-xyz[0], xyz[2], xyz[1]);//+ curTrans.parent.transform.localPosition;
            curTrans.gameObject.SetActive(true);
            
    }
        */
        return true;
    }

    bool handleJoint(XElement joint)
    {
        XElement origin = joint.Element("origin");
        XElement parent = joint.Element("parent");
        XElement child = joint.Element("child");
        string strOrigin = origin == null ? null : origin.Attribute("xyz") == null ? null : origin.Attribute("xyz").Value;

        if(parent != null && child != null)
        {
            joints.Add(child.Attribute("link").Value, new joint(parent.Attribute("link").Value, child.Attribute("link").Value, strOrigin));
        }

        return true;
    }

    bool handleLink(XElement link)
    {
        XElement visual;
        //get pose outside of visual for gazebo
        XElement pose = link.Element("pose");
        if ((visual = link.Element("visual")) != null)
        {
            XElement origin = visual.Element("origin");
            XElement geometry = visual.Element("geometry");
            string xyz = origin == null ? pose == null ? null : pose.Value : origin.Attribute("xyz").Value;
            if ( geometry != null) 
            {
                //handle mesh
                XElement mesh;
                if ((mesh = geometry.Element("mesh")) != null)
                {
                    string path = mesh.Attribute("filename") == null ? mesh.Element("uri") == null ? null : mesh.Element("uri").Value : mesh.Attribute("filename").Value;

                    if (path != null)
                    {
                        if (path.StartsWith("package://"))
                            path = path.Remove(0, 10);
                        if (path.EndsWith(".dae"))
                            path = path.Remove(path.Length - 4, 4);
                        GameObject go = Instantiate(Resources.Load(path) as GameObject);
                        go.transform.parent = transform;
                        foreach(Transform tf in go.transform)
                        {
                            tf.rotation *= Quaternion.Euler(0, 0, 90);
                        }
                        go.name = link.Attribute("name").Value;
                        links.Add(go.name, new link(go, xyz));
                    }
                }

                //handle shapes (Cubes, Cylinders)
                XElement shape;
                if ((shape = geometry.Element("box")) != null)
                {
                    string dimensions = shape.Attribute("size").Value;
                    string[] components = dimensions.Split(' ');
                    float x, y, z;
                    if(float.TryParse(components[0], out x) && float.TryParse(components[1], out y) && float.TryParse(components[2], out z) )
                    {

                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.transform.parent = transform;
                        go.transform.localScale = new Vector3(y, z, x);
                        go.name = link.Attribute("name").Value;
                        links.Add(go.name, new link(go, xyz));

                    }

                }

                if ((shape = geometry.Element("cylinder")) != null)
                {
                    string length = shape.Attribute("length").Value;
                    string radius = shape.Attribute("radius").Value;
                    float fLength, fRadius;
                    if (float.TryParse(length, out fLength) && float.TryParse(radius, out fRadius))
                    {
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        go.transform.parent = transform;
                        go.transform.localScale = new Vector3(fRadius * 2, fLength, fRadius * 2);
                        go.name = link.Attribute("name").Value;
                        links.Add(go.name, new link(go, xyz));
                    }

                }

            }

        }else
        {
            GameObject go = new GameObject();
            go.transform.parent = transform;
            go.name = link.Attribute("name").Value;
            links.Add(go.name, new link(go, null));
        }
        return true;
    }
    
}
class link
{
    public GameObject component { set; get; }
    public string localPos { set; get; }
    public link(GameObject component, string localPos)
    {
        this.component = component;
        this.localPos = localPos;
    }
    public float[] localPosToFloat()
    {
        float[] xyz = new float[3];
        if (localPos != null)
        {
            string[] poses = localPos.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < poses.Length; ++index)
            {
                if (!float.TryParse(poses[index], out xyz[index]))
                {
                    xyz[index] = 0;
                }
            }
        }else
        {
            return null;
        }
        return xyz;

    }

}

class joint
{
    public string parent { set; get; }
    public string child  { set; get; }
    public string childLocalPos  { set; get; }
    public joint ( string parent, string child, string childLocalPos)
    {
        this.child = child;
        this.parent = parent;
        this.childLocalPos = childLocalPos;
    }
    public float[] localPosToFloat()
    {
        float[] xyz = new float[3];
        if (childLocalPos != null)
        {
            string[] poses = childLocalPos.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < poses.Length; ++index)
            {
                if (!float.TryParse(poses[index], out xyz[index]))
                {
                    xyz[index] = 0;
                }
            }
        }
        else
        {
            return null;
        }
        return xyz;

    }

}


