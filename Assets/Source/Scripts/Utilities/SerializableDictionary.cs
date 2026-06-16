using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue>, ISerializationCallbackReceiver, IEnumerable
{
    [SerializeField] private List<DictionaryCell<TKey, TValue>> _cells;
    private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

    public SerializableDictionary(int capacity = 0)
    {
        _cells = new List<DictionaryCell<TKey, TValue>>(capacity);
        _dictionary = new Dictionary<TKey, TValue>(capacity);
    }

    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    public IEnumerable<TKey> Keys => _dictionary.Keys;
    public IEnumerable<TValue> Values => _dictionary.Values;
    public int Count => _dictionary.Count;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => _dictionary.Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => _dictionary.Values;

    public bool IsReadOnly => false;

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
    public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);
    IEnumerator IEnumerable.GetEnumerator() => _cells.GetEnumerator();

    public void Add(TKey key, TValue value)
    {
        _cells.Add(new DictionaryCell<TKey, TValue>(key, value));
        _dictionary.Add(key, value);
    }

    public bool Remove(TKey key)
    {
        _cells.RemoveAt(_cells.FindIndex(cell => cell.Key.Equals(key)));
        return _dictionary.Remove(key);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        _cells.Add(new DictionaryCell<TKey, TValue>(item.Key, item.Value));
        _dictionary.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _cells.Clear();
        _dictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        _cells.RemoveAt(_cells.FindIndex(cell => EqualityComparer<TKey>.Default.Equals(cell.Key, item.Key)));
        return _dictionary.Remove(item.Key);
    }

    private void ConvertDictionary()
    {
        _dictionary = _cells.ToDictionary(cell => cell.Key, cell => cell.Value);
    }

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize() => ConvertDictionary();
}
