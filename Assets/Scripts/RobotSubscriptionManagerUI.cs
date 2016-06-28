#if UNITY_EDITOR
using UnityEngine;
using System.Reflection;
using UnityEditor;


[CustomEditor(typeof(RobotSubscriptionManager))]
public class RobotSubscriptionManagerUI : Editor {

    public override void OnInspectorGUI()
    {
        RobotSubscriptionManager rsmTarget = (RobotSubscriptionManager)target;

        if (!Application.isPlaying)
            base.DrawDefaultInspector();
        //update UI and Masters
        EditorGUILayout.Separator();
        foreach (Component script in rsmTarget.getMasterScripts())
        {
            EditorGUILayout.LabelField(script.name);
            foreach (FieldInfo fi in script.GetType().GetFields())
            {
                if(fi.FieldType.Equals(typeof(string)))
                {
                    if (!(fi.Name.Equals("topic") || fi.Name.Equals("NameSpace")))
                    {
                        string temp = EditorGUILayout.TextField(fi.Name, (string)fi.GetValue(script));
                        if(GUI.changed) 
                        fi.SetValue(script, temp);
                    }
                    continue;
                }

                if (fi.FieldType.Equals(typeof(int)))
                {
                    int temp = EditorGUILayout.IntField(fi.Name, (int)fi.GetValue(script));
                    if(GUI.changed)
                        fi.SetValue(script, temp);

                    continue;
                }

                if (fi.FieldType.Equals(typeof(float)))
                {
                    float temp = EditorGUILayout.FloatField(fi.Name, (float)fi.GetValue(script));
                    if (GUI.changed)
                        fi.SetValue(script, temp);

                    continue;
                }

                if (fi.FieldType.Equals(typeof(double)))
                {
                    double temp = EditorGUILayout.DoubleField(fi.Name, (double)fi.GetValue(script));
                    if (GUI.changed)
                        fi.SetValue(script, temp);
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        //update agents
        if(GUI.changed)
        foreach (GameObject agent in rsmTarget.getAgents())
        {
            foreach (Transform child in agent.transform)
            {
                foreach (Component script in child.GetComponents(typeof(MonoBehaviour)))
                {
                    Component parentScript = rsmTarget.getMasterScripts().Find((ps) => { return ps.GetType().Equals(script.GetType()); });
                    foreach (FieldInfo fi in script.GetType().GetFields())
                    {
                        FieldInfo parentFI = parentScript.GetType().GetField(fi.Name);
                        if (parentFI != null)
                            fi.SetValue(script, parentFI.GetValue(parentScript));
                    }
                }
            }
        }            
    }
}
#endif
