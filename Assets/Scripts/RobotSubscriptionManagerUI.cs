#if UNITY_EDITOR
using UnityEngine;
using System.Reflection;
using UnityEditor;
using System;

[CustomEditor(typeof(RobotSubscriptionManager))]
public class RobotSubscriptionManagerUI : Editor
{
    public bool wrong_parent_warned = false;
    public override void OnInspectorGUI()
    {
        RobotSubscriptionManager rsmTarget = (RobotSubscriptionManager)target;

        //make RobotSubscriptionManager.cs inspector only visble when not playing
        if (!Application.isPlaying)
        {
            if (rsmTarget.getParentScripts().Count == 0)
                rsmTarget.CheckHierarchy();

            EditorGUILayout.HelpBox("\"Robot_Count\" can be EITHER an integer OR a parameter name", MessageType.Info);
            
            rsmTarget.Sample_Namespace = rsmTarget.NameSpace_Prefix + rsmTarget.First_Index;
            base.DrawDefaultInspector();
        }
        else
        {
            rsmTarget.wrong_parent_warned = false;
            //update UI and Masters
            EditorGUILayout.Separator();
            foreach (Component script in rsmTarget.getParentScripts())
            {
                EditorGUILayout.LabelField(script.name);
                foreach (FieldInfo fi in script.GetType().GetFields())
                {
                    if (fi.FieldType.Equals(typeof(string)))
                    {
                        if (!(fi.Name.Equals("Topic", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            string temp = EditorGUILayout.TextField(fi.Name, (string)fi.GetValue(script));
                            if (GUI.changed)
                            {
                                fi.SetValue(script, temp);
                            }
                        }
                        continue;
                    }

                    if (fi.FieldType.Equals(typeof(int)))
                    {
                        int temp = EditorGUILayout.IntField(fi.Name, (int)fi.GetValue(script));
                        if (GUI.changed)
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
            if (GUI.changed)
            {
                foreach (Component script in rsmTarget.getChildScripts())
                {
                    Component parentScript = rsmTarget.getParentScripts().Find((ps) => { return ps.GetType().Equals(script.GetType()); });
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

#region Editor

#endregion

#endif