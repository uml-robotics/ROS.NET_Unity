using System;
using UnityEngine;
using Ros_CSharp;


public class RobotSubscriptionManager : ROSMonoBehavior {

    public string NumberOfRobots;
    public string NameSpace = "/agent";
    public string CountParamter = "/mrm/robots_count";

    private NodeHandle nh = null;
    ParamIntDelegate callback;
    
    void callBack(string key, int value)
    {
        return;
    }

    void Start () {
       
        rosmanager.StartROS(this, () => {
            nh = new NodeHandle();
        });
        gameObject.hideFlags |= HideFlags.HideInHierarchy;

        int numRobs;
        if (!Int32.TryParse(NumberOfRobots, out numRobs) || NumberOfRobots.Equals(string.Empty))
        {
            if (!Param.get(CountParamter, ref numRobs))
            {
                numRobs = 1;
            }
        }
        for (int num = 1; num <= numRobs; ++num)
        {
            GameObject go = new GameObject();
            go.name = NameSpace + num;
            go.transform.parent = transform.root;

            foreach (Transform prefabTF in transform)
            {
                GameObject prefab = Instantiate(prefabTF.gameObject);
                prefab.transform.parent = go.transform;
                prefab.SendMessage("setNamespace", NameSpace + num);
            }
        }
    }
	
	// Update is called once per frame
	void Update () { }
	
}
