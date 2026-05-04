using System;
using System.Collections;
using UnityEngine;

public abstract class BaseProjectile : MonoBehaviour
{
    [SerializeField] protected float m_speed = 12f;
    [SerializeField] private int m_damage = 10;
    private Coroutine m_moveRoutine;
    private Coroutine m_lifeTimerRoutine;
    protected Transform m_transform;
    public Action m_OnDespawn { get; set; }
    public float m_Speed => m_speed;

    private void Awake()
    {
        m_transform = transform;
        m_OnDespawn += () =>
        {
            gameObject.SetActive(false);
        };
    }

    protected virtual void OnEnable()
    {
        m_moveRoutine = StartCoroutine(MovingRoutine());
        m_lifeTimerRoutine = StartCoroutine(LifeTimer());
    }

    private void OnDisable()
    {
        if (m_moveRoutine != null)
            StopCoroutine(m_moveRoutine);

        if (m_lifeTimerRoutine != null)
            StopCoroutine(m_lifeTimerRoutine);

        m_moveRoutine = null;
    }

    protected virtual IEnumerator MovingRoutine()
    {
        yield return null;
        while (true)
        {
            Move();
            yield return null;
        }
    }

    protected abstract void Move();
  
    void OnTriggerEnter(Collider other)
    {
        var monster = other.gameObject.GetComponent<Monster>();
        if (monster == null)
            return;

        monster.TakeDamage(m_damage);

        m_OnDespawn?.Invoke();
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(5);
        m_OnDespawn?.Invoke();
    }
}