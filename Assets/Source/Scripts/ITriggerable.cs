using System;
using UnityEngine;

public interface ITriggerable
{
    public Transform Transform { get; }
    public event Action<ITriggerable> OnTriggerRemoveForced;
}
