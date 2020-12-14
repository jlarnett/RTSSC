using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;

    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private float lastFireTime;

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if (target == null) return;

        if (!CanFireAtTarget()) return;

        //In range of target here
        Quaternion targetRotation =
            Quaternion.LookRotation(target.transform.position - transform.position);

        //Rotates the unit based upon our rotation and target rotation and speed. 
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        //Make sure we can fire based upon fire rate and fire timer.
        if (Time.time > (1 / fireRate) + lastFireTime)         //If firerate is 1 we fire 1 per second. E.G how ever many per second
        {

            //Get the projectile rotation to aim at center of targeter.
            Quaternion projectrileRotation =
                Quaternion.LookRotation(target.GetAimPoint().position - projectileSpawnPoint.position);

            //Instantiate the projectile instance onto the Server, but not network yet.
            GameObject projectileInstance = 
                Instantiate(projectilePrefab, projectileSpawnPoint.position, projectrileRotation);

            NetworkServer.Spawn(projectileInstance, connectionToClient);

            lastFireTime = Time.time;
        }
    }

    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude <= attackRange * attackRange;
    }







}
