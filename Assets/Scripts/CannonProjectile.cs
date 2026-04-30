using UnityEngine;

public class CannonProjectile : BaseProjectile
{
    protected override void Move()
    {
        var translation = m_transform.forward * m_speed * Time.deltaTime;
        m_transform.Translate(translation, Space.World);
    }
}