using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

public class CustomerBehavior : MonoBehaviour
{
    public bool isGood;
    public Seat seatTarget;
    public Bartender bartender;
    public GameManager gameManager;

    public UnityEvent<CustomerBehavior> OnLeave;

    private NavMeshAgent agent;
    private bool waiting = false;
    private bool seated = false;

    private float patience = 20f;
    private float waitTimer = 0f;

    public bool IsWandering => waiting && seatTarget == null;
    public BoxCollider wanderArea; // assign in inspector
    private float wanderCooldown = 3f;
    private float wanderTimer = 0f;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update() {
        if (waiting) {
            waitTimer += Time.deltaTime;
            wanderTimer += Time.deltaTime;

            if (wanderTimer >= wanderCooldown) {
                Wander();
                wanderTimer = 0f;
            }

            if (waitTimer >= patience && isGood) {
                BecomeBad();
            }
        }

        if (seated && agent.remainingDistance < 0.5f) {
            seated = false;
            StartCoroutine(DoSeatRoutine());
        }
    }

    public void AssignSeat(Seat seat) {
        seatTarget = seat;
        waiting = false;
        agent.SetDestination(seat.transform.position);
        seated = true;
        seatTarget.Claim(this);
    }



    public void EnterBar(BoxCollider area) {
        waiting = true;
        wanderArea = area;
        Wander();
    }

    public void TryTakeSeat(Seat seat) {
        if (IsWandering) {
            AssignSeat(seat);
        }
    }

    private void Wander() {
        if (wanderArea == null) return;
        Vector3 randomPoint = new Vector3(
            Random.Range(wanderArea.bounds.min.x, wanderArea.bounds.max.x),
            transform.position.y,
            Random.Range(wanderArea.bounds.min.z, wanderArea.bounds.max.z)
        );
        agent.SetDestination(randomPoint);
    }

    private IEnumerator DoSeatRoutine() {
        gameManager.RegisterRequest(this);

        yield return new WaitForSeconds(Random.Range(10f, 20f));

        if (isGood) {
            gameManager.AddTip(Random.Range(1, 3));
        } else {
            gameManager.ApplyPenalty(1, "Bad customer caused trouble!");
            bartender.TakeDamage(10);
        }

        Leave();
    }

    private void BecomeBad() {
        isGood = false;
        waiting = false;
        Debug.Log("Customer became bad inside the bar!");
    }

    private void Leave() {
        if (seatTarget != null) {
            seatTarget.Release();
        }
        OnLeave?.Invoke(this);
        Destroy(gameObject);
    }
}
