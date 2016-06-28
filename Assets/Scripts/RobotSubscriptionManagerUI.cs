#if UNITY_EDITOR
using UnityEngine;
using System.Reflection;
using UnityEditor;


[CustomEditor(typeof(RobotSubscriptionManager))]
public class RobotSubscriptionManagerUI : Editor {

    public override void OnInspectorGUI()
    {
        RobotSubscriptionManager rsmTarget = (RobotSubscriptionManager)target;

        //make RobotSubscriptionManager.cs inspector only visble when not playing
        if (!Application.isPlaying)
        {
            base.DrawDefaultInspector();
        }
        else
        {
            //update UI and Masters
            EditorGUILayout.Separator();
            foreach (Component script in rsmTarget.getParentScripts())
            {
                EditorGUILayout.LabelField(script.name);
                foreach (FieldInfo fi in script.GetType().GetFields())
                {
                    if (fi.FieldType.Equals(typeof(string)))
                    {
                        if (!(fi.Name.Equals("Topic") || fi.Name.Equals("NameSpace")))
                        {
                            string temp = EditorGUILayout.TextField(fi.Name, (string)fi.GetValue(script));
                            if (GUI.changed)
                                fi.SetValue(script, temp);
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

            //update child scripts (agents)
            if (GUI.changed)
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
#endif
