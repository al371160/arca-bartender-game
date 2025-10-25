using UnityEngine;

[CreateAssetMenu(menuName = "DrinkSystem/Ingredient", fileName = "NewIngredient")]
public class Ingredient : ScriptableObject
{
    public string ingredientName;
    public Color color = Color.white;
    public Sprite icon;

    [TextArea] public string description;
}
