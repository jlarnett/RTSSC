using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]            //Stops client from logging.
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if (target != null)
        {
                                                                                                                //Done this way since its called in Update Every frame when target is not null.
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)        //Really Efficient way to check distance between unit and target
            {
                //Chase
                agent.SetDestination(target.transform.position);
            }
            else if (agent.hasPath)
            {
                //stop chasing
                agent.ResetPath();
            }

            return;
        }


        //Once the units get to their spot it clears path so units stop.
        if (!agent.hasPath) return;
        if (agent.remainingDistance > agent.stoppingDistance) return;
        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    [Server]
    public void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();

        //This is a Server Command. This method validates navmesh hit and then sets the players Agent destination to position if valid.
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;
        agent.SetDestination(hit.position);
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }


    #endregion
}
