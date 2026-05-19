using System;
using System.Collections.Generic;
using UnityEngine;

public class TargetTracker : MonoBehaviour
{
    [SerializeField] private SphereCollider m_rangeCollider;
    [SerializeField] private float m_range = 4f;

    private Transform m_transform;
    private List<Monster> m_monstersInRange = new();
    public Monster CurrentTarget { get; private set; }
    public float m_Range => m_range;
    public event Action OnTargetChange;
    private void Awake()
    {
        m_transform = transform;
        m_rangeCollider.radius = m_range;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var monster = other.GetComponent<Monster>();
        if (monster != null && !m_monstersInRange.Contains(monster))
        {
            m_monstersInRange.Add(monster);
            monster.m_OnDespawn += InvalidateTarget;
        }
        if (CurrentTarget == null)
            TargetChange();
    }
    private void OnTriggerExit(Collider other)
    {
        var monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            InvalidateTarget(monster);
        }
    }

    public void InvalidateTarget(Monster monster)
    {
        if (m_monstersInRange.Contains(monster))
        {
            m_monstersInRange.Remove(monster);
            monster.m_OnDespawn -= InvalidateTarget;
        }

        if (CurrentTarget == monster)
            TargetChange();
    }

    private void TargetChange()
    {
        Monster closestMonster = null;
        float minDistance = float.MaxValue;
        foreach (var monster in m_monstersInRange)
        {
            float distance = (monster.m_Transform.position - m_transform.position).sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                closestMonster = monster;
            }
        }
        CurrentTarget = closestMonster;
        OnTargetChange?.Invoke();
    }
}