using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
#endif
public class TfTreeManager
{
    private static TfTreeManager instance;
    private static object instancelock = new object();

    public static TfTreeManager Instance
    {
        get
        {
            if (instance == null) lock (instancelock) if (instance == null) instance = new TfTreeManager();
            return instance;
        }
    }

    public delegate void TFTreeReady(TfVisualizer tfv);

    private Queue<TFTreeReady> beforeReady = new Queue<TFTreeReady>();

    private TfVisualizer tfTree;
    
    public void AddListener(TFTreeReady tftr)
    {
        lock(beforeReady)
        {
            if (tfTree != null)
                tftr(tfTree);
            else
            {
                beforeReady.Enqueue(tftr);
                Debug.LogWarning("THERE ARE NOW " + beforeReady.Count + " THINGS WAITING FOR THE TfVisualizer");
            }
        }
    }

    public void SetTFVisualizer(TfVisualizer tfv)
    {
        lock(beforeReady)
        {
            Debug.LogWarning("TfVisualizer is ready with " + beforeReady.Count + " things waiting for it!");
            tfTree = tfv;
            while (beforeReady.Count > 0)
                beforeReady.Dequeue()(tfTree);
        }
    }
}
