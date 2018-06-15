using System;
using UnityEngine;
using Ros_CSharp;
using System.Reflection;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
#endif
public class RobotSubscriptionManager : ROSMonoBehavior
{
    public string Robot_Count = "";
    public string NameSpace_Prefix = "";
    public int First_Index = 1;
    [ShowOnly] public string Sample_Namespace;
    
    List<Component> parentScripts = new List<Component>();
    List<Component> childScripts = new List<Component>();

/*#if UNITY_EDITOR
    internal bool wrong_parent_warned = false;

    public void CheckHierarchy(bool needToCheck = true)
    {
        if (transform.parent == null || transform.parent.gameObject.GetComponent<TfVisualizer>() == null)
        {
            if (!wrong_parent_warned || !needToCheck)
            {
                EditorUtility.DisplayDialog("Invalid hierarchy", @"This script MUST be attached to a component that is a child of TFTree.
                    
Example:
TFTree
    TFFrame (required but unrelated)
    RobotTemplate (with this script added to it)
        LaserView
        OdometryView
        (
         AND each other thing to copy to 
         ALL of the robots related to this
         subcriptionmanager
        )", "I understand");
                wrong_parent_warned = true;
            }
        }
    }
#endif*/

    void Start()
    {
/*#if UNITY_EDITOR
        CheckHierarchy(false);
#endif*/
        rosmanager.StartROS(this, () =>
        {
            int numRobots;
            if (!int.TryParse(Robot_Count, out numRobots) && Robot_Count.Length != 0)
            {
                if (!Param.get(Robot_Count, ref numRobots))
                {
                    numRobots = 1;
                    EDB.WriteLine("Failed to treat NumberOfRobots: {0} as a rosparam name. Using 1 robot as the default", Robot_Count);
                }
            }

            TfTreeManager.Instance.AddListener(vis =>
            {
                GameObject parent = new GameObject();
                parent.name = "Templated Robots";
                parent.transform.parent = vis.transform.root;
                for (int num = First_Index; num < First_Index + numRobots; num++)
                {
                    GameObject go = new GameObject();
                    go.name = NameSpace_Prefix + num + " (" + GetType().Name + ")";
                    go.transform.parent = parent.transform;
                    foreach (Transform prefabTF in transform)
                    {
                        //Maybe put this outside to remove unnecessary loops
                        foreach (Component script in prefabTF.GetComponents(typeof(MonoBehaviour)))
                        {
                            if (!parentScripts.Contains(script))
                                parentScripts.Add(script);
                        }

                        GameObject prefab = Instantiate(prefabTF.gameObject, parent.transform, false);

                        //add all instantiated objects scripts to child scripts
                        foreach (Component script in prefab.GetComponents(typeof(MonoBehaviour)))
                        {
                            childScripts.Add(script);
                        }

                        prefab.transform.parent = go.transform;
                        prefab.name = prefabTF.name;
                        prefab.SendMessage("setNamespace", NameSpace_Prefix + num, SendMessageOptions.DontRequireReceiver);
                    }
                }

                //after instantiation disable old prefabs to prevent conflicts with default agents
                foreach (Transform prefabTf in transform)
                {
                    prefabTf.gameObject.SetActive(false);
                }
            });
        });
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