using System.Collections.Generic;
using UnityEngine;

public class BoxObjectPool : MonoBehaviour
{
    public static BoxObjectPool current;

    [Tooltip("Assign the box prefab.")]
    public Indicator pooledObject;
    [Tooltip("Initial pooled amount.")]
    public int pooledAmount = 1;
    [Tooltip("Should the pooled amount increase.")]
    public bool willGrow = true;

    private List<Indicator> pooledObjects;

    private List<Indicator> PooledObjects => pooledObjects ??= GeneratePooledObjects();
    
    private List<Indicator> GeneratePooledObjects()
    {
        pooledObjects = new List<Indicator>();

        for (int i = 0; i < pooledAmount; i++)
        {
            Indicator box = Instantiate(pooledObject, transform, false);
            box.Activate(false);
            pooledObjects.Add(box);
        }
        return pooledObjects;
    }

    void Awake()
    {
        current = this;
    }

    /// <summary>
    /// Gets pooled objects from the pool.
    /// </summary>
    /// <returns></returns>
    public Indicator GetPooledObject()
    {
        for (int i = 0; i < PooledObjects.Count; i++)
        {
            if (!PooledObjects[i].Active)
            {
                return PooledObjects[i];
            }
        }

        if (!willGrow) return null;
        Indicator box = Instantiate(pooledObject, transform, false);
        box.Activate(false);
        PooledObjects.Add(box);
        return box;
    }

    /// <summary>
    /// De-active all the objects in the pool.
    /// </summary>
    public void DeactivateAllPooledObjects()
    {
        foreach (Indicator box in PooledObjects)
        {
            box.Activate(false);
        }
    }
}
