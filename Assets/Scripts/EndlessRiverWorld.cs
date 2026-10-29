using System.Collections.Generic;
using UnityEngine;

namespace TurboBoats
{
    public sealed class EndlessRiverWorld : MonoBehaviour
    {
        private const float SectionLength = 40f;
        private const float LaneWidth = 3.2f;
        private const float SpawnAhead = 170f;

        [SerializeField] private Transform boat;

        private readonly List<GameObject> sections = new List<GameObject>();
        private readonly string[] zoneNames = { "Bangkok Canal", "Floating Market", "Rice Field Waterway", "Forest River" };
        private int nextSectionIndex;
        private Material[] waterMaterials;
        private Material bankMaterial;
        private Material markerMaterial;
        private Material foliageMaterial;
        private Material roofMaterial;
        private Material obstacleMaterial;
        private Material tokenMaterial;
        private Material healthMaterial;
        private Material healthMarkMaterial;

        public Transform Boat
        {
            get => boat;
            set => boat = value;
        }

        public string CurrentZoneName
        {
            get
            {
                int currentSection = Mathf.Max(0, Mathf.FloorToInt((boat == null ? 0f : boat.position.z) / SectionLength));
                return zoneNames[(currentSection / 4) % zoneNames.Length];
            }
        }

        private void Awake()
        {
            waterMaterials = new[]
            {
                MakeMaterial("Bangkok Canal Water", new Color(0.06f, 0.36f, 0.39f)),
                MakeMaterial("Market Canal Water", new Color(0.05f, 0.38f, 0.39f)),
                MakeMaterial("Rice Field Water", new Color(0.07f, 0.40f, 0.37f)),
                MakeMaterial("Forest River Water", new Color(0.04f, 0.28f, 0.29f))
            };
            bankMaterial = MakeMaterial("Riverbank", new Color(0.39f, 0.48f, 0.25f));
            markerMaterial = MakeMaterial("Channel Marker", new Color(0.76f, 0.76f, 0.59f));
            foliageMaterial = MakeMaterial("Foliage", new Color(0.16f, 0.35f, 0.13f));
            roofMaterial = MakeMaterial("Market Roof", new Color(0.82f, 0.31f, 0.12f));
            obstacleMaterial = MakeMaterial("Floating Hazard", new Color(0.34f, 0.19f, 0.10f));
            tokenMaterial = MakeMaterial("River Token", new Color(1f, 0.72f, 0.05f));
            healthMaterial = MakeMaterial("Health Booster Green", new Color(0.12f, 0.82f, 0.32f));
            healthMarkMaterial = MakeMaterial("Health Booster Mark", Color.white);
        }

        private void Start()
        {
            ResetRiver();
        }

        private void Update()
        {
            if (boat == null)
                return;

            while (nextSectionIndex * SectionLength < boat.position.z + SpawnAhead)
                SpawnSection(nextSectionIndex++);

            float removeBefore = boat.position.z - SectionLength * 2f;
            for (int i = sections.Count - 1; i >= 0; i--)
            {
                if (sections[i] != null && sections[i].transform.position.z + SectionLength < removeBefore)
                {
                    Destroy(sections[i]);
                    sections.RemoveAt(i);
                }
            }
        }

        public void ResetRiver()
        {
            for (int i = 0; i < sections.Count; i++)
            {
                if (sections[i] != null)
                    Destroy(sections[i]);
            }
            sections.Clear();
            nextSectionIndex = 0;
            while (nextSectionIndex * SectionLength < (boat == null ? SpawnAhead : boat.position.z + SpawnAhead))
                SpawnSection(nextSectionIndex++);
        }

        private void SpawnSection(int index)
        {
            int zoneIndex = (index / 4) % zoneNames.Length;
            float startZ = index * SectionLength;
            GameObject root = new GameObject($"River Section {index + 1} - {zoneNames[zoneIndex]}");
            root.transform.SetParent(transform, false);
            root.transform.position = new Vector3(0f, 0f, startZ);
            sections.Add(root);

            CreateBlock(root.transform, "Water", new Vector3(0f, -0.42f, SectionLength * 0.5f), new Vector3(18f, 0.2f, SectionLength), waterMaterials[zoneIndex]);
            CreateBlock(root.transform, "Left bank", new Vector3(-14f, -0.05f, SectionLength * 0.5f), new Vector3(10f, 0.8f, SectionLength), bankMaterial);
            CreateBlock(root.transform, "Right bank", new Vector3(14f, -0.05f, SectionLength * 0.5f), new Vector3(10f, 0.8f, SectionLength), bankMaterial);

            for (int laneLine = -1; laneLine <= 1; laneLine += 2)
            {
                for (int dash = 0; dash < 5; dash++)
                {
                    float z = 3f + dash * 8f;
                    CreateBlock(root.transform, "Channel guide", new Vector3(laneLine * LaneWidth * 0.5f, -0.305f, z), new Vector3(0.07f, 0.025f, 3.5f), markerMaterial);
                }
            }

            AddBankDecoration(root.transform, zoneIndex, startZ);
            if (index > 0)
                AddHazardsAndTokens(root.transform, index);
        }

