using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    EnemyStats enemy;
    private Transform enemyPos;
    int damge = 3;
    float hitRange = 2.8f;
    void Start()
    {
        enemy = GameObject.Find("Enemy").GetComponent<EnemyStats>();
        enemyPos = GameObject.FindGameObjectWithTag("Enemy").transform;
    }
    
    // Update is called once per frame
    void Update()
    {

        Hit();   
        
    }

   void Hit()
    {
        if(enemyPos != null)
        {
            float dist = Vector3.Distance(enemyPos.position, transform.position);
            if (Input.GetKeyDown(KeyCode.F) && dist < hitRange)
            {
                enemy.TakeDamage(damge);
                Debug.Log("Hit!");
            }
        }
    } 
}
