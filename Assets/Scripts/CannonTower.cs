using System.Collections;
using UnityEngine;

public class CannonTower : BaseTower<CannonProjectile>
{
    [SerializeField] private Transform m_gunTransform;
    [SerializeField] private float m_yawSpeed = 30f;
    [SerializeField] private float m_pitchSpeed = 30f;

    private float m_actualYawSpeed;
    private float m_actualPitchSpeed;

    private Vector3 m_baseAimPosition;
    private Vector3 m_launchDirection;
#if UNITY_EDITOR
    private Vector3 debug_predictedPos;
#endif
    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(RotationRoutine());
        m_targetTracker.OnTargetChange += CalculateRotation;
    }

    private void CalculateRotation()
    {
        if (m_targetTracker.CurrentTarget == null)
        {
            m_actualYawSpeed = m_yawSpeed;
            m_actualPitchSpeed = m_pitchSpeed;
            m_launchDirection = m_baseAimPosition;
            return;
        }

        float projectileSpeed = m_projectilePrefab.m_Speed;
        Vector3 towerPos = m_shootPoint.position;
        Vector3 targetPos = m_targetTracker.CurrentTarget.m_Transform.position;
        Vector3 targetVel = m_targetTracker.CurrentTarget.m_Transform.forward * m_targetTracker.CurrentTarget.m_Speed;

        float totalTime = Vector3.Distance(towerPos, targetPos) / projectileSpeed;
        Vector3 predictedPos = targetPos + targetVel * totalTime;
        float epsilon = 0.1f;
        float preparingTime = 0;
        Vector3 launchVel = Vector3.zero;
            float flightTime = 0;

        for (int i = 0; i < 5; i++)
        {
            float distance = Vector3.Distance(towerPos, predictedPos);

            if (distance < m_targetTracker.m_Range && GetBallisticVelocity(towerPos, predictedPos, projectileSpeed, Physics.gravity, out launchVel, out float ballisticTime))
            {
                flightTime = ballisticTime;
            }
            else
            {
                m_targetTracker.InvalidateTarget(m_targetTracker.CurrentTarget);
                return;
            }
            
            float rotationTime = CalculateRotationTime(launchVel);

            float reloadTime = m_isReloading ? m_shootInterval : 0;
            preparingTime = Mathf.Max(rotationTime, reloadTime);
            float newTotalTime = flightTime + preparingTime;
            Vector3 newPredictedPos = targetPos + targetVel * newTotalTime;

#if UNITY_EDITOR
            Debug.DrawLine(newPredictedPos, newPredictedPos + Vector3.up * (i + 1) / 10, Color.cyan, 5f);
            debug_predictedPos = newPredictedPos;
#endif

            if (Vector3.Distance(newPredictedPos, predictedPos) < epsilon && Mathf.Abs(newTotalTime - totalTime) < epsilon)
            {
                m_launchDirection = launchVel;
                Debug.Log($"Calculation done on {i+1} iteration {Vector3.Distance(newPredictedPos, predictedPos) < epsilon} {Mathf.Abs(newTotalTime - totalTime) < epsilon}");
                break;
            }

            predictedPos = newPredictedPos;
            totalTime = newTotalTime;
        }
        m_launchDirection = launchVel;
        AdjustRotationSpeed(preparingTime);

        if (m_baseAimPosition.Equals(default))
            m_baseAimPosition = m_launchDirection;
    }

    protected override CannonProjectile Shoot()
    {
        var projectile = base.Shoot();

        projectile.Init(m_launchDirection);
        CalculateRotation();
        return projectile;
    }

    protected override bool CanShoot()
    {
        return base.CanShoot() && Vector3.Angle(m_launchDirection, m_shootPoint.forward) < 0.1f;
    }


    IEnumerator RotationRoutine()
    {
        while (true)
        {
#if UNITY_EDITOR
            Debug.DrawLine( m_shootPoint.position, m_shootPoint.position + m_launchDirection, Color.green, 0.01f);
            Debug.DrawLine(m_shootPoint.position, m_shootPoint.position + m_shootPoint.forward * 10, Color.red, 0.01f);
#endif

            Vector3 toAimFlat = m_launchDirection;
            toAimFlat.y = 0;
            if (toAimFlat != Vector3.zero)
            {
                Quaternion targetYaw = Quaternion.LookRotation(toAimFlat);
                m_transform.rotation = Quaternion.RotateTowards(m_transform.rotation, targetYaw, m_actualYawSpeed * Time.deltaTime);
            }

            Vector3 toAim = m_launchDirection;
            Vector3 localDir = m_transform.InverseTransformDirection(toAim);
            float targetPitch = Mathf.Atan2(localDir.y, Mathf.Sqrt(localDir.x * localDir.x + localDir.z * localDir.z)) * Mathf.Rad2Deg;
            Quaternion targetPitchRot = Quaternion.Euler(-targetPitch, 0, 0);
            m_gunTransform.localRotation = Quaternion.RotateTowards(m_gunTransform.localRotation, targetPitchRot, m_actualPitchSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private float CalculateRotationTime(Vector3 aimDirection)
    {
        Vector3 toTargetFlat = aimDirection;
        toTargetFlat.y = 0;
        float targetYawAngle = Vector3.SignedAngle(m_transform.forward, toTargetFlat, Vector3.up);
        float rotationTimeYaw = Mathf.Abs(targetYawAngle) / m_yawSpeed;

        Vector3 localDir = m_transform.InverseTransformDirection(aimDirection);
        float targetPitch = -Mathf.Atan2(localDir.y, new Vector2(localDir.x, localDir.z).magnitude) * Mathf.Rad2Deg;
        float currentPitch = m_gunTransform.localEulerAngles.x;
        if (currentPitch > 180) currentPitch -= 360;
        float deltaPitch = Mathf.DeltaAngle(currentPitch, targetPitch);
        float rotationTimePitch = Mathf.Abs(deltaPitch) / m_pitchSpeed;

        return Mathf.Max(rotationTimeYaw, rotationTimePitch);
    }

    bool GetBallisticVelocity(Vector3 start, Vector3 end, float speed, Vector3 gravity, out Vector3 velocity, out float flightTime)
    {
        velocity = Vector3.zero;
        flightTime = 0;
        Vector3 delta = end - start;
        Vector3 deltaXZ = new Vector3(delta.x, 0, delta.z);
        float distXZ = deltaXZ.magnitude;
        if (distXZ < 0.001f)
        {
            velocity = Vector3.up * speed;
            return false;
        }
        float dy = delta.y;
        float g = gravity.magnitude;
        float v2 = speed * speed;
        float v4 = v2 * v2;
        float discriminant = v4 - g * (g * distXZ * distXZ + 2 * dy * v2);
        if (discriminant < 0) return false;
        float sqrtDisc = Mathf.Sqrt(discriminant);
        float tanTheta1 = (v2 - sqrtDisc) / (g * distXZ);
        float tanTheta2 = (v2 + sqrtDisc) / (g * distXZ);
        float tanTheta = Mathf.Min(tanTheta1, tanTheta2);
        float theta = Mathf.Atan(tanTheta);
        float vXZ = speed * Mathf.Cos(theta);
        float vy = speed * Mathf.Sin(theta);
        Vector3 dirXZ = deltaXZ.normalized;
        velocity = dirXZ * vXZ + Vector3.up * vy;
        flightTime = distXZ / vXZ;
        return true;
    }

    private void AdjustRotationSpeed(float preparingTime)
    {
        Vector3 toTargetFlat = m_launchDirection;
        toTargetFlat.y = 0;
        float yawAngle = Vector3.SignedAngle(m_transform.forward, toTargetFlat, Vector3.up);
        float requiredYawSpeed = Mathf.Abs(yawAngle) / preparingTime;
        m_actualYawSpeed = Mathf.Min(m_yawSpeed, requiredYawSpeed);

        Vector3 localDir = m_transform.InverseTransformDirection(m_launchDirection);
        float targetPitch = -Mathf.Atan2(localDir.y, new Vector2(localDir.x, localDir.z).magnitude) * Mathf.Rad2Deg;
        float currentPitch = m_gunTransform.localEulerAngles.x;
        if (currentPitch > 180) currentPitch -= 360;
        float deltaPitch = Mathf.DeltaAngle(currentPitch, targetPitch);
        float requiredPitchSpeed = Mathf.Abs(deltaPitch) / preparingTime;
        m_actualPitchSpeed = Mathf.Min(m_pitchSpeed, requiredPitchSpeed);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (debug_predictedPos != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(debug_predictedPos, 0.4f);
            if (m_targetTracker.CurrentTarget != null)
                Gizmos.DrawWireSphere(m_targetTracker.CurrentTarget.m_Transform.position, 1f);
        }

    }
#endif
}
