using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "DrinkSystem/DrinkRecipe", fileName = "NewRecipe")]
public class DrinkRecipe : ScriptableObject
{
    public string recipeName;
    public List<Ingredient> requiredIngredients = new List<Ingredient>();
}
