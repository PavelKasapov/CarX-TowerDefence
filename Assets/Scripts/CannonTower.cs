using System.Collections;
using UnityEngine;

public class CannonTower : BaseTower<CannonProjectile>
{
    [SerializeField] private Transform m_gunTransform;
    [SerializeField] private float m_yawSpeed = 30f;
    [SerializeField] private float m_pitchSpeed = 30f;

    private Vector3 m_baseAimPosition;
    private Vector3 m_predictedPosition;
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
            m_predictedPosition = m_baseAimPosition;
            return;
        }

        float projectileSpeed = m_projectilePrefab.m_Speed;
        Vector3 towerPos = m_shootPoint.position;
        Vector3 targetPos = m_currentTarget.m_Transform.position;
        Vector3 targetVel = m_currentTarget.m_Transform.forward * m_currentTarget.m_Speed;

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
            float rotationTime = CalculateRotationTime(predictedPos);

            float reloadTime = isReloading ? m_shootInterval : 0;
            float newTotalTime = flightTime + Mathf.Max(rotationTime, reloadTime);
            Vector3 newPredictedPos = targetPos + targetVel * newTotalTime;

#if UNITY_EDITOR
            Debug.DrawLine(predictedPos, predictedPos + Vector3.up * (i+1) / 10, Color.cyan, 5f);
#endif

            if (Vector3.Distance(newPredictedPos, predictedPos) < epsilon && Mathf.Abs(newTotalTime - totalTime) < epsilon)
            {
                predictedPos = newPredictedPos;

                Debug.Log($"Calculation done on {i+1} inetation {Vector3.Distance(newPredictedPos, predictedPos) < epsilon} {Mathf.Abs(newTotalTime - totalTime) < epsilon}");
                break;
            }

            predictedPos = newPredictedPos;
            totalTime = newTotalTime;
        }

         m_predictedPosition = predictedPos;

        if (m_baseAimPosition.Equals(default))
            m_baseAimPosition = m_predictedPosition;
    }

    protected override CannonProjectile Shoot()
    {
        var projectile = base.Shoot();
        CalculateRotation();
        return projectile;
    }

    protected override bool CanShoot()
    {
#if UNITY_EDITOR
        Debug.DrawLine(m_predictedPosition, m_shootPoint.position, Color.green, 0.01f);
        Debug.DrawLine(m_shootPoint.position, m_shootPoint.position + m_shootPoint.forward * 10, Color.red, 0.01f);
#endif
        return base.CanShoot() && Vector3.Angle(m_predictedPosition - m_shootPoint.position, m_shootPoint.forward) < 0.5f;
    }
    IEnumerator RotationRoutine()
    {
        while (true)
        {
            Vector3 toAimFlat = m_predictedPosition - m_transform.position;
            toAimFlat.y = 0;
            if (toAimFlat != Vector3.zero)
            {
                Quaternion targetYaw = Quaternion.LookRotation(toAimFlat);
                m_transform.rotation = Quaternion.RotateTowards(m_transform.rotation, targetYaw, m_yawSpeed * Time.deltaTime);
            }

            Vector3 toAim = m_predictedPosition - m_gunTransform.position;
            Vector3 localDir = m_transform.InverseTransformDirection(toAim);
            float targetPitch = Mathf.Atan2(localDir.y, Mathf.Sqrt(localDir.x * localDir.x + localDir.z * localDir.z)) * Mathf.Rad2Deg;
            Quaternion targetPitchRot = Quaternion.Euler(-targetPitch, 0, 0);
            m_gunTransform.localRotation = Quaternion.RotateTowards(m_gunTransform.localRotation, targetPitchRot, m_pitchSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private float CalculateRotationTime(Vector3 aimPoint)
    {
        Vector3 toTargetFlat = aimPoint - m_transform.position;
        toTargetFlat.y = 0;
        float targetYawAngle = Vector3.SignedAngle(m_transform.forward, toTargetFlat, Vector3.up);
        float rotationTimeYaw = Mathf.Abs(targetYawAngle) / m_yawSpeed;

        Vector3 toTarget = aimPoint - m_gunTransform.position;
        Vector3 localDir = m_transform.InverseTransformDirection(toTarget);
        float targetPitchAngle = Mathf.Atan2(localDir.y, Mathf.Sqrt(localDir.x * localDir.x + localDir.z * localDir.z)) * Mathf.Rad2Deg;
        float deltaPitch = Vector3.SignedAngle(m_gunTransform.forward, toTarget, m_gunTransform.right);
        float rotationTimePitch = Mathf.Abs(deltaPitch) / m_pitchSpeed;

        return Mathf.Max(rotationTimeYaw, rotationTimePitch);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_predictedPosition != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_predictedPosition, 0.4f);
            if (m_currentTarget != null)
                Gizmos.DrawWireSphere(m_currentTarget.m_Transform.position, 1f);
        }

    }
#endif
}