        private void AddBankDecoration(Transform parent, int zoneIndex, float startZ)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * Random.Range(10.5f, 13.5f);
                float z = Random.Range(5f, SectionLength - 5f);
                if (zoneIndex == 1)
                {
                    CreateBlock(parent, "Market stall", new Vector3(x, 1.1f, z), new Vector3(2.8f, 2.2f, 3f), bankMaterial);
                    CreateBlock(parent, "Bright market canopy", new Vector3(x, 2.35f, z), new Vector3(3.4f, 0.3f, 3.6f), roofMaterial);
                }
                else if (zoneIndex == 2)
                {
                    CreateBlock(parent, "Rice field hut", new Vector3(x, 0.8f, z), new Vector3(2.6f, 1.6f, 2.6f), roofMaterial);
                    CreateBlock(parent, "Paddy field", new Vector3(side * 19f, -0.18f, z), new Vector3(8f, 0.15f, 11f), foliageMaterial);
                }
                else
                {
                    CreateBlock(parent, zoneIndex == 3 ? "Jungle tree trunk" : "Canal building", new Vector3(x, zoneIndex == 3 ? 1.6f : 1.2f, z), new Vector3(2.4f, zoneIndex == 3 ? 3.2f : 2.4f, 2.4f), zoneIndex == 3 ? foliageMaterial : roofMaterial);
                    if (zoneIndex == 3)
                        CreateBlock(parent, "Tree crown", new Vector3(x, 3.3f, z), new Vector3(4.2f, 2.8f, 4.2f), foliageMaterial);
                }
            }
        }

        private void AddHazardsAndTokens(Transform parent, int index)
        {
            int rowCount = Random.Range(1, 3);
            for (int row = 0; row < rowCount; row++)
            {
                float localZ = 9f + row * 17f + Random.Range(0f, 5f);
                int blockedLane = Random.Range(0, 3);
                CreateObstacle(parent.transform, blockedLane, localZ, index + row);

                if (Random.value < 0.55f)
                {
                    int clearLane = (blockedLane + Random.Range(1, 3)) % 3;
                    CreateToken(parent.transform, clearLane, localZ - 2f);
                }
            }

            if (Random.value < 0.38f)
            {
                int pickupLane = Random.Range(0, 3);
                float pickupZ = 27f + Random.Range(0f, 8f);
                CreateHealthBooster(parent, pickupLane, pickupZ);
            }
        }

        private void CreateHealthBooster(Transform parent, int lane, float localZ)
        {
            GameObject pickup = new GameObject("Health Booster");
            pickup.transform.SetParent(parent, false);
            pickup.transform.localPosition = new Vector3((lane - 1) * LaneWidth, 0.85f, localZ);
            SphereCollider trigger = pickup.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.8f;
            pickup.AddComponent<HealthBooster>();

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = "Green first-aid pack";
            core.transform.SetParent(pickup.transform, false);
            core.transform.localScale = new Vector3(0.9f, 0.7f, 0.42f);
            core.GetComponent<Renderer>().sharedMaterial = healthMaterial;
            Destroy(core.GetComponent<Collider>());

            GameObject verticalMark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            verticalMark.name = "White plus vertical";
            verticalMark.transform.SetParent(pickup.transform, false);
            verticalMark.transform.localPosition = new Vector3(0f, 0f, -0.23f);
            verticalMark.transform.localScale = new Vector3(0.16f, 0.48f, 0.08f);
            verticalMark.GetComponent<Renderer>().sharedMaterial = healthMarkMaterial;
            Destroy(verticalMark.GetComponent<Collider>());

            GameObject horizontalMark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            horizontalMark.name = "White plus horizontal";
            horizontalMark.transform.SetParent(pickup.transform, false);
            horizontalMark.transform.localPosition = new Vector3(0f, 0f, -0.23f);
            horizontalMark.transform.localScale = new Vector3(0.48f, 0.16f, 0.08f);
            horizontalMark.GetComponent<Renderer>().sharedMaterial = healthMarkMaterial;
            Destroy(horizontalMark.GetComponent<Collider>());
        }

        private void CreateObstacle(Transform parent, int lane, float localZ, int variety)
        {
            float x = (lane - 1) * LaneWidth;
            GameObject obstacle = new GameObject("Floating log");
            obstacle.transform.SetParent(parent, false);
            obstacle.transform.localPosition = new Vector3(x, 0f, localZ);
            BoxCollider trigger = obstacle.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2.2f, 0.8f, 1.7f);
            obstacle.AddComponent<BoatObstacle>();

            GameObject visual = GameObject.CreatePrimitive(variety % 2 == 0 ? PrimitiveType.Cylinder : PrimitiveType.Cube);
            visual.name = variety % 2 == 0 ? "Driftwood" : "Floating debris";
            visual.transform.SetParent(obstacle.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            visual.transform.localRotation = variety % 2 == 0 ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.identity;
            visual.transform.localScale = variety % 2 == 0 ? new Vector3(0.55f, 2.4f, 0.55f) : new Vector3(1.7f, 0.45f, 1.5f);
            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
                Destroy(visualCollider);
            visual.GetComponent<Renderer>().sharedMaterial = obstacleMaterial;
        }

        private void CreateToken(Transform parent, int lane, float localZ)
        {
            GameObject token = new GameObject("River token");
            token.transform.SetParent(parent, false);
            token.transform.localPosition = new Vector3((lane - 1) * LaneWidth, 0.65f, localZ);
            SphereCollider trigger = token.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.55f;
            token.AddComponent<RiverToken>();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "Token visual";
            visual.transform.SetParent(token.transform, false);
            visual.transform.localScale = new Vector3(0.7f, 0.12f, 0.7f);
            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
                Destroy(visualCollider);
            visual.GetComponent<Renderer>().sharedMaterial = tokenMaterial;
        }

        private static GameObject CreateBlock(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = localPosition;
            block.transform.localScale = localScale;
            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            Collider collider = block.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
            return block;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            Material material = new Material(shader) { name = name, color = color };
            return material;
        }
    }
}
