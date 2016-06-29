using System;
using UnityEngine;
using Ros_CSharp;
using System.Reflection;
using System.Collections.Generic;


public class RobotSubscriptionManager : ROSMonoBehavior
{
    public string Robot_Count = "";
    public string NameSpace_Prefix = "";
    public int First_Index = 1;
    [ShowOnly] public string Sample_Namespace;
    
    List<Component> parentScripts = new List<Component>();
    List<Component> childScripts = new List<Component>();
    private NodeHandle nh = null;


    void Start()
    {
        rosmanager.StartROS(this, () => {
            nh = new NodeHandle();
        });


        int numRobots;
        if (!int.TryParse(Robot_Count, out numRobots) && Robot_Count.Length != 0)
        {
            if (!Param.get(Robot_Count, ref numRobots))
            {
                numRobots = 1;
                EDB.WriteLine("Failed to treat NumberOfRobots: {0} as a rosparam name. Using 1 robot as the default", Robot_Count);
            }
        }

        for (int num = First_Index; num < First_Index + numRobots; num++)
        {
            GameObject go = new GameObject();
            go.name = NameSpace_Prefix + num;
            go.transform.parent = transform.root;
            foreach (Transform prefabTF in transform)
            {
                prefabTF.gameObject.hideFlags |= HideFlags.HideInHierarchy;

                //Maybe put this outside to remove unnecessary loops
                foreach (Component script in prefabTF.GetComponents(typeof(MonoBehaviour)))
                {
                    if (!parentScripts.Contains(script))
                        parentScripts.Add(script);
                }

                GameObject prefab = Instantiate(prefabTF.gameObject);

                //add all instantiated objects scripts to child scripts
                foreach (Component script in prefab.GetComponents(typeof(MonoBehaviour)))
                {
                    childScripts.Add(script);
                }

                prefab.transform.parent = go.transform;
                prefab.SendMessage("setNamespace", NameSpace_Prefix + num);
            }
        }

        //after instantiation disable old prefabs to prevent conflicts with default agents
        foreach (Transform prefabTf in transform)
        {
            prefabTf.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update() { }

    //getters for the UI to get and update settings on the fly
    public List<Component> getParentScripts()
    {
        return parentScripts;
    }
    public List<Component> getChildScripts()
    {
        return childScripts;
    }

}