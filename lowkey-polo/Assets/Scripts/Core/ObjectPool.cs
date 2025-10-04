using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> _objects = new Queue<T>();
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly int _maxSize;
    
    public ObjectPool(T prefab, Transform parent, int maxSize = 50)
    {
        _prefab = prefab;
        _parent = parent;
        _maxSize = maxSize;
    }
    
    public T Get()
    {
        if (_objects.Count > 0)
        {
            var obj = _objects.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        
        return Object.Instantiate(_prefab, _parent);
    }
    
    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        
        if (_objects.Count < _maxSize)
        {
            _objects.Enqueue(obj);
        }
        else
        {
            Object.Destroy(obj.gameObject);
        }
    }
    
    public void Clear()
    {
        while (_objects.Count > 0)
        {
            Object.Destroy(_objects.Dequeue().gameObject);
        }
    }
}

public interface IPoolable
{
    void OnPoolGet();
    void OnPoolReturn();
}
