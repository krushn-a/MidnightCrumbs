using UnityEngine;

public class capsuleCollider : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Debug.Log("Collided with obstacle: " + collision.gameObject.name);
        }
    }
}
