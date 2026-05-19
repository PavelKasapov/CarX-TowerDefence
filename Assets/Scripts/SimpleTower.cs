public class SimpleTower : BaseTower<GuidedProjectile>
{
    protected override GuidedProjectile Shoot()
    {
        var projectile = base.Shoot();
        projectile.Init(m_targetTracker.CurrentTarget.m_Transform);
        return projectile;
    }
}