using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class Spawner : MonoBehaviour
{
	[SerializeField] private Monster m_monsterPrefab;
	[SerializeField] private Transform m_moveTarget;
    [SerializeField] private float m_interval = 3f;

	private ObjectPool<Monster> m_monstersPool;
	private Transform m_transform;

    private void Awake()
    {
		m_transform = transform;
		m_monstersPool = new(
			() => 
			{
				var newMonster = Instantiate(m_monsterPrefab, m_transform.position, Quaternion.LookRotation(m_moveTarget.position - m_transform.position));
				newMonster.m_OnDespawn += m_monstersPool.Release;
				return newMonster;
            },
			monster =>
			{
				monster.transform.position = m_transform.position;
				monster.gameObject.SetActive(true);
			},
			monster =>
			{
				monster.gameObject.SetActive(false);
			},
			defaultCapacity: 5, maxSize: 20);

    }

    private void Start()
    {
		StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
	{
		while (true) 
		{
            var newMonster = m_monstersPool.Get();
            newMonster.m_moveTarget = m_moveTarget;
			yield return new WaitForSeconds(m_interval);
        }
	}
}