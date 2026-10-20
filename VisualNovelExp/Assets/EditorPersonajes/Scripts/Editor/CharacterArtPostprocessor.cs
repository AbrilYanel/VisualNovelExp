using UnityEditor;
using UnityEngine;

public class CharacterArtPostprocessor : AssetPostprocessor
{
    private const string ART_ROOT = "Assets/_CharacterEditor/Art/";

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ART_ROOT)) return;

        var importer = (TextureImporter)assetImporter;

        bool isIcon = assetPath.Contains("/Icons/");

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.maxTextureSize = isIcon ? 256 : 2048;

        // Full Rect = Unity NO recorta el espacio transparente del PNG,
        // asi todas las piezas (mismo tamano de lienzo) coinciden al apilarse.
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings);
    }
}