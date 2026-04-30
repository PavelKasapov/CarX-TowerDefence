using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseTower<T> : MonoBehaviour where T : BaseProjectile
{
    [SerializeField] private ProjectileManager m_projectileManager;
    [SerializeField] private SphereCollider m_rangeCollider;
	[SerializeField] protected float m_shootInterval = 0.5f;
    [SerializeField] protected float m_range = 4f;
    [SerializeField] protected T m_projectilePrefab;
    [SerializeField] protected Transform m_shootPoint;

    protected Monster m_currentTarget;
    protected Transform m_transform;
    protected List<Monster> m_monstersInRange = new();
    protected bool isReloading;
    protected virtual bool CanShoot() => m_currentTarget != null;
    protected virtual void Awake()
    {
        m_transform = transform;
        m_rangeCollider.radius = m_range;
        StartCoroutine(ShootingRoutine());
    }
    private void OnTriggerEnter(Collider other)
    {
        var monster = other.GetComponent<Monster>();
        if (monster != null && !m_monstersInRange.Contains(monster))
        {
            m_monstersInRange.Add(monster);
            monster.m_OnDespawn += OnEnemyLost;
        }
        if (m_currentTarget == null)
            TargetChange();
    }
    private void OnTriggerExit(Collider other)
    {
        var monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            OnEnemyLost(monster);
        }
    }

    private void OnEnemyLost(Monster monster)
    {
        if (m_monstersInRange.Contains(monster))
        {
            m_monstersInRange.Remove(monster);
            monster.m_OnDespawn -= OnEnemyLost;
        }

        if (m_currentTarget == monster)
            TargetChange();
    }

    protected virtual void TargetChange()
    {
        if (m_monstersInRange.Count == 0)
        {
            m_currentTarget = null;
            return;
        }

        m_currentTarget = m_monstersInRange
            .Aggregate((closest, next) =>
            (m_transform.position - next.m_Transform.position).sqrMagnitude <
            (m_transform.position - closest.m_Transform.position).sqrMagnitude ? next : closest);
    }

    private IEnumerator ShootingRoutine()
    {
        while (true)
        {
            if (CanShoot())
            {
                isReloading = true;
                Shoot();
                yield return new WaitForSeconds(m_shootInterval);
                isReloading = false;
            }    
            else
            {
                yield return null;
            }
            
        }
    }

    protected virtual T Shoot()
    {
        T projectile = m_projectileManager.Get<T>();
        projectile.transform.position = m_shootPoint.position;
        projectile.transform.rotation = m_shootPoint.rotation;
        return projectile;
    }
}