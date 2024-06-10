using System.Collections.Generic;
using UnityEngine;

class ArrowObjectPool : MonoBehaviour
{
    public static ArrowObjectPool current;

    [Tooltip("Assign the arrow prefab.")]
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
            Indicator arrow = Instantiate(pooledObject, transform, false);
            arrow.Activate(false);
            pooledObjects.Add(arrow);
        }
        return pooledObjects;
    }

    private void Awake()
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
        Indicator arrow = Instantiate(pooledObject, transform, false);
        arrow.Activate(false);
        PooledObjects.Add(arrow);
        return arrow;
    }

    /// <summary>
    /// De-active all the objects in the pool.
    /// </summary>
    public void DeactivateAllPooledObjects()
    {
        foreach (Indicator arrow in PooledObjects)
        {
            arrow.Activate(false);
        }
    }
}
