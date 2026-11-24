using UnityEditor;
using UnityEngine;

/// <summary>
/// Pixel-art friendly import settings for third-party art in Assets/ThirdParty/Resources/Art
/// (point filter, no compression, no mipmaps), so sprites stay crisp on every teammate's machine.
/// </summary>
public class ThirdPartyArtImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.Replace('\\', '/').Contains("/ThirdParty/Resources/Art/")) return;
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
    }
}
