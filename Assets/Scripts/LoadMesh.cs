using UnityEngine;
using System.Collections;
using Ros_CSharp;
using System.Collections.Generic;
using System.Xml.Linq;
using System;
using System.IO;
using Collada141;

public class LoadMesh : ROSMonoBehavior {

    public string RobotDescriptionParam = "";
    private string robotdescription;
    private Dictionary<string, Color?> materials = new Dictionary<string, Color?>();
    public XDocument RobotDescription { get; private set; }
    
    private TfVisualizer tfviz;

    //ros stuff
    public string NameSpace = "";
    public void setNamespace(string _NameSpace)
    {
        NameSpace = _NameSpace;
    }

    internal NodeHandle nh;
    void Start () {
        rosmanager.StartROS(this, () => {
            nh = new NodeHandle();
            Load();
        });
    }
	
    //Written by Eric M.
    //Modified by Dalton C.
    public bool Load()
    {
        if (!NameSpace.Equals(string.Empty))
        {
            if (!RobotDescriptionParam.StartsWith("/"))
                RobotDescriptionParam = "/" + RobotDescriptionParam;
        }else
        {
            if (RobotDescriptionParam.StartsWith("/"))
                RobotDescriptionParam.TrimStart('/');
        }


        if (Param.get(NameSpace + RobotDescriptionParam, ref robotdescription))
        {
            TfTreeManager.Instance.AddListener(vis => 
            {
                tfviz = vis;
                Parse();
            });
            return true;
        }
        return false;
    }

    private bool Parse(IEnumerable<XElement> elements = null)
    {

        //Written by Eric M. (Listener in ROS.NET samples)
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
       
        //written by Dalton C.
        //grab joints and links
        foreach (XElement element in elements)
        {
            if (element.Name == "material")
                handleMaterial(element); 

            if (element.Name == "link")
                handleLink(element);

            if (element.Name == "gazebo")
            {
                XElement link = element.Element("link");
                if (link != null)
                    handleLink(link);
            }
        }
        
        return true;
    }
    
    Color? handleMaterial(XElement material)
    {
        string colorName = material.Attribute("name").Value;
        Color? colorOut = null;

        if (materials.ContainsKey(colorName))
            return materials[colorName];

        XElement color = material.Element("color");
        if (color != null)
        {
            string colorVal = color.Attribute("rgba").Value;
            string[] colorValSplit = colorVal.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            float[] rbga = new float[colorValSplit.Length];
           

            for (int index = 0; index < colorValSplit.Length; ++index)
            {
                if (!float.TryParse(colorValSplit[index], out rbga[index]))
                {
                    rbga[index] = 0;
                }
            }
            if (rbga != null)
            {
                if (rbga.Length == 3)
                {
                    colorOut = new Color(rbga[0], rbga[1], rbga[2]);
                    materials.Add(colorName, colorOut);
                }

                if (rbga.Length == 4)
                {
                    colorOut = new Color(rbga[0], rbga[1], rbga[2], rbga[3]);
                    materials.Add(colorName, colorOut);
                }
            }
        }
        return colorOut;
    }
    
    //curently not using joints
    bool handleJoint(XElement joint)
    {
        XElement origin = joint.Element("origin");
        XElement parent = joint.Element("parent");
        XElement child = joint.Element("child");
        string strOrigin = origin == null ? null : origin.Attribute("xyz") == null ? null : origin.Attribute("xyz").Value;
        return true;
    }

