using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : CharacterStats
{

    private PlayerHUD hud;
    void Start()
    {
        hud = GetComponent<PlayerHUD>();
        InitVariables();
        Debug.Log("Player HP: "+maxHealth);
    }

    void Update()
    {
        
    }

    public override void CheckHealth()
    {
        base.CheckHealth();
        hud.UpdateStats(health, attack, defence);
    }
    public override void Die()
    {
        base.Die();
        SceneManager.LoadScene("GameOver");
    }
}

