using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Dictionary<Type, IPlayerComponent> _components;
    [SerializeField] private InputReaderSO _inputReader;

    private void Awake()
    {
        _components = new();
        
        _inputReader.Initialize(this);
        _components.Add(_inputReader.GetType(), _inputReader);
        
        foreach (var compo in GetComponentsInChildren<IPlayerComponent>(true).ToList())
        {
            compo.Initialize(this);
            _components.Add(compo.GetType(), compo);
        }
    }

    public T GetCompo<T>(bool isDerived = false) where T : IPlayerComponent
    {
        if (isDerived == false)
            return (T)_components.GetValueOrDefault(typeof(T));

        Type findType = _components.Keys.FirstOrDefault(type => type.IsSubclassOf(typeof(T)));
        if (findType != null)
            return (T)_components[findType];

        return default;
    }
}