using UnityEngine;
using System.Collections.Generic;

public class DrinkTracker : MonoBehaviour
{
    [Header("Drink Info")]
    public string drinkName;
    public List<Ingredient> ingredients = new List<Ingredient>();

    [Header("All Drink Recipes")]
    public DrinkRecipe[] allRecipes; // assign your drink ScriptableObjects in Inspector


    [Header("Visual References")]
    public Renderer cupRenderer;
    public Transform liquidParent;

    private List<GameObject> liquidLayers = new List<GameObject>();

    void Start()
    {
        // Cache liquid layers (disabled by default)
        if (liquidParent != null)
        {
            foreach (Transform child in liquidParent)
            {
                liquidLayers.Add(child.gameObject);
                child.gameObject.SetActive(false);
            }
        }

        UpdateDrinkName(); // initialize name
    }

    public void AddIngredient(Ingredient newIngredient)
    {
        if (newIngredient == null)
        {
            Debug.LogWarning("Tried to add a null ingredient!");
            return;
        }

        ingredients.Add(newIngredient);
        Debug.Log($"🧪 Added ingredient: {newIngredient.ingredientName}");
        UpdateDrinkVisual(newIngredient);
        UpdateDrinkName(); // update after adding
    }

    private void UpdateDrinkName()
    {
        // Default to showing raw ingredients
        drinkName = string.Join(" + ", ingredients.ConvertAll(i => i.ingredientName));

        // Check if any recipe matches
        DrinkRecipe match = FindMatchingRecipe();
        if (match != null)
        {
            drinkName = match.recipeName;
            Debug.Log($"🍸 Drink matches recipe: {drinkName}");
        }
        else
        {
            Debug.Log($"❌ No recipe match. Current: {drinkName}");
        }
    }

    private void UpdateDrinkVisual(Ingredient ingredient)
    {
        if (cupRenderer != null)
            cupRenderer.material.color = ingredient.color;

        if (liquidLayers.Count > ingredients.Count - 1)
        {
            GameObject layer = liquidLayers[ingredients.Count - 1];
            if (layer != null)
            {
                layer.SetActive(true);
                Renderer r = layer.GetComponent<Renderer>();
                if (r != null)
                    r.material.color = ingredient.color;
            }
        }
    }

    private DrinkRecipe FindMatchingRecipe()
    {
        if (allRecipes == null || allRecipes.Length == 0)
            return null;

        foreach (DrinkRecipe recipe in allRecipes)
        {
            if (DoIngredientsMatch(recipe))
                return recipe;
        }

        return null;
    }

    private bool DoIngredientsMatch(DrinkRecipe recipe)
    {
        if (recipe == null) return false;
        if (recipe.requiredIngredients.Count != ingredients.Count)
            return false;

        for (int i = 0; i < recipe.requiredIngredients.Count; i++)
        {
            if (recipe.requiredIngredients[i] != ingredients[i])
                return false;
        }

        return true;
    }
}
