using System.Collections;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(TargetTracker))]
public abstract class BaseTower<T> : MonoBehaviour where T : BaseProjectile
{
    [Inject] private ProjectileManager m_projectileManager;
    [SerializeField] protected TargetTracker m_targetTracker;

	[SerializeField] protected float m_shootInterval = 2f;
    [SerializeField] protected T m_projectilePrefab;
    [SerializeField] protected Transform m_shootPoint;
    [SerializeField] protected float m_range = 10f;

    protected Transform m_transform;
    protected bool m_isReloading;
    protected virtual bool CanShoot() => m_targetTracker.CurrentTarget != null;
    protected virtual void Awake()
    {
        m_transform = transform;
        m_targetTracker ??= GetComponent<TargetTracker>();
        m_targetTracker.Init(m_transform, m_range);
    }

    protected virtual void Start()
    {
        StartCoroutine(ShootingRoutine());
    }

    private IEnumerator ShootingRoutine()
    {
        while (true)
        {
            if (CanShoot())
            {
                m_isReloading = true;
                Shoot();
                yield return new WaitForSeconds(m_shootInterval);
                m_isReloading = false;
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
