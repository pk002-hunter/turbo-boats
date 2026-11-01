using System.IO;
using TurboBoats;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TurboBoats.Editor
{
    [InitializeOnLoad]
    internal static class BootstrapScene
    {
        private const string ScenePath = "Assets/Scenes/CanalRun.unity";

        static BootstrapScene()
        {
            EditorApplication.delayCall += CreateSceneIfNeeded;
        }

        [MenuItem("Turbo Boats/Create Graybox Scene")]
        private static void CreateSceneIfNeeded()
        {
            if (File.Exists(ScenePath))
                return;

            Directory.CreateDirectory("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material hullMaterial = CreateMaterial("Longtail Hull", new Color(0.78f, 0.12f, 0.08f));
            Material trimMaterial = CreateMaterial("Longtail Trim", new Color(1f, 0.68f, 0.12f));
            Material shaftMaterial = CreateMaterial("Engine and Shaft", new Color(0.12f, 0.12f, 0.13f));

            GameObject boat = new GameObject("Thai Longtail Boat");
            boat.transform.position = new Vector3(0f, 0.15f, 0f);
            Rigidbody body = boat.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            BoxCollider boatCollider = boat.AddComponent<BoxCollider>();
            boatCollider.size = new Vector3(1.8f, 0.65f, 4.2f);
            boat.AddComponent<BoatRunnerController>();

            CreateBoatPart(boat.transform, "Narrow wooden hull", PrimitiveType.Capsule, new Vector3(0f, 0f, 0f), new Vector3(1f, 0.34f, 2.25f), Quaternion.Euler(90f, 0f, 0f), hullMaterial);
            CreateBoatPart(boat.transform, "Raised bow", PrimitiveType.Cube, new Vector3(0f, 0.16f, 1.45f), new Vector3(0.62f, 0.3f, 0.5f), Quaternion.identity, trimMaterial);
            CreateBoatPart(boat.transform, "Rear engine", PrimitiveType.Cube, new Vector3(0f, 0.35f, -1.2f), new Vector3(0.62f, 0.56f, 0.72f), Quaternion.identity, shaftMaterial);
            CreateBoatPart(boat.transform, "Long propeller shaft", PrimitiveType.Cylinder, new Vector3(0f, 0.13f, -2.05f), new Vector3(0.07f, 0.95f, 0.07f), Quaternion.Euler(90f, 0f, 0f), shaftMaterial);
            CreateBoatPart(boat.transform, "Propeller", PrimitiveType.Cylinder, new Vector3(0f, 0.13f, -3.05f), new Vector3(0.38f, 0.08f, 0.12f), Quaternion.identity, trimMaterial);

            GameObject river = new GameObject("Endless River");
            EndlessRiverWorld world = river.AddComponent<EndlessRiverWorld>();
            world.Boat = boat.transform;

            GameObject manager = new GameObject("Game Manager");
            GameManager gameManager = manager.AddComponent<GameManager>();
            gameManager.Boat = boat.GetComponent<BoatRunnerController>();
            gameManager.River = world;

            GameObject cameraObject = new GameObject("Chase Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 65f;
            cameraObject.tag = "MainCamera";
            CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
            follow.Target = boat.transform;
            cameraObject.transform.position = new Vector3(0f, 5.5f, -10f);
            cameraObject.transform.LookAt(boat.transform.position + Vector3.forward * 7.5f);

            GameObject lightObject = new GameObject("Sunlight");
            Light sunlight = lightObject.AddComponent<Light>();
            sunlight.type = LightType.Directional;
            sunlight.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Turbo Boats graybox scene created at " + ScenePath);
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Directory.CreateDirectory("Assets/Art/Materials");
            string path = "Assets/Art/Materials/" + name.Replace(" ", "") + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            Material material = new Material(shader) { name = name, color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void CreateBoatPart(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.transform.localRotation = localRotation;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
        }
    }
}
