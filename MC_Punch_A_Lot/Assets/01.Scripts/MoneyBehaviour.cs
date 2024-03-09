using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyBehaviour : MonoBehaviour
{
    public uint moneyValue;
    public float spawnSpeed;

    [SerializeField] Rigidbody m_rigidbody;
    [SerializeField] Collider m_collider;


    public void Spawn(Vector3 _direction)
    {
        m_collider.enabled = false;
        Vector3 deltaPos = _direction - transform.position;

        Vector3 velocity = Vector3.zero;
        velocity.x = deltaPos.x * MathConstants.COS_45;
        velocity.z = deltaPos.z * MathConstants.COS_45;
        velocity.y = (deltaPos.y - MathConstants.g / 2f) / MathConstants.SIN_45;

        Debug.DrawLine(transform.position, transform.position + velocity, Color.magenta, 10f);
        m_rigidbody.AddForce(velocity.normalized * spawnSpeed, ForceMode.VelocityChange);
        StartCoroutine(Resume_Collisions());
    }

    IEnumerator Resume_Collisions()
    {
        yield return new WaitForSeconds(1f);
        m_collider.enabled = true;
        yield break;
    }


    public uint Collect()
    {
        StartCoroutine(De_Spawn());
        return moneyValue;
    }

    IEnumerator De_Spawn()
    {
        transform.position = Vector3.down * 10f;
        yield return new WaitForEndOfFrame();
        Destroy(this);
    }
}
