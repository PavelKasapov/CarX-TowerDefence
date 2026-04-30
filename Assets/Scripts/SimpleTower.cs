public class SimpleTower : BaseTower<GuidedProjectile>
{
    protected override GuidedProjectile Shoot()
    {
        var projectile = base.Shoot();
        projectile.m_target = m_currentTarget.m_Transform;
        return projectile;
    }
}