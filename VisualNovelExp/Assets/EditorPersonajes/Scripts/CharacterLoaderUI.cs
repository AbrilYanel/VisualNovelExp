
using UnityEngine;

public class CharacterLoaderUI : MonoBehaviour
{
    public CharacterDatabase database;
    public CharacterRendererUI characterRendererUI;
    public bool loadOnStart = true;

    void Start()
    {
        LoadCharacter();

        // RESEARCH: Log cuando aparece la pantalla de juego
        ResearchAnalytics.LogGameplayScreenOn();
    }

    public void LoadCharacter()
    {
        if (database == null || characterRendererUI == null) return;
        CharacterSaveData data = CharacterSaveSystem.Load();

        Try(data.skinIndex, database.skinColors, characterRendererUI.SetSkinColor);
        Try(data.hairIndex, database.hairColors, characterRendererUI.SetHairColor);
        Try(data.shoesIndex, database.shoes, characterRendererUI.SetShoes);

        if (data.dressIndex >= 0) Try(data.dressIndex, database.dresses, characterRendererUI.SetDress);
        else
        {
            if (data.shirtIndex >= 0) Try(data.shirtIndex, database.shirts, characterRendererUI.SetShirt);
            if (data.bottomIndex >= 0)
            {
                var src = data.bottomIsJeans ? database.jeans : database.skirts;
                Try(data.bottomIndex, src, characterRendererUI.SetBottom);
            }
        }
        if (data.accessoryIndex >= 0) Try(data.accessoryIndex, database.accessories, characterRendererUI.SetAccessory);
    }

    private void Try(int index, CosmeticItem[] arr, System.Action<CosmeticItem> apply)
    {
        if (arr == null || index < 0 || index >= arr.Length) return;
        var item = arr[index];
        if (item == null || item.sprite == null) return;
        apply(item);
    }
}
