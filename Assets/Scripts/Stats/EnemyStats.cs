using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats
{
    private EnemyHUD hud;
    void Start()
    {
        hud = GetComponent<EnemyHUD>();
        InitVariables();
        Debug.Log("Enemy HP: "+maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void CheckHealth()
    {
        base.CheckHealth();
        hud.UpdateStats(health);
    }
}