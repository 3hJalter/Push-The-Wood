using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public abstract bool IsDead
    {
        get; set;
    }
}
