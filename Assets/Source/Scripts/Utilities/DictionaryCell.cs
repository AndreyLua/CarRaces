using System;
using UnityEngine;

[Serializable]
public class DictionaryCell<TKey, TValue>
{
    [SerializeField] public TKey Key;
    [SerializeField] public TValue Value;

    public DictionaryCell(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}