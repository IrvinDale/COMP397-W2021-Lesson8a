using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NinjaState
{
    IDLE,
    RUN,
    JUMP,
    KICK
}

public class NinjaBehaviour : MonoBehaviour
{
    [Header("Line of Sight")]
    public bool HasLOS;

    public GameObject player;

    private NavMeshAgent agent;
    private Animator animator;

    [Header("Attack")]
    public float attackDistance;
    public PlayerBehaviour playerBehaviour;
    public float damageDelay = 1.0f;
    public bool isAttacking = false;
    public float kickForce = 0.01f;
    public float distanceToPlayer;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        playerBehaviour = FindObjectOfType<PlayerBehaviour>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (HasLOS)
        {
            agent.SetDestination(player.transform.position);
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        }


        // if has line of sight & distance to player is less than the attack distance & currently not attack then do the attack animation
        if (HasLOS && distanceToPlayer < attackDistance && !isAttacking)
        {
            // could be an attack
            animator.SetInteger("AnimState", (int)NinjaState.KICK);
            transform.LookAt(transform.position - player.transform.forward);
           
            DoKickDamage();
            isAttacking = true;
  
            if (agent.isOnOffMeshLink)
            {
                animator.SetInteger("AnimState", (int)NinjaState.JUMP);
            }
        }
        // if has line of sight & distance to the player greater than attack distance
        else if (HasLOS && distanceToPlayer > attackDistance)
        {
            animator.SetInteger("AnimState", (int)NinjaState.RUN);
            isAttacking = false;
        }
        else
        {
            animator.SetInteger("AnimState", (int)NinjaState.IDLE);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            HasLOS = true;
            player = other.transform.gameObject;
        }
    }

    private void DoKickDamage()
    {
        playerBehaviour.TakeDamage(20);
        StartCoroutine(kickBack());
        //yield return new WaitForSeconds(damageDelay);
        //StopCoroutine(DoKickDamage());
    }

    private IEnumerator kickBack()
    {
        yield return new WaitForSeconds(damageDelay);

        var direction = Vector3.Normalize(player.transform.position - transform.position);
        playerBehaviour.controller.SimpleMove(direction * kickForce);
        StopCoroutine(kickBack());
    }
}
