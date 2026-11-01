using UnityEditor;
using UnityEngine;

namespace TurboBoats.Editor
{
    /// <summary>Sets repeat, mip, color-space, and normal-map import settings for the art maps.</summary>
    [InitializeOnLoad]
    internal static class StylizedTextureImporter
    {
        private const string TextureFolder = "Assets/Resources/TurboBoats/Textures";
        private const string MaterialFolder = "Assets/Resources/TurboBoats/Materials";
        private const string CelMaterialPath = MaterialFolder + "/StylizedCel.mat";

        static StylizedTextureImporter()
        {
            EditorApplication.delayCall += ConfigureImports;
        }

        [MenuItem("Turbo Boats/Art/Configure Stylized Texture Imports")]
        private static void ConfigureImports()
        {
            if (!AssetDatabase.IsValidFolder(TextureFolder))
                return;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TextureFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                    continue;

                string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                bool isNormal = filename.EndsWith("_normal");
                bool isSurface = filename.EndsWith("_surface");
                TextureImporterType type = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                bool srgb = !isNormal && !isSurface;
                bool changed = importer.textureType != type
                    || importer.sRGBTexture != srgb
                    || importer.wrapMode != TextureWrapMode.Repeat
                    || !importer.mipmapEnabled
                    || importer.filterMode != FilterMode.Trilinear
                    || (isSurface && importer.alphaSource != TextureImporterAlphaSource.FromInput);
                if (!changed)
                    continue;

                importer.textureType = type;
                importer.sRGBTexture = srgb;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.mipmapEnabled = true;
                importer.filterMode = FilterMode.Trilinear;
                importer.anisoLevel = 2;
                if (isSurface)
                    importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.SaveAndReimport();
            }

            EnsureCelMaterial();
        }

        private static void EnsureCelMaterial()
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(CelMaterialPath) != null)
                return;

            Shader shader = Shader.Find("TurboBoats/StylizedCel");
            if (shader == null)
                return;

            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder("Assets/Resources/TurboBoats", "Materials");

            Material material = new Material(shader) { name = "StylizedCel" };
            AssetDatabase.CreateAsset(material, CelMaterialPath);
            AssetDatabase.SaveAssets();
        }
    }
}
