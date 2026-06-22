using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class SteeringControllerBase
{
    public abstract float GetAngleToTurn(Transform carTransform, NavigationContext context);
}

public enum SteeringAlgorithm
{
    DirectWaypoint,
    PurePursuit,
    Stanley,
    NavMeshDirect
}

public static class SteeringControllerFactory
{
    public static SteeringControllerBase Create(
        SteeringAlgorithm algorithm,
        LineFactory lineFactory,
        List<Vector3> points,
        NavMeshAgent navMeshAgent = null,
        int finishIndex = 0)
    {
        return algorithm switch
        {
            SteeringAlgorithm.PurePursuit   => new PurePursuitController(lineFactory),
            SteeringAlgorithm.Stanley       => new StanleyController(lineFactory),
            SteeringAlgorithm.NavMeshDirect => new NavMeshDirectController(lineFactory, navMeshAgent, points, finishIndex),
            _                               => new DirectWaypointController(lineFactory)
        };
    }
}
