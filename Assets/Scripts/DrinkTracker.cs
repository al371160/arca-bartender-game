using UnityEngine;
using System.Collections.Generic;

public class DrinkTracker : MonoBehaviour
{
    [Header("Drink Info")]
    public string drinkName;
    public List<Ingredient> ingredients = new List<Ingredient>();

    [Header("Visual References")]
    public Renderer cupRenderer;
    public Transform liquidParent;

    private List<GameObject> liquidLayers = new List<GameObject>();

    void Start()
    {
        // Cache child layers for quick access
        if (liquidParent != null)
        {
            foreach (Transform child in liquidParent)
            {
                liquidLayers.Add(child.gameObject);
                child.gameObject.SetActive(false);
            }
        }
    }

    public void AddIngredient(Ingredient newIngredient)
    {
        if (newIngredient == null) return;

        ingredients.Add(newIngredient);
        UpdateDrinkName();
        UpdateDrinkVisual(newIngredient);
    }

    private void UpdateDrinkName()
    {
        drinkName = string.Join(" + ", ingredients.ConvertAll(i => i.ingredientName));
    }

    private void UpdateDrinkVisual(Ingredient ingredient)
    {
        if (cupRenderer != null)
        {
            cupRenderer.material.color = ingredient.color;
        }

        // Activate the next available layer
        if (liquidLayers.Count > ingredients.Count - 1)
        {
            GameObject layer = liquidLayers[ingredients.Count - 1];
            if (layer != null)
            {
                layer.SetActive(true);
                Renderer r = layer.GetComponent<Renderer>();
                if (r != null) r.material.color = ingredient.color;
            }
        }
    }
}
