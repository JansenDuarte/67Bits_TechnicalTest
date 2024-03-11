using System.Collections;
using UnityEngine;

public class MoneyBehaviour : MonoBehaviour
{
    /*      MoneyBehaviour        */
    /*
            Created by Jansen Duarte    
    
            Controls money spawning and collection
    */


    #region PUBLIC_METHODS
    public void Spawn(Vector3 _direction)
    {
        m_collider.enabled = false;
        Vector3 deltaPos = _direction - transform.position;

        Vector3 velocity = Vector3.zero;
        velocity.x = deltaPos.x * MathConstants.COS_45;
        velocity.z = deltaPos.z * MathConstants.COS_45;
        velocity.y = (deltaPos.y - MathConstants.g / 2f) / MathConstants.SIN_45;

        //DEBUG this line helps in the editor
        Debug.DrawLine(transform.position, transform.position + velocity, Color.magenta, 10f);

        m_rigidbody.AddForce(velocity.normalized * spawnSpeed, ForceMode.VelocityChange);
        StartCoroutine(Resume_Collisions());
    }

    public uint Collect()
    {
        m_collectParticles.Play();
        StartCoroutine(De_Spawn());
        return moneyValue;
    }
    #endregion //PUBLIC_METHODS



    #region PRIVATE_METHODS
    private IEnumerator Resume_Collisions()
    {
        yield return new WaitForSeconds(0.5f);
        m_collider.enabled = true;
        yield break;
    }

    private IEnumerator De_Spawn()
    {
        //Give time to the particle system
        m_renderer.enabled = false;
        m_collider.enabled = false;
        yield return new WaitForSeconds(1f);

        transform.position = Vector3.down * 10f;
        yield return new WaitForEndOfFrame();
        Destroy(this);
    }
    #endregion //PRIVATE_METHODS





    #region VARIABLES
    //Public variables
    public uint moneyValue;
    public float spawnSpeed;

    //Serialized variables
    [SerializeField] Rigidbody m_rigidbody;
    [SerializeField] Collider m_collider;
    [SerializeField] ParticleSystem m_collectParticles;
    [SerializeField] MeshRenderer m_renderer;
    #endregion //VARIABLES
}
