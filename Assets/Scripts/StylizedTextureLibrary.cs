using System.Collections.Generic;
using UnityEngine;

namespace TurboBoats
{
    /// <summary>Loads small shared texture maps for the procedural runtime materials.</summary>
    internal static class StylizedTextureLibrary
    {
        private const string ResourceRoot = "TurboBoats/Textures/";
        private const string MaterialResource = "TurboBoats/Materials/StylizedCel";
        private const string CelShaderName = "TurboBoats/StylizedCel";
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
        private static Material template;

        public static Material CreateMaterial(string name, Color color, float smoothness = 0.25f)
        {
            if (template == null)
                template = Resources.Load<Material>(MaterialResource);

            Shader shader = template != null ? template.shader : Shader.Find(CelShaderName);
            if (shader == null)
                shader = Shader.Find("Standard");

            Material material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            return material;
        }

        public static void Apply(Material material, string albedoName, string normalName, string surfaceName, Vector2 tiling, float normalStrength, bool preserveTint = false)
        {
            if (material == null)
                return;

            Texture2D albedo = Load(albedoName);
            if (albedo != null)
            {
                if (!preserveTint)
                    material.color = Color.white;
                SetTexture(material, "_BaseMap", albedo);
                SetTexture(material, "_MainTex", albedo);
                SetTiling(material, "_BaseMap", tiling);
                SetTiling(material, "_MainTex", tiling);
            }

            Texture2D normal = Load(normalName);
            if (normal != null)
            {
                SetTexture(material, "_BumpMap", normal);
                SetTiling(material, "_BumpMap", tiling);
                material.EnableKeyword("_NORMALMAP");
                if (material.HasProperty("_BumpScale"))
                    material.SetFloat("_BumpScale", normalStrength);
            }

            Texture2D surface = Load(surfaceName);
            if (surface != null)
            {
                SetTexture(material, "_MetallicGlossMap", surface);
                SetTiling(material, "_MetallicGlossMap", tiling);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.EnableKeyword("_METALLICGLOSSMAP");
                if (material.HasProperty("_GlossMapScale"))
                    material.SetFloat("_GlossMapScale", 1f);
            }
        }

        private static Texture2D Load(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            if (!Cache.TryGetValue(name, out Texture2D texture))
            {
                texture = Resources.Load<Texture2D>(ResourceRoot + name);
                Cache.Add(name, texture);
            }
            return texture;
        }

        private static void SetTexture(Material material, string property, Texture texture)
        {
            if (material.HasProperty(property))
                material.SetTexture(property, texture);
        }

        private static void SetTiling(Material material, string property, Vector2 tiling)
        {
            if (material.HasProperty(property))
                material.SetTextureScale(property, tiling);
        }
    }
}
