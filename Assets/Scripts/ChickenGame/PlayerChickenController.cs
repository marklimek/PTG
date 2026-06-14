using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChickenController : MonoBehaviour
{
    private HashSet<Collider> wallsInProgress = new HashSet<Collider>();

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider != null && hit.collider.name.Contains("Wall"))
        {
            if (ChickenGameManager.Instance != null && ChickenGameManager.Instance.HasWallPassCharge())
            {
                if (!wallsInProgress.Contains(hit.collider))
                {
                    BoxCollider boxCol = hit.collider as BoxCollider;
                    if (boxCol != null && !boxCol.isTrigger)
                    {
                        StartCoroutine(PassThroughWallRoutine(boxCol));
                    }
                }
            }
        }
    }

    private IEnumerator PassThroughWallRoutine(BoxCollider wallCollider)
    {
        wallsInProgress.Add(wallCollider);
        
        wallCollider.isTrigger = true;
        
        Debug.Log("PlayerChickenController: Wall passage triggered! Passing through " + wallCollider.name);

        CharacterController playerCc = GetComponent<CharacterController>();

        while (wallCollider != null && IsOverlapping(playerCc, wallCollider))
        {
            yield return null;
        }

        if (wallCollider != null)
        {
            wallCollider.isTrigger = false;
            Debug.Log("PlayerChickenController: Wall passage complete! Restored collision for " + wallCollider.name);
        }

        wallsInProgress.Remove(wallCollider);

        if (ChickenGameManager.Instance != null)
        {
            ChickenGameManager.Instance.ConsumeWallPassCharge();
        }
    }

    private bool IsOverlapping(CharacterController player, BoxCollider wall)
    {
        if (player == null || wall == null) return false;

        Bounds playerBounds = player.bounds;
        Bounds wallBounds = wall.bounds;

        wallBounds.Expand(0.2f);

        return playerBounds.Intersects(wallBounds);
    }
}
