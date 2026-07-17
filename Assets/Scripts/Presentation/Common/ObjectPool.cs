using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeckoOut.Presentation.Common
{
    /// <summary>
    /// Minimal generic pool for scene objects. Get() revives an inactive
    /// instance or creates a new one; Release() hides and stores it.
    /// The single pool implementation of the project.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _inactiveInstances = new Stack<T>();

        public ObjectPool(T prefab, Transform parent)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            _prefab = prefab;
            _parent = parent;
        }

        public T Get()
        {
            T instance;

            if (_inactiveInstances.Count > 0)
            {
                instance = _inactiveInstances.Pop();
            }
            else
            {
                instance = UnityEngine.Object.Instantiate(_prefab, _parent);
            }

            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Release(T instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.gameObject.SetActive(false);
            _inactiveInstances.Push(instance);
        }
    }
}