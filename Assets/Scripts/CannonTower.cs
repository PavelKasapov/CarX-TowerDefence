using UnityEngine;
using VContainer;

public class CannonTower : BaseTower<CannonProjectile>
{
    [Inject] private TurretRotation m_turretRotation;
    [Inject] private BallisticAimSolver m_ballisticAimSolver;

    [SerializeField] private Transform m_gunTransform;
    [SerializeField] private float m_yawSpeed = 30f;
    [SerializeField] private float m_pitchSpeed = 30f;

    protected override void Awake()
    {
        base.Awake();
        m_ballisticAimSolver.Init(m_transform, m_shootPoint, m_gunTransform, m_yawSpeed, m_pitchSpeed, m_range);
        m_turretRotation.Init(m_transform, m_shootPoint, m_gunTransform, m_yawSpeed, m_pitchSpeed);
        m_targetTracker.OnTargetChange += PrepareForNextShot;
    }

    private void PrepareForNextShot()
    {
        if (m_ballisticAimSolver.CalculateRotation(m_targetTracker.CurrentTarget, m_projectilePrefab.m_Speed, m_isReloading ? m_shootInterval : 0, out var launchDirection, out var preparingTime))
        {
            m_turretRotation.RotateToDirection(launchDirection, preparingTime);
        }
        else
        {
            m_targetTracker.InvalidateTarget(m_targetTracker.CurrentTarget);
        }
    }

    protected override CannonProjectile Shoot()
    {
        var projectile = base.Shoot();
        Debug.Log(m_shootPoint.forward);
        projectile.Init(m_shootPoint.forward);
        PrepareForNextShot();
        return projectile;
    }

    protected override bool CanShoot()
    {
        return base.CanShoot() && m_turretRotation.IsAimedAtLaunchDirection();
    }
}
