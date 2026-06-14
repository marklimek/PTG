using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveWheel : MonoBehaviour
{
    private WheelType type;
    private bool isCollected = false;

    public void Initialize(WheelType wheelType)
    {
        type = wheelType;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player") || other.name == "Player")
        {
            isCollected = true;

            if (ChickenGameManager.Instance != null)
            {
                ChickenGameManager.Instance.OnCollectWheel(type);
            }

            Destroy(gameObject);
        }
    }
}
