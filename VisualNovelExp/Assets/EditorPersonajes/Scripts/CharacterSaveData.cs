// CharacterSaveData.cs - PASO 4
// Pon en: Assets/_CharacterEditor/Scripts/Data/CharacterSaveData.cs
// Esta es la versión FINAL que reemplaza la temporal que tenías dentro del Customizer

[System.Serializable]
public class CharacterSaveData
{
    public int skinIndex = 0;
    public int hairIndex = 0;
    public int shirtIndex = 0;
    public int bottomIndex = 0;
    public int dressIndex = -1;      // -1 = ninguno, usa shirt+bottom
    public int shoesIndex = 0;
    public int accessoryIndex = -1;  // -1 = ninguno (opcional)

    // Flags extras para diferenciar tipos
    public bool bottomIsJeans = false; // false = falda, true = jean

    // Para debug
    public override string ToString()
    {
        return $"Skin:{skinIndex} Hair:{hairIndex} Shirt:{shirtIndex} Bottom:{bottomIndex}({(bottomIsJeans ? "Jean" : "Skirt")}) Dress:{dressIndex} Shoes:{shoesIndex} Acc:{accessoryIndex}";
    }
}
