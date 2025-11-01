using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IngredientMachine : MonoBehaviour
{
    [Header("Machine Settings")]
    public Ingredient outputIngredient;
    public float interactionRange = 2f;
    public float dispenseTime = 2f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Progress UI")]
    public Canvas progressCanvas;
    public Image progressBar;

    private bool isDispensing = false;
    private Transform playerCam;

    private void Start()
    {
        playerCam = Camera.main?.transform;
        if (progressCanvas != null)
            progressCanvas.enabled = false;

    }

    private void Update()
    {
        if (isDispensing || outputIngredient == null || playerCam == null)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            TryUseMachine();
        }
    }

    private void TryUseMachine()
    {
        if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, interactionRange))
        {


            if (hit.collider.gameObject == gameObject)
            {

                InteractiveItem heldItem = FindHeldItem();

                if (heldItem != null)
                {
                    if (heldItem.TryGetComponent(out DrinkTracker cup))
                    {
                        StartCoroutine(DispenseRoutine(cup));
                    }
                }
                else
                {
                    Debug.LogWarning($"[{name}] No held item found (nothing parented to 'itemOrientation').");
                }
            }

        }

    }

    private InteractiveItem FindHeldItem()
    {
        InteractiveItem[] allItems = FindObjectsByType<InteractiveItem>(FindObjectsSortMode.None);
        foreach (var item in allItems)
        {
            if (item.transform.parent != null && item.transform.parent.name.Contains("itemOrientation"))
            {
                return item;
            }
        }

        return null;
    }

    private IEnumerator DispenseRoutine(DrinkTracker cup)
    {
        isDispensing = true;
        Debug.Log($"[{name}] Dispensing {outputIngredient.ingredientName} into {cup.name}...");

        if (progressCanvas != null) progressCanvas.enabled = true;
        if (progressBar != null) progressBar.fillAmount = 0f;

        float elapsed = 0f;
        while (elapsed < dispenseTime)
        {
            elapsed += Time.deltaTime;
            if (progressBar != null)
                progressBar.fillAmount = Mathf.Clamp01(elapsed / dispenseTime);
            yield return null;
        }

        cup.AddIngredient(outputIngredient);

        if (progressCanvas != null) progressCanvas.enabled = false;
        isDispensing = false;

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
