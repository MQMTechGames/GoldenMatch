using UnityEngine;
using System.Collections;

public class EnemyAIController : MonoBehaviour
{
    public Transform _target;

    NavMeshAgent _navMeshAgent;

    void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        DebugUtils.assert(null != _navMeshAgent, "BasePlayerAI: nav mesh agent can not be null");

        if (null == _target)
        {
            _target = GameObjectUtils.getComponentByEntityTag<Transform>("Player");
        }
        DebugUtils.assert(null != _target, "BasePlayerAI: _target can not be null");
    }

    void Update()
    {
        _navMeshAgent.destination = _target.position;
    }
}