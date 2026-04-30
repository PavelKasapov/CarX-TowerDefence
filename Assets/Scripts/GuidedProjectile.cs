using UnityEngine;

public class GuidedProjectile : BaseProjectile
{
    public Transform m_target;
    protected override void Move()
    {
        if (m_target == null)
        {
            m_OnDespawn?.Invoke();
            return;
        }

        Vector3 newPosition = Vector3.MoveTowards(m_transform.position, m_target.position, m_speed * Time.deltaTime);
        m_transform.position = newPosition;
    }
}
