using UnityEngine;
using System.Collections;
using Ros_CSharp;
using System.Collections.Generic;
using System.Xml.Linq;

public class LoadMesh : ROSMonoBehavior {

    public string Package = "";
    public string RobotDescriptionParam = "";
    private string robotdescription;
    public XDocument RobotDescription { get; private set; }


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
        bool success = true;
        elements = RobotDescription.Root.Elements();
        foreach (XElement element in elements)
        {
            if (element.Name == "link")
            {
                handleLink(element);
            }

            if (element.Name == "gazebo")
            {
                XElement link = element.Element("link");
                if(link != null)
                handleLink(link);
            }

        }
        return success;
    }

    bool handleLink(XElement link)
    {
        XElement visual;
        if ((visual = link.Element("visual")) != null)
        {
            XElement origin = visual.Element("origin");
            XElement geometry = visual.Element("geometry");
            if ( geometry != null) //origin != null && //soon...
            {
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

                        Instantiate(Resources.Load(path) as GameObject);
                    }
                }
                XElement shape;
                if ((shape = geometry.Element("box")) != null)
                {
                    string dimensions = shape.Attribute("size").Value;
                    string[] components = dimensions.Split(' ');
                    float x, y, z;
                    if(float.TryParse(components[0], out x) && float.TryParse(components[1], out y) && float.TryParse(components[2], out z) )
                    {

                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.transform.localScale = new Vector3(x, z, y);
                        go.name = link.Attribute("name").Value;
                    }
                    //probably unecessary
                    /* 
                    List<float> fComponents = new List<float>();
                    foreach(string component in components)
                    {
                        float temp;
                        if(float.TryParse(component, out temp))
                        {
                            fComponents.Add(temp);
                        }else
                        {
                            fComponents.Add(0);
                        }
                        
                    }*/
                    //Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube).transform.localScale = new Vector3(fComponents.g))

                }

                if ((shape = geometry.Element("cylinder")) != null)
                {
                    string length = shape.Attribute("length").Value;
                    string radius = shape.Attribute("radius").Value;
                    float fLength, fRadius;
                    if (float.TryParse(length, out fLength) && float.TryParse(radius, out fRadius))
                    {

                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        go.transform.localScale = new Vector3(fRadius, fLength, fRadius);
                        go.name = link.Attribute("name").Value;
                    }
                    //probably unecessary
                    /* 
                    List<float> fComponents = new List<float>();
                    foreach(string component in components)
                    {
                        float temp;
                        if(float.TryParse(component, out temp))
                        {
                            fComponents.Add(temp);
                        }else
                        {
                            fComponents.Add(0);
                        }
                        
                    }*/
                    //Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube).transform.localScale = new Vector3(fComponents.g))

                }

            }

        }
        return true;
    }
}


