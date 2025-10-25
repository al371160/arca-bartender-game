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

        // Optional: instantiate layered liquids
        if (liquidParent != null)
        {
            GameObject layer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            layer.transform.SetParent(liquidParent, false);
            layer.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f);
            layer.transform.localPosition = new Vector3(0, ingredients.Count * 0.1f, 0);
            layer.GetComponent<Renderer>().material.color = ingredient.color;
        }
    }
}
