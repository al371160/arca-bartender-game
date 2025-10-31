using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomerBehavior : MonoBehaviour
{
    [Header("Customer Settings")]
    public bool isGood = true;
    public Seat seatTarget;
    public Bartender bartender;
    public GameManager gameManager;
    public UnityEvent<CustomerBehavior> OnLeave;

    [Header("Drink Order")]
    public CustomerOrder order; // The customer's requested drink

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
    [SerializeField] private float patience = 20f; // how long before they get angry
    private float waitTimer = 0f;
    public BoxCollider wanderArea;
    [SerializeField] private float wanderCooldown = 3f;
    private float wanderTimer = 0f;

    public bool IsWandering => waiting && seatTarget == null;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!ragdoll) ragdoll = GetComponent<RagdollController>();
        if (!order) order = GetComponent<CustomerOrder>();
        if (!gameManager) gameManager = FindFirstObjectByType<GameManager>();

        customerCurrentHealth = customerMaxHealth;
    }

    void Update()
    {
        HandleWandering();
        HandleSeating();
    }

    // ---------------------- DRINK SERVING ----------------------
    public void ReceiveDrink(DrinkTracker cup)
    {
        if (order == null || cup == null)
        {
            Debug.LogWarning($"{name} has no order or received null cup!");
            return;
        }

        if (order.CheckDrink(cup))
        {
            Debug.Log($"✅ {name} received the correct drink: {order.requestedRecipe.recipeName}");
            
            // Add tip via GameManager
            if (gameManager)
                gameManager.AddTip(Random.Range(3, 6));

            // Customer leaves happily
            Leave();
            
            // Remove served cup
            Destroy(cup.gameObject);
        }
        else
        {
            Debug.Log($"❌ {name} got the WRONG drink!");
            if (gameManager)
                gameManager.ApplyPenalty(1, $"{name} was unhappy with the wrong drink!");

            BecomeBad();
        }
    }

    // ---------------------- HEALTH & HIT SYSTEM ----------------------
    public void TakeDamage(int amount)
    {
        customerCurrentHealth = Mathf.Max(0, customerCurrentHealth - amount);
        Debug.Log($"{name} took {amount} damage (HP: {customerCurrentHealth}/{customerMaxHealth})");

        if (customerCurrentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        waiting = false;
        seated = false;

        ragdoll?.SetRagdoll(true);
        Debug.Log($"{name} has died.");
    }

    public bool CanBeHit() => Time.time - lastHitTime >= hitCooldown;
    public void RegisterHit() => lastHitTime = Time.time;
    public void ApplyHit(Vector3 hitPoint) => ragdoll?.ApplyHit(hitPoint);

    // ---------------------- SEATING & WANDERING ----------------------
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

    private void HandleWandering()
    {
        if (!waiting) return;

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

    private void HandleSeating()
    {
        if (seated && agent.remainingDistance < 0.5f)
        {
            seated = false;
            StartCoroutine(DoSeatRoutine());
        }
    }

    private IEnumerator DoSeatRoutine()
    {
        gameManager?.RegisterRequest(this);

        // Wait while the player prepares drink
        yield return new WaitForSeconds(Random.Range(10f, 20f));

        if (isGood)
        {
            // They waited patiently, small passive tip
            gameManager?.AddTip(Random.Range(1, 3));
        }
        else
        {
            // Angry customer penalizes the player
            gameManager?.ApplyPenalty(1, "Bad customer caused trouble!");
            bartender?.TakeDamage(10);
        }

        Leave();
    }

    public void BecomeBad()
    {
        if (!isGood) return;
        isGood = false;
        waiting = false;
        Debug.Log($"{name} became bad inside the bar!");
        // Add optional: play angry animation, UI, etc.
    }

    public void Leave()
    {
        if (seatTarget != null)
            seatTarget.Release();

        OnLeave?.Invoke(this);
        Destroy(gameObject);
    }
}
