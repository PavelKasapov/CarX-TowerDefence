using System.Collections;
using UnityEngine;

public class CannonTower : BaseTower<CannonProjectile>
{
    [SerializeField] private float m_rotationSpeed = 15f;
    private Quaternion m_aimRotation;
#if UNITY_EDITOR
    private Vector3 debug_aimpoint;
#endif
    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(RotationRoutine());
    }

    protected override void TargetChange()
    {
        base.TargetChange();
        CalculateRotation();
    }

    private void CalculateRotation()
    {
        Debug.Log(m_currentTarget);
        if (m_currentTarget == null)
        { 
            m_aimRotation = m_transform.rotation;
            return;
        } 

        float projectileSpeed = m_projectilePrefab.m_Speed;
        Vector3 towerPos = m_shootPoint.position;
        Vector3 targetPos = m_currentTarget.m_Transform.position;
        Vector3 targetVel = m_currentTarget.m_Transform.forward * m_currentTarget.m_Speed;
        Debug.Log($"{m_currentTarget.m_Transform.position} {m_currentTarget.m_Transform.position + m_currentTarget.m_Transform.forward * m_currentTarget.m_Speed}");

        float t = Vector3.Distance(towerPos, targetPos) / projectileSpeed;
        Vector3 predictedPos = targetPos + targetVel * t;

        for (int i = 0; i < 10; i++)
        {
            float distance = Vector3.Distance(towerPos, predictedPos);
            if (distance > m_range)
            {
                m_monstersInRange.Remove(m_currentTarget);
                TargetChange();
                return;
            }

            float flightTime = distance / projectileSpeed;

            Vector3 toPredicted = predictedPos - towerPos;
            float angle = Vector3.Angle(m_transform.forward, toPredicted);
            float rotationTime = angle / m_rotationSpeed;

            t = flightTime + Mathf.Max(rotationTime, m_shootInterval);

            predictedPos = targetPos + targetVel * t;
#if UNITY_EDITOR
            debug_aimpoint = predictedPos;
            Debug.Log(predictedPos);
#endif
        }

        Vector3 aimPoint = predictedPos;
        Vector3 direction = aimPoint - towerPos;
        m_aimRotation = Quaternion.LookRotation(direction);
    }

    protected override CannonProjectile Shoot()
    {
        var projectile = base.Shoot();
        CalculateRotation();
        return projectile;
    }

    protected override bool CanShoot()
    {
        Debug.Log(Quaternion.Angle(m_transform.rotation, m_aimRotation) < 0.1f);
        return Quaternion.Angle(m_transform.rotation, m_aimRotation) < 0.1f;
    }
    IEnumerator RotationRoutine()
    {
        while (true)
        {
            /*CalculateRotation();*/
            if (m_currentTarget != null)
            {
                m_transform.rotation = Quaternion.RotateTowards(
                    m_transform.rotation,
                    m_aimRotation,
                    m_rotationSpeed * Time.deltaTime
                    );
            }

            yield return null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (debug_aimpoint != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(debug_aimpoint, 0.4f);
            Gizmos.DrawWireSphere(m_currentTarget.m_Transform.position, 1f);
        }
    }
#endif
}
