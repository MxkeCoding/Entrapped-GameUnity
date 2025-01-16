using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    public float damage;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collided with Player!");

            PlayerHealth pHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (pHealth != null)
            {
                pHealth.TakeDamage(damage); // Delegate damage handling
            }
            else
            {
                Debug.LogError("PlayerHealth script reference is missing!");
            }
        }
    }
}
