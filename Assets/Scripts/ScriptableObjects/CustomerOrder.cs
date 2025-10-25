using UnityEngine;
using System.Linq;

public class CustomerOrder : MonoBehaviour
{
    public DrinkRecipe requestedRecipe;

    public bool CheckDrink(DrinkTracker cup)
    {
        if (requestedRecipe == null || cup == null)
            return false;

        if (cup.ingredients.Count != requestedRecipe.requiredIngredients.Count)
            return false;

        for (int i = 0; i < requestedRecipe.requiredIngredients.Count; i++)
        {
            if (cup.ingredients[i] != requestedRecipe.requiredIngredients[i])
                return false;
        }

        Debug.Log($"Correct drink! Served {requestedRecipe.recipeName}");
        return true;
    }
}
