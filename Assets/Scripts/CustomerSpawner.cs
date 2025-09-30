using UnityEngine;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
    public CustomerBehavior customerPrefab;
    public Transform[] doors;
    public Bartender bartender;
    public GameManager gameManager;
    public BoxCollider waitingArea;

    public float spawnInterval = 6f;
    private float timer;

    private List<CustomerBehavior> activeCustomers = new List<CustomerBehavior>();
    private List<Seat> seats = new List<Seat>();

    void Start() {
        seats.AddRange(FindObjectsOfType<Seat>());
    }

    void Update() {
        timer += Time.deltaTime;
        if (timer >= spawnInterval) {
            TrySpawnCustomer();
            timer = 0f;
        }
    }

    void TrySpawnCustomer() {
        Transform door = doors[Random.Range(0, doors.Length)];

        CustomerBehavior c = Instantiate(customerPrefab, door.position, Quaternion.identity);
        c.bartender = bartender;
        c.gameManager = gameManager;

        c.isGood = (Random.value < 0.7f);

        Seat freeSeat = GetFreeSeat();
        if (freeSeat != null) {
            c.AssignSeat(freeSeat);
        } else {
            c.EnterBar(waitingArea);
        }

        c.OnLeave.AddListener(RemoveCustomer);
        activeCustomers.Add(c);
    }

    Seat GetFreeSeat() {
        foreach (Seat seat in seats) {
            if (!seat.IsOccupied) return seat;
        }
        return null;
    }

    void RemoveCustomer(CustomerBehavior c) {
        activeCustomers.Remove(c);
    }
}
