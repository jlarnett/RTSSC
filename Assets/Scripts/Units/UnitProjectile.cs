using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private int damageToDeal = 20;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private float destroyAfterSeconds = 5f;

    void Start()
    {
        //Applys force to a rigidbody
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer()
    {
        //On the Start of this gameobjects server life. Invoke the destorymethod below after the specified time
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }


    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        //We are on the server and we are told the projectile hit something. First We check if it has a Network Identity Component
        if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            //If the connection is our return. Stops us form dealing damage to our own objects
            if (networkIdentity.connectionToClient == connectionToClient) return;
        }

        if (other.TryGetComponent<Health>(out Health health))
        {
            //Deal Damage if collider has health component and doesnt below to projectile connection
            health.DealDamage(damageToDeal);
        }

        DestroySelf();
    }

    [Server]
    private void DestroySelf()
    {
        //Destroys a networked object.
        NetworkServer.Destroy(gameObject);
    }
}
