using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHUD : MonoBehaviour
{
    [SerializeField] Text hpText = null;
   

    public void UpdateStats(int currentHealth)
    {
        hpText.text = "Enemy HP:" + currentHealth.ToString();
    }
}
