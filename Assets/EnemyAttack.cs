using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{

    private float timeBtwShoots;
    [SerializeField] float startTimeBtwShoots = 1f;
    private Transform player;
    [SerializeField] float shootRange = 2.8f;
    int damage = 1;

    PlayerStats stats;
  

    private void Awake()
    {
        timeBtwShoots = startTimeBtwShoots;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        stats = GameObject.Find("Player").GetComponent<PlayerStats>();

    }

    void Update()
    {
        if(player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < shootRange)
            {
                Shoot();
            }
        }

    }
   

    private void Shoot()
    {
        if (timeBtwShoots <= 0)
        {
            timeBtwShoots = startTimeBtwShoots;
            stats.TakeDamage(damage);
            Debug.Log("I hit you");
        }
        else
        {

            timeBtwShoots -= Time.deltaTime;
        }
    }


}
