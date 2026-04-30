using System.Collections;
using UnityEngine;

public class CannonTower : BaseTower<CannonProjectile>
{
    [SerializeField] private float m_rotationSpeed = 15f;
    private Quaternion m_aimRotation;
    private Quaternion m_baseRotation;
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
        if (m_currentTarget == null)
        {
            m_aimRotation = m_baseRotation;
            return;
        } 

        float projectileSpeed = m_projectilePrefab.m_Speed;
        Vector3 towerPos = m_shootPoint.position;
        Vector3 targetPos = m_currentTarget.m_Transform.position;
        Vector3 targetVel = m_currentTarget.m_Transform.forward * m_currentTarget.m_Speed;
        Debug.Log($"{m_currentTarget.m_Transform.position} {m_currentTarget.m_Transform.position + m_currentTarget.m_Transform.forward * m_currentTarget.m_Speed}");

        float totalTime = Vector3.Distance(towerPos, targetPos) / projectileSpeed;
        Vector3 predictedPos = targetPos + targetVel * totalTime;
        float epsilon = 0.1f;

        for (int i = 0; i < 30; i++)
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
            float newTotalTime = flightTime + Mathf.Max(rotationTime, isReloading ? m_shootInterval : 0);
            Vector3 newPredictedPos = targetPos + targetVel * newTotalTime;

            if (Vector3.Distance(newPredictedPos, predictedPos) < epsilon && Mathf.Abs(newTotalTime - totalTime) < epsilon)
            {
                Debug.Log($"Calculation done on {i} inetation {Vector3.Distance(newPredictedPos, predictedPos) < epsilon} {Mathf.Abs(newTotalTime - totalTime) < epsilon}");
                break;
            }

            predictedPos = newPredictedPos;
            totalTime = newTotalTime;

#if UNITY_EDITOR
            debug_aimpoint = predictedPos;
            Debug.Log(predictedPos);
#endif
        }

        Vector3 aimPoint = predictedPos;
        Vector3 direction = aimPoint - towerPos;
        m_aimRotation = Quaternion.LookRotation(direction);

        if (m_baseRotation.Equals(default))
            m_baseRotation = m_aimRotation;
    }

    protected override CannonProjectile Shoot()
    {
        var projectile = base.Shoot();
        CalculateRotation();
        return projectile;
    }

    protected override bool CanShoot()
    {
        
        return base.CanShoot() && Quaternion.Angle(m_transform.rotation, m_aimRotation) < 0.1f;
    }
    IEnumerator RotationRoutine()
    {
        while (true)
        {
            /*CalculateRotation();*/
            
            m_transform.rotation = Quaternion.RotateTowards(
                m_transform.rotation,
                m_aimRotation,
                m_rotationSpeed * Time.deltaTime
                );

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
            if (m_currentTarget != null)
                Gizmos.DrawWireSphere(m_currentTarget.m_Transform.position, 1f);
        }
    }
#endif
}