    bool handleLink(XElement link)
    {
        if (link.Attribute("name").Value == "left_gripper")
            Debug.Log("thing");

        XElement visual;
        //get pose outside of visual for gazebo
        XElement pose = link.Element("pose");
        if ((visual = link.Element("visual")) != null)
        {
            XElement origin = visual.Element("origin");
            XElement geometry = visual.Element("geometry");
            XElement material = visual.Element("material");
            string xyz = origin == null ? pose == null ? null : pose.Value : origin.Attribute("xyz").Value;
            //string materialName = material == null ? null : material.Attribute("name") == null ? null : material.Attribute("name").Value;


            //make a function that can return an array of floats given an element value
            //hackey shit - gets relevant rpy rotation and xyz transform from the link
            float[] rpy_rot = null;
            string localRot = visual.Element("origin") == null ? null : visual.Element("origin").Attribute("rpy") == null ? null : visual.Element("origin").Attribute("rpy").Value;
            if (localRot != null)
            {
                string[] poses = localRot.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                rpy_rot = new float[poses.Length];
                for (int index = 0; index < poses.Length; ++index)
                {
                    if (!float.TryParse(poses[index], out rpy_rot[index]))
                    {
                        rpy_rot[index] = 0;
                    }
                }
            }//may be incorect shifting of rpy 
            Vector3 rpy_v = rpy_rot == null ? Vector3.zero : new Vector3(rpy_rot[0] * 57.3f, rpy_rot[2] * 57.3f, rpy_rot[1] * 57.3f);

            float[] xyz_pos = null;
            string localPos = visual.Element("origin") == null ? null : visual.Element("origin").Attribute("xyz") == null ? null : visual.Element("origin").Attribute("xyz").Value;
            if (localPos != null)
            {
                string[] poses = localPos.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                xyz_pos = new float[poses.Length];
                for (int index = 0; index < poses.Length; ++index)
                {
                    if (!float.TryParse(poses[index], out xyz_pos[index]))
                    {
                        xyz_pos[index] = 0;
                    }
                }
            }
            Vector3 xyz_v = xyz_pos == null ? Vector3.zero :  new Vector3(-xyz_pos[1], xyz_pos[2], xyz_pos[0]);
            //hackey shit


            Color ? color = null;
            if(material != null)
            {
                color = handleMaterial(material);
            }

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

                        COLLADA foundDae = null;
                        string dataPath = Application.dataPath;

                        if (path.EndsWith(".dae"))
                        {
                            foundDae = COLLADA.Load(dataPath + "/Resources/" + path);
                            path = path.Substring(0, path.LastIndexOf("."));
                        }

                        if (path.EndsWith(".DAE"))
                        {
                            foundDae = COLLADA.Load(dataPath + "/Resources/" + path);
                            path = path.Substring(0, path.LastIndexOf("."));
                        }

                        try {
                            UnityEngine.Object foundMesh = Resources.Load(path) as GameObject;

                            //handle rotations based on what axis is up for the mesh, this should fix most problems but 
                            //a better solution may need to be persued.  Potentially rewriting the meshes to be some specific orientation (probably Z)
                            //and reloading them.
                            if (foundDae != null)
                            {

                                switch (foundDae.asset.up_axis)
                                {
                                    case (UpAxisType.Z_UP):
                                        rpy_v += new Vector3(0f, 90f, 0f);
                                        break;
                                    case (UpAxisType.X_UP):
                                        //NA at the moment                               
                                        break;

                                    case (UpAxisType.Y_UP):
                                        rpy_v += new Vector3(-90f, 90f, 0f);
                                        break;

                                    default:
                                        //NA at the moment
                                        break;
                                }
                            }

                            if (foundMesh != null)
                            {
                                GameObject go = Instantiate(foundMesh as GameObject);
                                if (link.Attribute("name").Value == "pedestal")
                                    Debug.Log("thin");


                                //crunch this down into a simpler chunk of code to eliminate repetition 
                                if (go.transform.childCount == 0)
                                {

                                    go.transform.localPosition += xyz_v;
                                    go.transform.localRotation = Quaternion.Euler(rpy_v + go.transform.localEulerAngles);
                                    
                                    GameObject goParent = new GameObject();
                                    goParent.transform.parent = transform;
                                    goParent.name = link.Attribute("name").Value;
                                    go.transform.parent = goParent.transform;

                                    //this sucks, 
                                    // in some cases the urdf is declaring a mesh but not all the meshes that the dae needs
                                   // if (go.GetComponent<MeshRenderer>() != null && color != null)
                                    //    go.GetComponent<MeshRenderer>().material.color = color.Value;
                                }
                                else
                                {

                                    foreach (Transform tf in go.transform)
                                    {
                                        if (tf.name == "Lamp" || tf.name == "Camera")
                                        {
                                            Destroy(tf.gameObject);
                                            continue;
                                        }
                                        tf.transform.localPosition += xyz_v;
                                        tf.transform.localRotation = Quaternion.Euler(tf.transform.localEulerAngles + go.transform.localEulerAngles + rpy_v);

                                        //this sucks, 
                                        // in some cases the urdf is declaring a mesh but not all the meshes that the dae needs
                                       // if (tf.GetComponent<MeshRenderer>() != null && color != null)
                                         //   tf.GetComponent<MeshRenderer>().material.color = color.Value;
                                    }
                                    go.name = link.Attribute("name").Value;
                                    go.transform.parent = transform;

                                }
                            }
                        }
                        catch(Exception e)
                        {
                            Debug.LogWarning(e);
                        }
                       
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
                        GameObject parent = new GameObject();
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        parent.name = link.Attribute("name").Value;
                        parent.transform.parent = transform;
                        go.transform.parent = parent.transform;
                        go.transform.localScale = new Vector3(y, z, x);

                        if (xyz_pos != null)
                            go.transform.localPosition += new Vector3(-xyz_pos[1], xyz_pos[2], xyz_pos[0]);

                        if (rpy_rot != null)
                            go.transform.localRotation = Quaternion.Euler(new Vector3(rpy_rot[1] * 57.3f, rpy_rot[2] * 57.3f, -rpy_rot[0] * 57.3f));
                        
                        if (go.GetComponent<MeshRenderer>() != null && color != null)
                            go.GetComponent<MeshRenderer>().material.color = color.Value;
                        //links.Add(go.name, new link(go, xyz));

                    }

                }

                if ((shape = geometry.Element("cylinder")) != null)
                {
                    string length = shape.Attribute("length").Value;
                    string radius = shape.Attribute("radius").Value;
                    float fLength, fRadius;
                    if (float.TryParse(length, out fLength) && float.TryParse(radius, out fRadius))
                    {
                        GameObject parent = new GameObject();
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        parent.name = link.Attribute("name").Value;
                        parent.transform.parent = transform;
                        go.transform.parent = parent.transform;
                        go.transform.localScale = new Vector3(fRadius * 2, fLength/2, fRadius * 2);

                        if (xyz_pos != null)
                            go.transform.localPosition += new Vector3(-xyz_pos[1], xyz_pos[2], xyz_pos[0]);

                        if (rpy_rot != null)
                            go.transform.localRotation = Quaternion.Euler(new Vector3(rpy_rot[1] * 57.3f, rpy_rot[2] * 57.3f, -rpy_rot[0] * 57.3f));


                        if (go.GetComponent<MeshRenderer>() != null && color != null)
                            go.GetComponent<MeshRenderer>().material.color = color.Value;
                        //links.Add(go.name, new link(go, xyz));
                    }

                }

            }
        }
        else
        {
            GameObject go = new GameObject();
            go.transform.parent = transform;
            go.name = link.Attribute("name").Value;

        }
        return true;
    }


    //Position meshes appropriately in space 
    void Update()
    {

        foreach (Transform tf in transform)
        {
            Transform tff;

            if (tfviz != null && tfviz.queryTransforms(NameSpace + "/" + tf.name, out tff))
            {
                tf.transform.position = tff.position;
                tf.transform.rotation = tff.rotation;

            }

        }

    }


}



