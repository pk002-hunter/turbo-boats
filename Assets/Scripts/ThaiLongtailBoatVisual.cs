using System.Collections.Generic;
using UnityEngine;

namespace TurboBoats
{
    /// <summary>
    /// Builds the player's art from small, untextured meshes so the prototype does not
    /// depend on imported or concept-image assets. It has no colliders and no gameplay logic.
    /// </summary>
    public sealed class ThaiLongtailBoatVisual : MonoBehaviour
    {
        private readonly List<Mesh> generatedMeshes = new List<Mesh>();
        private readonly List<Material> generatedMaterials = new List<Material>();

        private static readonly Color HullColor = new Color(0.34f, 0.12f, 0.055f);
        private static readonly Color DeckColor = new Color(0.48f, 0.22f, 0.085f);
        private static readonly Color GoldColor = new Color(0.94f, 0.54f, 0.12f);
        private static readonly Color TealColor = new Color(0.08f, 0.52f, 0.50f);
        private static readonly Color CreamColor = new Color(0.96f, 0.82f, 0.55f);
        private static readonly Color EngineColor = new Color(0.12f, 0.16f, 0.17f);
        private static readonly Color MetalColor = new Color(0.46f, 0.53f, 0.51f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AttachToSceneBoats()
        {
            BoatRunnerController[] boats = Object.FindObjectsOfType<BoatRunnerController>();
            for (int i = 0; i < boats.Length; i++)
            {
                if (boats[i] != null && boats[i].GetComponent<ThaiLongtailBoatVisual>() == null)
                    boats[i].gameObject.AddComponent<ThaiLongtailBoatVisual>();
            }
        }

        private void Awake()
        {
            RemoveGrayboxParts();
            BuildBoat();
        }

        private void RemoveGrayboxParts()
        {
            string[] oldParts = { "Narrow wooden hull", "Raised bow", "Rear engine", "Long propeller shaft", "Propeller" };
            for (int i = 0; i < oldParts.Length; i++)
            {
                Transform oldPart = transform.Find(oldParts[i]);
                if (oldPart != null)
                {
                    oldPart.gameObject.SetActive(false);
                    Destroy(oldPart.gameObject);
                }
            }
        }

        private void BuildBoat()
        {
            Material hull = MakeMaterial("Longtail Teak Hull", HullColor, 0.22f);
            Material deck = MakeMaterial("Longtail Warm Deck", DeckColor, 0.18f);
            Material gold = MakeMaterial("Longtail Saffron Paint", GoldColor, 0.24f);
            Material teal = MakeMaterial("Longtail Lagoon Paint", TealColor, 0.22f);
            Material cream = MakeMaterial("Longtail Silk Paint", CreamColor, 0.25f);
            Material engine = MakeMaterial("Longtail Engine Graphite", EngineColor, 0.48f);
            Material metal = MakeMaterial("Longtail Engine Steel", MetalColor, 0.72f);

            BuildHull(hull, gold, teal);

            CreateBlock("Inside floor", new Vector3(0f, -0.025f, -0.12f), new Vector3(0.68f, 0.12f, 3.15f), deck);
            CreateBlock("Bow deck", new Vector3(0f, 0.18f, 1.48f), new Vector3(0.48f, 0.08f, 0.62f), deck);
            CreateBlock("Cross bench", new Vector3(0f, 0.22f, 0.25f), new Vector3(0.92f, 0.13f, 0.32f), deck);
            CreateBlock("Aft bench", new Vector3(0f, 0.22f, -1.14f), new Vector3(0.82f, 0.12f, 0.26f), deck);

            CreateBlock("Rear motor block", new Vector3(0f, 0.43f, -1.56f), new Vector3(0.45f, 0.46f, 0.62f), engine);
            CreateCylinder("Motor flywheel", new Vector3(0f, 0.48f, -1.56f), new Vector3(0.22f, 0.1f, 0.22f), Quaternion.Euler(90f, 0f, 0f), metal);
            CreateBlock("Motor fuel tank", new Vector3(0f, 0.76f, -1.62f), new Vector3(0.3f, 0.22f, 0.38f), gold);
            CreateCylinder("Long-tail drive shaft", new Vector3(0f, -0.015f, -2.92f), new Vector3(0.055f, 1.08f, 0.055f), Quaternion.Euler(88f, 0f, 0f), metal);
            CreateCylinder("Shaft coupling", new Vector3(0f, 0.02f, -2.06f), new Vector3(0.12f, 0.12f, 0.12f), Quaternion.Euler(90f, 0f, 0f), engine);
            CreateCylinder("Propeller hub", new Vector3(0f, -0.15f, -3.8f), new Vector3(0.11f, 0.1f, 0.11f), Quaternion.Euler(90f, 0f, 0f), metal);
            CreateBlock("Propeller blade one", new Vector3(0f, -0.15f, -3.84f), new Vector3(0.52f, 0.045f, 0.09f), gold);
            CreateBlock("Propeller blade two", new Vector3(0f, -0.15f, -3.84f), new Vector3(0.09f, 0.045f, 0.52f), gold);

            CreateBlock("Port gunwale", new Vector3(-0.62f, 0.2f, -0.1f), new Vector3(0.07f, 0.075f, 2.75f), deck);
            CreateBlock("Starboard gunwale", new Vector3(0.62f, 0.2f, -0.1f), new Vector3(0.07f, 0.075f, 2.75f), deck);
            CreateBlock("Bow ornament", new Vector3(0f, 0.52f, 1.92f), new Vector3(0.075f, 0.12f, 0.32f), gold);
            CreateSphere("Bow silk knot", new Vector3(0f, 0.64f, 1.95f), new Vector3(0.13f, 0.12f, 0.13f), teal);
            CreateBlock("Port silk streamer", new Vector3(-0.16f, 0.53f, 2.02f), new Vector3(0.05f, 0.2f, 0.12f), cream, Quaternion.Euler(0f, 0f, -25f));
            CreateBlock("Starboard silk streamer", new Vector3(0.16f, 0.53f, 2.02f), new Vector3(0.05f, 0.2f, 0.12f), teal, Quaternion.Euler(0f, 0f, 25f));
        }

        private void OnDestroy()
        {
            for (int i = 0; i < generatedMeshes.Count; i++)
            {
                if (generatedMeshes[i] != null)
                    Destroy(generatedMeshes[i]);
            }
            for (int i = 0; i < generatedMaterials.Count; i++)
            {
                if (generatedMaterials[i] != null)
                    Destroy(generatedMaterials[i]);
            }
        }

        private void BuildHull(Material hull, Material gold, Material teal)
        {
            HullStation[] stations =
            {
                new HullStation(-2.22f, 0.38f, 0.3f, 0.16f, -0.17f),
                new HullStation(-1.65f, 0.67f, 0.48f, 0.22f, -0.34f),
                new HullStation(-0.75f, 0.8f, 0.55f, 0.27f, -0.4f),
                new HullStation(0.35f, 0.78f, 0.51f, 0.3f, -0.36f),
                new HullStation(1.3f, 0.53f, 0.28f, 0.37f, -0.22f),
                new HullStation(2.0f, 0.04f, 0.015f, 0.68f, 0.02f)
            };

            MeshBuilder shell = new MeshBuilder();
            MeshBuilder goldStripe = new MeshBuilder();
            MeshBuilder tealStripe = new MeshBuilder();
            for (int i = 0; i < stations.Length - 1; i++)
            {
                HullStation back = stations[i];
                HullStation front = stations[i + 1];
                Vector3 backLeftTop = new Vector3(-back.topWidth, back.topY, back.z);
                Vector3 backRightTop = new Vector3(back.topWidth, back.topY, back.z);
                Vector3 frontLeftTop = new Vector3(-front.topWidth, front.topY, front.z);
                Vector3 frontRightTop = new Vector3(front.topWidth, front.topY, front.z);
                Vector3 backLeftKeel = new Vector3(-back.keelWidth, back.keelY, back.z);
                Vector3 backRightKeel = new Vector3(back.keelWidth, back.keelY, back.z);
                Vector3 frontLeftKeel = new Vector3(-front.keelWidth, front.keelY, front.z);
                Vector3 frontRightKeel = new Vector3(front.keelWidth, front.keelY, front.z);

                shell.AddQuad(backRightTop, frontRightTop, frontRightKeel, backRightKeel, Vector3.right);
                shell.AddQuad(backLeftTop, frontLeftTop, frontLeftKeel, backLeftKeel, Vector3.left);
                shell.AddQuad(backLeftKeel, frontLeftKeel, frontRightKeel, backRightKeel, Vector3.down);

                AddStripe(goldStripe, back, front, 0.24f, 0.025f);
                AddStripe(tealStripe, back, front, 0.39f, 0.018f);
            }

            HullStation stern = stations[0];
            shell.AddQuad(
                new Vector3(-stern.topWidth, stern.topY, stern.z),
                new Vector3(stern.topWidth, stern.topY, stern.z),
                new Vector3(stern.keelWidth, stern.keelY, stern.z),
                new Vector3(-stern.keelWidth, stern.keelY, stern.z), Vector3.back);
            HullStation bow = stations[stations.Length - 1];
            shell.AddTriangle(
                new Vector3(-bow.topWidth, bow.topY, bow.z),
                new Vector3(bow.topWidth, bow.topY, bow.z),
                new Vector3(0f, bow.keelY, bow.z), Vector3.forward);

            CreateMesh("Hand-shaped longtail hull", shell.Build(), hull);
            CreateMesh("Saffron hull pinstripe", goldStripe.Build(), gold);
            CreateMesh("Lagoon hull pinstripe", tealStripe.Build(), teal);
        }

        private static void AddStripe(MeshBuilder builder, HullStation back, HullStation front, float fraction, float thickness)
        {
            float backX = Mathf.Lerp(back.topWidth, back.keelWidth, fraction) + thickness;
            float frontX = Mathf.Lerp(front.topWidth, front.keelWidth, fraction) + thickness;
            float backY = Mathf.Lerp(back.topY, back.keelY, fraction);
            float frontY = Mathf.Lerp(front.topY, front.keelY, fraction);

            builder.AddQuad(
                new Vector3(backX, backY, back.z), new Vector3(frontX, frontY, front.z),
                new Vector3(frontX, frontY - thickness, front.z), new Vector3(backX, backY - thickness, back.z), Vector3.right);
            builder.AddQuad(
                new Vector3(-backX, backY, back.z), new Vector3(-frontX, frontY, front.z),
                new Vector3(-frontX, frontY - thickness, front.z), new Vector3(-backX, backY - thickness, back.z), Vector3.left);
        }

        private void CreateMesh(string objectName, Mesh mesh, Material material)
        {
            GameObject visual = new GameObject(objectName);
            visual.transform.SetParent(transform, false);
            visual.AddComponent<MeshFilter>().sharedMesh = mesh;
            visual.AddComponent<MeshRenderer>().sharedMaterial = material;
            generatedMeshes.Add(mesh);
        }

        private void CreateBlock(string objectName, Vector3 position, Vector3 scale, Material material, Quaternion rotation = default(Quaternion))
        {
            CreatePrimitive(objectName, PrimitiveType.Cube, position, scale, rotation == default(Quaternion) ? Quaternion.identity : rotation, material);
        }

        private void CreateCylinder(string objectName, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            CreatePrimitive(objectName, PrimitiveType.Cylinder, position, scale, rotation, material);
        }

        private void CreateSphere(string objectName, Vector3 position, Vector3 scale, Material material)
        {
            CreatePrimitive(objectName, PrimitiveType.Sphere, position, scale, Quaternion.identity, material);
        }

        private void CreatePrimitive(string objectName, PrimitiveType type, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(type);
            visual.name = objectName;
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = position;
            visual.transform.localScale = scale;
            visual.transform.localRotation = rotation;
            visual.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
        }

        private Material MakeMaterial(string name, Color color, float smoothness)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            Material material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            generatedMaterials.Add(material);
            return material;
        }

        private struct HullStation
        {
            public readonly float z;
            public readonly float topWidth;
            public readonly float keelWidth;
            public readonly float topY;
            public readonly float keelY;

            public HullStation(float z, float topWidth, float keelWidth, float topY, float keelY)
            {
                this.z = z;
                this.topWidth = topWidth;
                this.keelWidth = keelWidth;
                this.topY = topY;
                this.keelY = keelY;
            }
        }

        private sealed class MeshBuilder
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<int> triangles = new List<int>();
            private readonly List<Vector3> normals = new List<Vector3>();

            public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normalHint)
            {
                Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
                if (normalHint != Vector3.zero && Vector3.Dot(normal, normalHint) < 0f)
                {
                    Vector3 swap = b;
                    b = c;
                    c = swap;
                    normal = -normal;
                }

                int first = vertices.Count;
                vertices.Add(a);
                vertices.Add(b);
                vertices.Add(c);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);
                triangles.Add(first);
                triangles.Add(first + 1);
                triangles.Add(first + 2);
            }

            public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normalHint)
            {
                AddTriangle(a, b, c, normalHint);
                AddTriangle(a, c, d, normalHint);
            }

            public Mesh Build()
            {
                Mesh mesh = new Mesh();
                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.normals = normals.ToArray();
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
