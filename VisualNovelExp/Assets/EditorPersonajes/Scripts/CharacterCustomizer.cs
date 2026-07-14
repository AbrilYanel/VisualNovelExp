// CharacterCustomizer_ConAnalytics.cs - Tu CharacterCustomizer con los logs integrados
// Usa este para reemplazar tu CharacterCustomizer actual (o copia las líneas marcadas con // RESEARCH)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharacterCustomizer : MonoBehaviour
{
    public CharacterDatabase database;
    public CharacterRendererUI characterRenderer;
    public CategoryPanel categoryPanel;

    public string nextSceneName = "MainScene";

    private CharacterSaveData current = new CharacterSaveData();

    void Start()
    {
        // RESEARCH: Log apertura del editor
        ResearchAnalytics.LogEditorOpen();

        if (CharacterSaveSystem.HasSavedCharacter())
        {
            current = CharacterSaveSystem.Load();
            ApplySave(current);
        }
        else ResetCharacter();
        OpenSkinTab();
    }

    private void ApplySave(CharacterSaveData d)
    {
        if (IsValid(d.skinIndex, database.skinColors)) characterRenderer.SetSkinColor(database.skinColors[d.skinIndex]);
        if (IsValid(d.hairIndex, database.hairColors)) characterRenderer.SetHairColor(database.hairColors[d.hairIndex]);
        if (d.dressIndex >= 0) { if (IsValid(d.dressIndex, database.dresses)) characterRenderer.SetDress(database.dresses[d.dressIndex]); }
        else
        {
            if (IsValid(d.shirtIndex, database.shirts)) characterRenderer.SetShirt(database.shirts[d.shirtIndex]);
            if (IsValid(d.bottomIndex, d.bottomIsJeans ? database.jeans : database.skirts)) characterRenderer.SetBottom((d.bottomIsJeans ? database.jeans : database.skirts)[d.bottomIndex]);
        }
        if (IsValid(d.shoesIndex, database.shoes)) characterRenderer.SetShoes(database.shoes[d.shoesIndex]);
        if (d.accessoryIndex >= 0 && IsValid(d.accessoryIndex, database.accessories)) characterRenderer.SetAccessory(database.accessories[d.accessoryIndex]);
    }

    public void OpenSkinTab() => categoryPanel.Populate(database.skinColors, item => SelectSkin(System.Array.IndexOf(database.skinColors, item)));
    public void OpenHairTab() => categoryPanel.Populate(database.hairColors, item => SelectHair(System.Array.IndexOf(database.hairColors, item)));
    public void OpenShirtTab() => categoryPanel.Populate(database.shirts, item => SelectShirt(System.Array.IndexOf(database.shirts, item)));
    public void OpenDressTab() => categoryPanel.Populate(database.dresses, item => SelectDress(System.Array.IndexOf(database.dresses, item)));
    public void OpenShoesTab() => categoryPanel.Populate(database.shoes, item => SelectShoes(System.Array.IndexOf(database.shoes, item)));
    public void OpenAccessoryTab() => categoryPanel.Populate(database.accessories, item => SelectAccessory(System.Array.IndexOf(database.accessories, item)));
    public void OpenBottomTab()
    {
        List<CosmeticItem> combined = new List<CosmeticItem>();
        if (database.skirts != null) combined.AddRange(database.skirts.Where(x => x != null));
        if (database.jeans != null) combined.AddRange(database.jeans.Where(x => x != null));
        categoryPanel.Populate(combined.ToArray(), item =>
        {
            int sIdx = System.Array.IndexOf(database.skirts, item);
            int jIdx = System.Array.IndexOf(database.jeans, item);
            if (sIdx >= 0) SelectBottom(sIdx, true); else if (jIdx >= 0) SelectBottom(jIdx, false);
        });
    }

    // Helpers con RESEARCH
    private void SelectSkin(int i)
    {
        if (IsValid(i, database.skinColors))
        {
            current.skinIndex = i; characterRenderer.SetSkinColor(database.skinColors[i]);
            ResearchAnalytics.LogEditorInteraction("skin", database.skinColors[i].id); // RESEARCH
        }
    }
    private void SelectHair(int i)
    {
        if (IsValid(i, database.hairColors))
        {
            current.hairIndex = i; characterRenderer.SetHairColor(database.hairColors[i]);
            ResearchAnalytics.LogEditorInteraction("hair", database.hairColors[i].id);
        }
    }
    private void SelectShirt(int i)
    {
        if (IsValid(i, database.shirts))
        {
            current.shirtIndex = i; current.dressIndex = -1; characterRenderer.SetShirt(database.shirts[i]);
            ResearchAnalytics.LogEditorInteraction("shirt", database.shirts[i].id);
        }
    }
    private void SelectBottom(int i, bool isSkirt)
    {
        var src = isSkirt ? database.skirts : database.jeans;
        if (!IsValid(i, src)) return;
        current.bottomIndex = i; current.bottomIsJeans = !isSkirt; current.dressIndex = -1; characterRenderer.SetBottom(src[i]);
        ResearchAnalytics.LogEditorInteraction(isSkirt ? "skirt" : "jeans", src[i].id);
    }
    private void SelectDress(int i)
    {
        if (IsValid(i, database.dresses))
        {
            current.dressIndex = i; current.shirtIndex = -1; current.bottomIndex = -1; characterRenderer.SetDress(database.dresses[i]);
            ResearchAnalytics.LogEditorInteraction("dress", database.dresses[i].id);
        }
    }
    private void SelectShoes(int i)
    {
        if (IsValid(i, database.shoes))
        {
            current.shoesIndex = i; characterRenderer.SetShoes(database.shoes[i]);
            ResearchAnalytics.LogEditorInteraction("shoes", database.shoes[i].id);
        }
    }
    private void SelectAccessory(int i)
    {
        if (IsValid(i, database.accessories))
        {
            current.accessoryIndex = i; characterRenderer.SetAccessory(database.accessories[i]);
            ResearchAnalytics.LogEditorInteraction("accessory", database.accessories[i].id);
        }
    }

    private bool IsValid(int index, CosmeticItem[] arr) => arr != null && index >= 0 && index < arr.Length && arr[index] != null && arr[index].sprite != null;

    public void ResetCharacter()
    {
        current = new CharacterSaveData();
        if (database.skinColors.Length > 0) SelectSkin(0);
        if (database.hairColors.Length > 0) SelectHair(0);
        if (database.shirts.Length > 0) { SelectShirt(0); if (database.skirts.Length > 0) SelectBottom(0, true); else if (database.jeans.Length > 0) SelectBottom(0, false); }
        else if (database.dresses.Length > 0) SelectDress(0);
        if (database.shoes.Length > 0) SelectShoes(0);

        ResearchAnalytics.LogEditorInteraction("reset", "reset_character");
    }

    public void ConfirmAndSave()
    {
        // RESEARCH: Log cierre del editor
        ResearchAnalytics.LogEditorClose();

        CharacterSaveSystem.Save(current);
        Debug.Log($"Guardado: {current}");

        if (!string.IsNullOrEmpty(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    public CharacterSaveData GetCurrentData() => current;
}
