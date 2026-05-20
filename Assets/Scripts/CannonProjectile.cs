using UnityEngine;

public class CannonProjectile : BaseProjectile
{
    private Vector3 m_velocity;

    public void Init(Vector3 direction)
    {
        m_velocity = direction * m_speed;
    }

    protected override void Move()
    {
        m_velocity += Physics.gravity * Time.deltaTime;
        transform.position += m_velocity * Time.deltaTime;
        if (m_velocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(m_velocity);
    }
}