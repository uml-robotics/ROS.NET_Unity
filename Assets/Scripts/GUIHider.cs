using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class GUIHider : MonoBehaviour
{
    private static List<GUIHider> _instances = new List<GUIHider>();
    static GUIHider()
    {
#if UNITY_EDITOR
        EditorApplication.update += EditorUpdate;
#endif
    }

    public GUIHider()
    {
        lock (_instances)
            _instances.Add(this);
    }

    public bool HideInEditor;
    public bool HideAlways;
    private GameObject parent = null;

	static void EditorUpdate () {
	    lock (_instances)
	    {
	        Queue<GUIHider> deadmeat = new Queue<GUIHider>();
	        foreach (GUIHider hider in _instances)
	        {
	            if (hider.parent == null)
	                try
	                {
	                    hider.parent = hider.gameObject;
	                }
	                catch (Exception)
	                {
	                    deadmeat.Enqueue(hider);
	                }
	            if (hider.parent != null)
	            {
	                bool desired = 
#if UNITY_EDITOR
	                    !hider.HideAlways && (EditorApplication.isPlaying || !hider.HideInEditor);
#else
                        !hider.HideAlways;
#endif
	                bool current = hider.parent.activeSelf;
	                if (current != desired)
	                {
	                    hider.parent.SetActive(desired);
	                }
	            }
	        }
	        while (deadmeat.Count > 0)
	        {
	            _instances.Remove(deadmeat.Dequeue());
	        }
	    }
	}
}
