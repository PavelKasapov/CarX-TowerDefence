using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileManager : MonoBehaviour
{
    [SerializeField] private BaseProjectile[] m_prefabs;

    private Dictionary<Type, ObjectPool<BaseProjectile>> m_pools;

    private void Awake()
    {
        m_pools = new Dictionary<Type, ObjectPool<BaseProjectile>>();
        foreach (var prefab in m_prefabs)
        {
            var type = prefab.GetType();
            m_pools[type] = new ObjectPool<BaseProjectile>(
                createFunc: () => {
                    var newProjectile = Instantiate(prefab, transform);
                    newProjectile.m_OnDespawn += () => Release(newProjectile);
                    return newProjectile;
                },
                projectile => projectile.gameObject.SetActive(true),
                projectile => projectile.gameObject.SetActive(false),
                collectionCheck: false,
                defaultCapacity: 3,
                maxSize: 50
            );
        }
    }

    public T Get<T>() where T : BaseProjectile
    {
        var type = typeof(T);
        if (m_pools.TryGetValue(type, out var pool))
            return (T)pool.Get();
        throw new ArgumentException($"No pool for type {type}");
    }

    public void Release(BaseProjectile projectile)
    {
        var type = projectile.GetType();
        if (m_pools.TryGetValue(type, out var pool))
            pool.Release(projectile);
        else
            Destroy(projectile.gameObject);
    }
}