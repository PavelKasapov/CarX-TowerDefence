using System.Collections;
using UnityEngine;

public class TurretRotation : MonoBehaviour
{
    private Transform m_shootPoint;
    private Transform m_gunTransform;
    private float m_maxYawSpeed;
    private float m_maxPitchSpeed;

    private float m_actualYawSpeed;
    private float m_actualPitchSpeed;
    private Vector3 m_launchDirection;
    private Transform m_transform;

    public float MaxYawSpeed => m_maxYawSpeed;
    public float MaxPitchSpeed => m_maxPitchSpeed;
    private void Awake()
    {
        m_transform = transform;
    }

    private void Start()
    {
        StartCoroutine(RotationRoutine());
    }

    public void Init(Transform shootPoint, Transform gunTransform, float maxYawSpeed, float maxPitchSpeed)
    {
        m_shootPoint = shootPoint;
        m_gunTransform = gunTransform;
        m_maxYawSpeed = maxYawSpeed;
        m_maxPitchSpeed = maxPitchSpeed;
    }

    public void RotateToDirection(Vector3 launchDirection, float rotationTime = -1)
    {
        m_launchDirection = launchDirection;
        if (rotationTime == -1)
        {
            m_actualYawSpeed = m_maxYawSpeed;
            m_actualPitchSpeed = m_maxPitchSpeed;
        }
        else
        {
            AdjustRotationSpeed(rotationTime);
        }
    }

    public bool IsAimedAtLaunchDirection()
    {
        if (m_launchDirection == Vector3.zero)
            return false;

        return Vector3.Angle(m_launchDirection, m_shootPoint.forward) < 0.1f;
    }

    IEnumerator RotationRoutine()
    {
        while (true)
        {
#if UNITY_EDITOR
            Debug.DrawLine(m_shootPoint.position, m_shootPoint.position + m_launchDirection, Color.green, 0.01f);
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

    private void AdjustRotationSpeed(float preparingTime)
    {
        Vector3 toTargetFlat = m_launchDirection;
        toTargetFlat.y = 0;
        float yawAngle = Vector3.SignedAngle(m_transform.forward, toTargetFlat, Vector3.up);
        float requiredYawSpeed = Mathf.Abs(yawAngle) / preparingTime;
        m_actualYawSpeed = Mathf.Min(m_maxYawSpeed, requiredYawSpeed);

        Vector3 localDir = m_transform.InverseTransformDirection(m_launchDirection);
        float targetPitch = -Mathf.Atan2(localDir.y, new Vector2(localDir.x, localDir.z).magnitude) * Mathf.Rad2Deg;
        float currentPitch = m_gunTransform.localEulerAngles.x;
        if (currentPitch > 180) currentPitch -= 360;
        float deltaPitch = Mathf.DeltaAngle(currentPitch, targetPitch);
        float requiredPitchSpeed = Mathf.Abs(deltaPitch) / preparingTime;
        m_actualPitchSpeed = Mathf.Min(m_maxPitchSpeed, requiredPitchSpeed);
    }
}