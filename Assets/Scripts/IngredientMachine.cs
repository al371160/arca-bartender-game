using UnityEngine;

public class IngredientMachine : MonoBehaviour
{
    [Header("Machine Settings")]
    public Ingredient outputIngredient;
    public float useCooldown = 1.5f;
    public float interactionRange = 2f;
    public KeyCode interactKey = KeyCode.E;

    private bool canUse = true;
    private Transform playerCam;

    private void Start()
    {
        playerCam = Camera.main.transform;
    }

    private void Update()
    {
        if (!canUse || outputIngredient == null || playerCam == null)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            TryUseMachine();
        }
    }

    private void TryUseMachine()
    {
        // Raycast from camera to check if looking at this machine
        if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, interactionRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                // Find the held item
                InteractiveItem heldItem = FindHeldItem();
                if (heldItem != null && heldItem.TryGetComponent(out DrinkTracker cup))
                {
                    AddIngredientToCup(cup);
                }
            }
        }
    }

    private InteractiveItem FindHeldItem()
    {
        // Find any InteractiveItem currently parented under the player's holdPoint
        InteractiveItem[] allItems = FindObjectsOfType<InteractiveItem>();
        foreach (var item in allItems)
        {
            if (item.transform.parent != null && item.isActiveAndEnabled)
            {
                // We assume only one is held at a time
                Debug.Log(item);
                return item;
            }
        }
        return null;
    }

    private void AddIngredientToCup(DrinkTracker cup)
    {
        cup.AddIngredient(outputIngredient);
        Debug.Log($"Added {outputIngredient.ingredientName} to {cup.gameObject.name}");
        StartCoroutine(Cooldown());
    }

    private System.Collections.IEnumerator Cooldown()
    {
        canUse = false;
        yield return new WaitForSeconds(useCooldown);
        canUse = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
