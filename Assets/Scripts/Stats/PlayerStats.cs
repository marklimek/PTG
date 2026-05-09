using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : CharacterStats
{

    private PlayerHUD hud;
    // Start is called before the first frame update
    void Start()
    {
        hud = GetComponent<PlayerHUD>();
        InitVariables();
        Debug.Log("Player HP: "+maxHealth);
    }

    // Update is called once per frame
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

