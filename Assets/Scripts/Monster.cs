using System;
using System.Collections;
using UnityEngine;

public class Monster : MonoBehaviour
{
    const float m_reachDistance = 0.3f;
    
    [SerializeField] private float m_speed = 6f;
    [SerializeField] private int m_maxHP = 30;
    
    private int m_hp;
    private Transform m_transform;
    private Coroutine m_moveRoutine;

    public Transform m_moveTarget;
	public Action<Monster> m_OnDespawn {  get; set; }
    public float m_Speed => m_speed;
    public Transform m_Transform => m_transform;

    private void Awake()
    {
        m_transform = transform;
    }

    public void TakeDamage(int damage)
	{
        m_hp -= damage;
		if (m_hp <= 0)
		{
			m_OnDespawn?.Invoke(this);
        }
    }

    private void OnEnable()
    {
        m_hp = m_maxHP;
        m_moveRoutine = StartCoroutine(MovementRoutine());
        
    }

    private void OnDisable()
    {
        if (m_moveRoutine != null)
            StopCoroutine(m_moveRoutine);

        m_moveRoutine = null;
    }

    private IEnumerator MovementRoutine()
    {
        yield return null;

        m_transform.LookAt(m_moveTarget);
        while (m_moveTarget != null && Vector3.Distance(m_transform.position, m_moveTarget.position) > m_reachDistance)
		{
            Vector3 newPosition = Vector3.MoveTowards(m_transform.position, m_moveTarget.position, m_speed * Time.deltaTime);
            m_transform.position = newPosition;
            yield return null;
        }		
        m_OnDespawn?.Invoke(this);
    }
}