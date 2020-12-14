using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targetable : NetworkBehaviour
{
    [SerializeField] private Transform aimPoint = null;

    public Transform GetAimPoint()
    {
        return aimPoint;
    }
}
