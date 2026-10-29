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
        private Material[] bankMaterials;
        private Material[] roofMaterials;
        private Material markerMaterial;
        private Material foliageMaterial;
        private Material lightFoliageMaterial;
        private Material reedMaterial;
        private Material timberMaterial;
        private Material timberDarkMaterial;
        private Material paddyWaterMaterial;
        private Material paddyRowMaterial;
        private Material marketCanopyMaterial;
        private Material marketCanopyAltMaterial;
        private Material buoyAccentMaterial;
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
                MakeMaterial("Bangkok Canal Water", new Color(0.055f, 0.31f, 0.35f), 0.72f),
                MakeMaterial("Market Canal Water", new Color(0.045f, 0.36f, 0.35f), 0.76f),
                MakeMaterial("Rice Field Water", new Color(0.085f, 0.34f, 0.28f), 0.64f),
                MakeMaterial("Forest River Water", new Color(0.035f, 0.23f, 0.24f), 0.72f)
            };
            bankMaterials = new[]
            {
                MakeMaterial("Canal Silt and Stone", new Color(0.36f, 0.30f, 0.19f)),
                MakeMaterial("Market Canal Timber Bank", new Color(0.40f, 0.30f, 0.18f)),
                MakeMaterial("Rice Field Earth", new Color(0.40f, 0.39f, 0.20f)),
                MakeMaterial("Forest River Earth", new Color(0.20f, 0.25f, 0.14f))
            };
            roofMaterials = new[]
            {
                MakeMaterial("Canal Terracotta Roof", new Color(0.55f, 0.20f, 0.12f)),
                MakeMaterial("Market Orange Awning", new Color(0.86f, 0.32f, 0.10f)),
                MakeMaterial("Paddy Thatch", new Color(0.61f, 0.43f, 0.17f)),
                MakeMaterial("Forest Roof Thatch", new Color(0.31f, 0.31f, 0.13f))
            };
            markerMaterial = MakeMaterial("River Route Buoy", new Color(0.93f, 0.74f, 0.39f), 0.38f);
            buoyAccentMaterial = MakeMaterial("Buoy Saffron Cap", new Color(0.93f, 0.32f, 0.11f));
            foliageMaterial = MakeMaterial("Deep Tropical Foliage", new Color(0.08f, 0.25f, 0.12f));
            lightFoliageMaterial = MakeMaterial("Sunlit Tropical Foliage", new Color(0.20f, 0.39f, 0.12f));
            reedMaterial = MakeMaterial("River Reeds", new Color(0.40f, 0.43f, 0.16f));
            timberMaterial = MakeMaterial("Weathered River Timber", new Color(0.39f, 0.21f, 0.11f));
            timberDarkMaterial = MakeMaterial("Shadowed River Timber", new Color(0.22f, 0.12f, 0.075f));
            paddyWaterMaterial = MakeMaterial("Irrigated Field Water", new Color(0.18f, 0.40f, 0.27f), 0.52f);
            paddyRowMaterial = MakeMaterial("Rice Seedling Rows", new Color(0.48f, 0.58f, 0.20f));
            marketCanopyMaterial = MakeMaterial("Market Canopy Mango", new Color(0.92f, 0.61f, 0.18f));
            marketCanopyAltMaterial = MakeMaterial("Market Canopy Indigo", new Color(0.10f, 0.31f, 0.36f));
            obstacleMaterial = MakeMaterial("Floating Driftwood", new Color(0.29f, 0.15f, 0.075f));
            tokenMaterial = MakeMaterial("River Token Gold", new Color(1f, 0.69f, 0.08f), 0.68f);
            healthMaterial = MakeMaterial("Health Booster Green", new Color(0.12f, 0.82f, 0.32f));
            healthMarkMaterial = MakeMaterial("Health Booster Mark", Color.white);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.55f, 0.70f, 0.68f);
            RenderSettings.fogStartDistance = 105f;
            RenderSettings.fogEndDistance = 250f;
        }

        private void OnDestroy()
        {
            DestroyMaterials(waterMaterials);
            DestroyMaterials(bankMaterials);
            DestroyMaterials(roofMaterials);
            Material[] singleMaterials =
            {
                markerMaterial, foliageMaterial, lightFoliageMaterial, reedMaterial,
                timberMaterial, timberDarkMaterial, paddyWaterMaterial, paddyRowMaterial,
                marketCanopyMaterial, marketCanopyAltMaterial, buoyAccentMaterial,
                obstacleMaterial, tokenMaterial, healthMaterial, healthMarkMaterial
            };
            for (int i = 0; i < singleMaterials.Length; i++)
            {
                if (singleMaterials[i] != null)
                    Destroy(singleMaterials[i]);
            }
        }

        private void DestroyMaterials(Material[] materials)
        {
            if (materials == null)
                return;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                    Destroy(materials[i]);
            }
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
            root.AddComponent<GeneratedMeshCleanup>();
            sections.Add(root);

            CreateBlock(root.transform, "Water", new Vector3(0f, -0.42f, SectionLength * 0.5f), new Vector3(18f, 0.2f, SectionLength), waterMaterials[zoneIndex]);
            CreateBlock(root.transform, "Left bank", new Vector3(-14f, -0.05f, SectionLength * 0.5f), new Vector3(10f, 0.8f, SectionLength), bankMaterials[zoneIndex]);
            CreateBlock(root.transform, "Right bank", new Vector3(14f, -0.05f, SectionLength * 0.5f), new Vector3(10f, 0.8f, SectionLength), bankMaterials[zoneIndex]);

            for (int channelSide = -1; channelSide <= 1; channelSide += 2)
            {
                for (int buoy = 0; buoy < 5; buoy++)
                {
                    float z = 3f + buoy * 8f;
                    CreateRiverBuoy(root.transform, channelSide * 8.25f, z, buoy % 2 == 0);
                }
            }

            AddBankDecoration(root.transform, zoneIndex);
            AddShorelineReeds(root.transform, zoneIndex);
            if (index > 0)
                AddHazardsAndTokens(root.transform, index);
        }

        private void AddBankDecoration(Transform parent, int zoneIndex)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * Random.Range(10.5f, 13.5f);
                float z = Random.Range(5f, SectionLength - 5f);
                float towardWater = side < 0 ? 90f : -90f;
                if (zoneIndex == 1)
                {
                    CreateMarketStall(parent, x, z, towardWater, side);
                }
                else if (zoneIndex == 2)
                {
                    AddRicePaddies(parent, side, z);
                    CreateStiltHouse(parent, "Rice field hut", x, z, side, roofMaterials[zoneIndex], timberMaterial);
                }
                else if (zoneIndex == 3)
                    CreatePalm(parent, x, z, side);
                else
                    CreateStiltHouse(parent, "Bangkok canal house", x, z, side, roofMaterials[zoneIndex], timberMaterial);
            }
        }

        private void CreateRiverBuoy(Transform parent, float x, float z, bool useAccent)
        {
            Material capMaterial = useAccent ? buoyAccentMaterial : markerMaterial;
            CreatePrimitive(parent, "Floating channel buoy", PrimitiveType.Cylinder, new Vector3(x, -0.17f, z), new Vector3(0.42f, 0.2f, 0.42f), Quaternion.identity, markerMaterial);
            CreatePrimitive(parent, "Buoy crown", PrimitiveType.Sphere, new Vector3(x, -0.015f, z), new Vector3(0.25f, 0.23f, 0.25f), Quaternion.identity, capMaterial);
        }

        private void AddShorelineReeds(Transform parent, int zoneIndex)
        {
            Material reeds = zoneIndex == 3 ? foliageMaterial : reedMaterial;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int cluster = 0; cluster < 4; cluster++)
                {
                    float z = 4f + cluster * 10f + ((side + zoneIndex) & 1) * 4f;
                    float x = side * (9.25f + (cluster % 2) * 0.28f);
                    CreatePrimitive(parent, "Shore reed one", PrimitiveType.Capsule, new Vector3(x, 0.42f, z), new Vector3(0.09f, 0.43f, 0.09f), Quaternion.Euler(0f, 0f, side * -12f), reeds);
                    CreatePrimitive(parent, "Shore reed two", PrimitiveType.Capsule, new Vector3(x + side * 0.16f, 0.38f, z + 0.22f), new Vector3(0.075f, 0.34f, 0.075f), Quaternion.Euler(0f, 0f, side * 17f), reeds);
                }
            }
        }

        private void CreateStiltHouse(Transform parent, string name, float x, float z, int riverSide, Material roof, Material wood)
        {
            CreateBlock(parent, name + " raised floor", new Vector3(x, 1.47f, z), new Vector3(3.1f, 0.16f, 3.3f), timberDarkMaterial);
            for (int postX = -1; postX <= 1; postX += 2)
            {
                for (int postZ = -1; postZ <= 1; postZ += 2)
                    CreatePrimitive(parent, name + " timber stilts", PrimitiveType.Cylinder, new Vector3(x + postX * 1.2f, 0.86f, z + postZ * 1.25f), new Vector3(0.16f, 0.72f, 0.16f), Quaternion.identity, timberMaterial);
            }

            CreateBlock(parent, name + " wooden walls", new Vector3(x, 1.98f, z), new Vector3(2.6f, 0.86f, 2.8f), wood);
            CreateBlock(parent, name + " river-facing shutter", new Vector3(x - riverSide * 1.43f, 2.02f, z), new Vector3(0.055f, 0.44f, 0.82f), timberDarkMaterial);
            CreateBlock(parent, name + " veranda rail", new Vector3(x - riverSide * 1.85f, 1.82f, z), new Vector3(0.08f, 0.09f, 2.65f), roof);
            CreateGableRoof(parent, name + " terracotta pitched roof", new Vector3(x, 3.04f, z), new Vector2(1.8f, 1.9f), 1.2f, roof);
        }

        private void CreateMarketStall(Transform parent, float x, float z, float facingYaw, int side)
        {
            CreateBlock(parent, "Market timber counter", new Vector3(x, 0.85f, z), new Vector3(2.65f, 0.25f, 2.1f), timberDarkMaterial);
            CreateBlock(parent, "Market produce display", new Vector3(x, 1.08f, z), new Vector3(2.15f, 0.25f, 1.55f), side < 0 ? marketCanopyMaterial : marketCanopyAltMaterial);
            for (int post = -1; post <= 1; post += 2)
                CreatePrimitive(parent, "Market canopy pole", PrimitiveType.Cylinder, new Vector3(x + post * 1.12f, 1.65f, z), new Vector3(0.075f, 0.8f, 0.075f), Quaternion.identity, timberMaterial);
            CreateMarketUmbrella(parent, new Vector3(x, 2.42f, z), facingYaw);
            CreateBlock(parent, "Market mooring plank", new Vector3(x - side * 1.75f, 0.54f, z + 0.3f), new Vector3(0.72f, 0.12f, 2.8f), timberMaterial);
        }

        private void CreateMarketUmbrella(Transform parent, Vector3 center, float yaw)
        {
            const int panelCount = 8;
            const float radius = 1.9f;
            MeshBuilder mangoPanels = new MeshBuilder();
            MeshBuilder indigoPanels = new MeshBuilder();
            Vector3 peak = center + Vector3.up * 0.48f;
            for (int i = 0; i < panelCount; i++)
            {
                float angleA = (i * 360f / panelCount + yaw) * Mathf.Deg2Rad;
                float angleB = ((i + 1) * 360f / panelCount + yaw) * Mathf.Deg2Rad;
                Vector3 rimA = center + new Vector3(Mathf.Cos(angleA) * radius, 0f, Mathf.Sin(angleA) * radius);
                Vector3 rimB = center + new Vector3(Mathf.Cos(angleB) * radius, 0f, Mathf.Sin(angleB) * radius);
                (i % 2 == 0 ? mangoPanels : indigoPanels).AddTriangle(peak, rimA, rimB, Vector3.up);
            }
            CreateMeshObject(parent, "Mango market umbrella panels", mangoPanels.Build(), marketCanopyMaterial);
            CreateMeshObject(parent, "Indigo market umbrella panels", indigoPanels.Build(), marketCanopyAltMaterial);
            CreatePrimitive(parent, "Umbrella center pole", PrimitiveType.Cylinder, new Vector3(center.x, center.y - 0.58f, center.z), new Vector3(0.075f, 0.72f, 0.075f), Quaternion.identity, timberDarkMaterial);
        }

        private void AddRicePaddies(Transform parent, int side, float z)
        {
            float fieldX = side * 17.4f;
            CreateBlock(parent, "Flooded rice paddy", new Vector3(fieldX, 0.39f, z), new Vector3(5.8f, 0.055f, 8.4f), paddyWaterMaterial);
            for (int row = -2; row <= 2; row++)
            {
                CreateBlock(parent, "Rice seedling row", new Vector3(fieldX + row * 0.78f, 0.48f, z), new Vector3(0.12f, 0.13f, 7.6f), paddyRowMaterial);
            }
            CreateBlock(parent, "Paddy berm", new Vector3(fieldX, 0.48f, z - 4.22f), new Vector3(6.0f, 0.18f, 0.16f), bankMaterials[2]);
        }

        private void CreatePalm(Transform parent, float x, float z, int side)
        {
            CreatePrimitive(parent, "Palm trunk", PrimitiveType.Cylinder, new Vector3(x, 1.85f, z), new Vector3(0.28f, 1.55f, 0.28f), Quaternion.Euler(0f, 0f, side * 2f), timberDarkMaterial);
            Vector3 crown = new Vector3(x, 3.4f, z);
            CreatePrimitive(parent, "Palm crown", PrimitiveType.Sphere, crown, new Vector3(0.42f, 0.35f, 0.42f), Quaternion.identity, foliageMaterial);
            for (int frond = 0; frond < 7; frond++)
            {
                float angle = frond * (360f / 7f) + side * 16f;
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * new Vector3(0.8f, 0.45f, 0f);
                Vector3 leafPosition = crown + direction * 0.72f;
                Quaternion leafRotation = Quaternion.LookRotation(direction + Vector3.up * 0.45f, Vector3.up);
                CreatePrimitive(parent, "Palm frond", PrimitiveType.Sphere, leafPosition, new Vector3(0.27f, 0.13f, 1.45f), leafRotation, frond % 2 == 0 ? foliageMaterial : lightFoliageMaterial);
            }
        }

        private void CreateGableRoof(Transform parent, string name, Vector3 center, Vector2 halfSize, float rise, Material material)
        {
            float halfWidth = halfSize.x;
            float halfLength = halfSize.y;
            Vector3 ridge = center + Vector3.up * (rise * 0.5f);
            Vector3 eaveLeft = center + Vector3.left * halfWidth - Vector3.up * (rise * 0.5f);
            Vector3 eaveRight = center + Vector3.right * halfWidth - Vector3.up * (rise * 0.5f);
            CreateRoofPanel(parent, name + " left slope", ridge, eaveLeft, halfLength, material);
            CreateRoofPanel(parent, name + " right slope", ridge, eaveRight, halfLength, material);
            CreateBlock(parent, name + " ridge cap", new Vector3(center.x, center.y + rise * 0.5f, center.z), new Vector3(0.13f, 0.12f, halfLength * 2f + 0.12f), timberDarkMaterial);
        }

        private void CreateRoofPanel(Transform parent, string name, Vector3 ridge, Vector3 eave, float halfLength, Material material)
        {
            Vector3 across = eave - ridge;
            Vector3 normal = Vector3.Cross(across, Vector3.forward).normalized;
            if (eave.x > ridge.x)
                normal = -normal;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, normal);
            CreatePrimitive(parent, name, PrimitiveType.Cube, (ridge + eave) * 0.5f, new Vector3(across.magnitude + 0.08f, 0.11f, halfLength * 2f), rotation, material);
        }

        private void CreateMeshObject(Transform parent, string name, Mesh mesh, Material material)
        {
            GameObject visual = new GameObject(name);
            visual.transform.SetParent(parent, false);
            visual.AddComponent<MeshFilter>().sharedMesh = mesh;
            visual.AddComponent<MeshRenderer>().sharedMaterial = material;
            GeneratedMeshCleanup cleanup = parent.GetComponent<GeneratedMeshCleanup>();
            if (cleanup != null)
                cleanup.Register(mesh);
        }

        private void AddHazardsAndTokens(Transform parent, int index)
        {
            var obstacleSpawns = new List<Vector2>();
            int rowCount = Random.Range(1, 3);
            for (int row = 0; row < rowCount; row++)
            {
                float localZ = 9f + row * 17f + Random.Range(0f, 5f);
                int blockedLane = Random.Range(0, 3);
                CreateObstacle(parent.transform, blockedLane, localZ, index + row);
                obstacleSpawns.Add(new Vector2(blockedLane, localZ));

                if (Random.value < 0.55f)
                {
                    int clearLane = (blockedLane + Random.Range(1, 3)) % 3;
                    CreateToken(parent.transform, clearLane, localZ - 2f);
                }
            }

            if (Random.value < 0.38f)
            {
                float pickupZ = 27f + Random.Range(0f, 8f);
                int pickupLane = ChooseClearPickupLane(obstacleSpawns, pickupZ);
                if (pickupLane >= 0)
                    CreateHealthBooster(parent, pickupLane, pickupZ);
            }
        }

        private static int ChooseClearPickupLane(List<Vector2> obstacleSpawns, float pickupZ)
        {
            const float longitudinalClearance = 2f;
            bool[] blockedLanes = new bool[3];
            for (int i = 0; i < obstacleSpawns.Count; i++)
            {
                Vector2 obstacle = obstacleSpawns[i];
                if (Mathf.Abs(obstacle.y - pickupZ) < longitudinalClearance)
                    blockedLanes[Mathf.RoundToInt(obstacle.x)] = true;
            }

            int clearLaneCount = 0;
            for (int lane = 0; lane < blockedLanes.Length; lane++)
            {
                if (!blockedLanes[lane])
                    clearLaneCount++;
            }

            if (clearLaneCount == 0)
                return -1;

            int selectedLane = Random.Range(0, clearLaneCount);
            for (int lane = 0; lane < blockedLanes.Length; lane++)
            {
                if (blockedLanes[lane])
                    continue;

                if (selectedLane == 0)
                    return lane;
                selectedLane--;
            }

            return -1;
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
            return CreatePrimitive(parent, name, PrimitiveType.Cube, localPosition, localScale, Quaternion.identity, material);
        }

        private static GameObject CreatePrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(type);
            visual.name = name;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localScale = localScale;
            visual.transform.localRotation = localRotation;
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
            return visual;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            return MakeMaterial(name, color, 0.25f);
        }

        private static Material MakeMaterial(string name, Color color, float smoothness)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            Material material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            return material;
        }

        private sealed class MeshBuilder
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<int> triangles = new List<int>();

            public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normalHint)
            {
                Vector3 normal = Vector3.Cross(b - a, c - a);
                if (normal.sqrMagnitude > 0.000001f && Vector3.Dot(normal, normalHint) < 0f)
                {
                    Vector3 swap = b;
                    b = c;
                    c = swap;
                }
                int first = vertices.Count;
                vertices.Add(a);
                vertices.Add(b);
                vertices.Add(c);
                triangles.Add(first);
                triangles.Add(first + 1);
                triangles.Add(first + 2);
            }

            public Mesh Build()
            {
                Mesh mesh = new Mesh();
                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }

    internal sealed class GeneratedMeshCleanup : MonoBehaviour
    {
        private readonly List<Mesh> meshes = new List<Mesh>();

        public void Register(Mesh mesh)
        {
            if (mesh != null)
                meshes.Add(mesh);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                if (meshes[i] != null)
                    Destroy(meshes[i]);
            }
        }
    }
}
