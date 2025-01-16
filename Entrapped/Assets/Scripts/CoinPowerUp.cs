using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        Dash,
        Jump,
        WallClimb
    }

    public PowerUpType powerUpType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // Apply the corresponding power-up effect
                switch (powerUpType)
                {
                    case PowerUpType.Dash:
                        player.GrantDashAbility();  // Grant Dash power-up
                        break;

                    case PowerUpType.Jump:
                        player.GrantExtraJump();  // Grant Extra Jump power-up
                        break;

                    case PowerUpType.WallClimb:
                        player.GrantWallClimbAbility();  // Grant Wall Climb power-up
                        break;
                }

                // Destroy the coin after collection
                Destroy(gameObject);
            }
        }
    }
}
