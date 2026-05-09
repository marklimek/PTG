using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] Text hpText = null;
    [SerializeField] Text atcText = null;
    [SerializeField] Text defText = null;

   public void UpdateStats(int currentHealth, int attack, int defence)
    {
        hpText.text = "HP:"+currentHealth.ToString();
        atcText.text = "ATC:"+attack.ToString();
        defText.text = "DEF:"+defence.ToString();
    }
}
