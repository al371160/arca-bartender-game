using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

public class CustomerBehavior : MonoBehaviour
{
    [Header("Customer Settings")]
    public bool isGood;
    public Seat seatTarget;
    public Bartender bartender;
    public GameManager gameManager;
    public UnityEvent<CustomerBehavior> OnLeave;

    [Header("Health & Ragdoll")]
    public int customerMaxHealth = 100;
    public int customerCurrentHealth = 100;
    public bool customerIsDead => customerCurrentHealth <= 0;
    public RagdollController ragdoll;

    [Header("Hit Cooldown")]
    public float hitCooldown = 1f;
    private float lastHitTime = -Mathf.Infinity;

    [Header("Wandering")]
    private NavMeshAgent agent;
    private bool waiting = false;
    private bool seated = false;
    private float patience = 20f;
    private float waitTimer = 0f;
    public BoxCollider wanderArea;
    private float wanderCooldown = 3f;
    private float wanderTimer = 0f;

    public bool IsWandering => waiting && seatTarget == null;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!ragdoll) ragdoll = GetComponent<RagdollController>();
    }

    void Update()
    {
        if (waiting)
        {
            waitTimer += Time.deltaTime;
            wanderTimer += Time.deltaTime;

            if (wanderTimer >= wanderCooldown)
            {
                Wander();
                wanderTimer = 0f;
            }

            if (waitTimer >= patience && isGood)
                BecomeBad();
        }

        if (seated && agent.remainingDistance < 0.5f)
        {
            seated = false;
            StartCoroutine(DoSeatRoutine());
        }
    }

    // ---------------------- Health & Hit System ----------------------
    public void TakeDamage(int amount)
    {
        customerCurrentHealth -= amount;
        customerCurrentHealth = Mathf.Max(0, customerCurrentHealth);
                Debug.Log("this brother has taken damage, current health: " + customerCurrentHealth + ""+ customerIsDead);

        if (customerCurrentHealth <= 0 /*&& !customerIsDead*/)
        {
            Die();
            Debug.Log ("this brother is dead.");
        }
    }

    private void Die()
    {
        // Stop AI movement safely
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Disable any wandering or seating logic
        waiting = false;
        seated = false;

        // Enable ragdoll
        if (ragdoll != null)
            ragdoll.SetRagdoll(true);

        Debug.Log($"{name} has died.");
    }


    public bool CanBeHit() => Time.time - lastHitTime >= hitCooldown;

    public void RegisterHit() => lastHitTime = Time.time;

    public void ApplyHit(Vector3 hitPoint)
    {
        ragdoll?.ApplyHit(hitPoint);
    }

    // ---------------------- Seating & Wandering ----------------------
    public void AssignSeat(Seat seat)
    {
        seatTarget = seat;
        waiting = false;
        agent.SetDestination(seat.transform.position);
        seated = true;
        seatTarget.Claim(this);
    }

    public void EnterBar(BoxCollider area)
    {
        waiting = true;
        wanderArea = area;
        Wander();
    }

    public void TryTakeSeat(Seat seat)
    {
        if (IsWandering)
            AssignSeat(seat);
    }

    private void Wander()
    {
        if (wanderArea == null) return;

        Vector3 randomPoint = new Vector3(
            Random.Range(wanderArea.bounds.min.x, wanderArea.bounds.max.x),
            transform.position.y,
            Random.Range(wanderArea.bounds.min.z, wanderArea.bounds.max.z)
        );
        agent.SetDestination(randomPoint);
    }

    private IEnumerator DoSeatRoutine()
    {
        gameManager.RegisterRequest(this);

        yield return new WaitForSeconds(Random.Range(10f, 20f));

        if (isGood)
        {
            gameManager.AddTip(Random.Range(1, 3));
        }
        else
        {
            gameManager.ApplyPenalty(1, "Bad customer caused trouble!");
            bartender.TakeDamage(10);
        }

        Leave();
    }

    private void BecomeBad()
    {
        isGood = false;
        waiting = false;
        Debug.Log($"{name} became bad inside the bar!");
    }

    private void Leave()
    {
        if (seatTarget != null)
            seatTarget.Release();

        OnLeave?.Invoke(this);
        Destroy(gameObject);
    }
}
