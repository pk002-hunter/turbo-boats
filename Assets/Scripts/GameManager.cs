using UnityEngine;
using UnityEngine.InputSystem;

namespace TurboBoats
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private BoatRunnerController boat;
        [SerializeField] private EndlessRiverWorld river;
        [SerializeField] private int maximumHealth = 3;

        private GUIStyle titleStyle;
        private GUIStyle detailStyle;
        private GUIStyle overlayStyle;
        private GUIStyle buttonStyle;
        private GUIStyle touchControlStyle;
        private int cachedDistance = int.MinValue;
        private int cachedTokens = int.MinValue;
        private int cachedHealth = int.MinValue;
        private int cachedBoostSeconds = int.MinValue;
        private bool cachedBoosting;
        private bool cachedBoostReady;
        private string cachedZone;
        private string compactMetricsText;
        private string compactHullText;
        private string wideMetricsText;
        private string boostControlText = "BOOST";

        public bool IsGameOver { get; private set; }
        public int Tokens { get; private set; }
        public int Health { get; private set; }
        public int MaximumHealth => maximumHealth;

        public BoatRunnerController Boat
        {
            get => boat;
            set => boat = value;
        }

        public EndlessRiverWorld River
        {
            get => river;
            set => river = value;
        }

        private void Awake()
        {
            Instance = this;
            Health = maximumHealth;
            RefreshHudText();
        }

        private void Update()
        {
            RefreshHudText();
        }

        public void Crash()
        {
            IsGameOver = true;
        }

        public bool TakeDamage()
        {
            if (IsGameOver || Health <= 0)
                return false;

            Health = Mathf.Max(0, Health - 1);
            if (Health == 0)
                Crash();
            return true;
        }

        public void RestoreHealth(int amount)
        {
            Health = Mathf.Min(maximumHealth, Health + Mathf.Max(0, amount));
        }

        public void CollectToken()
        {
            Tokens++;
        }

        public void RestartRun()
        {
            IsGameOver = false;
            Tokens = 0;
            Health = maximumHealth;
            if (boat != null)
                boat.ResetBoat();
            if (river != null)
                river.ResetRiver();
        }

        public static Rect GetTouchControlRect(Rect safeArea, bool rightSide)
        {
            float width = Mathf.Clamp(safeArea.width * 0.28f, 120f, 180f);
            float height = Mathf.Clamp(safeArea.height * 0.16f, 82f, 126f);
            const float edgeInset = 18f;
            float y = safeArea.yMin + edgeInset;
            float x = rightSide ? safeArea.xMax - edgeInset - width : safeArea.xMin + edgeInset;
            return new Rect(x, y, width, height);
        }

        public static Rect GetBoostControlRect(Rect safeArea)
        {
            Rect steeringControl = GetTouchControlRect(safeArea, false);
            float width = steeringControl.width;
            float height = steeringControl.height;
            float x = safeArea.center.x - width * 0.5f;
            float y = steeringControl.yMin;

            const float edgeInset = 18f;
            const float rowGap = 10f;
            if (safeArea.width < width * 3f + edgeInset * 2f)
                y = steeringControl.yMax + rowGap;

            if (y + height > safeArea.yMax - edgeInset)
                y = Mathf.Max(safeArea.yMin + edgeInset, safeArea.yMax - edgeInset - height);

            return new Rect(x, y, width, height);
        }

        private void OnGUI()
        {
            Rect safeArea = Screen.safeArea;
            if (safeArea.width <= 0f || safeArea.height <= 0f)
                safeArea = new Rect(0f, 0f, Screen.width, Screen.height);

            Rect guiSafeArea = new Rect(
                safeArea.xMin,
                Screen.height - safeArea.yMax,
                safeArea.width,
                safeArea.height);

            GUI.BeginGroup(guiSafeArea);
            DrawHud(safeArea.width, safeArea.height, safeArea);
            GUI.EndGroup();
        }

        private void DrawHud(float width, float height, Rect safeArea)
        {
            float panelWidth = Mathf.Min(width - 28f, 660f);
            bool compact = panelWidth < 440f;
            float scale = Mathf.Clamp(panelWidth / 560f, 0.72f, 1.18f);
            float panelHeight = compact ? 112f : 100f;
            float panelX = (width - panelWidth) * 0.5f;

            EnsureStyles(scale);
            DrawPanel(new Rect(panelX, 14f, panelWidth, panelHeight));

            GUI.Label(new Rect(panelX + 16f, 24f, panelWidth - 32f, 30f), "TURBO BOATS", titleStyle);

            if (compact)
            {
                GUI.Label(new Rect(panelX + 16f, 56f, panelWidth - 32f, 22f),
                    compactMetricsText, detailStyle);
                GUI.Label(new Rect(panelX + 16f, 81f, panelWidth - 32f, 22f),
                    compactHullText, detailStyle);
            }
            else
            {
                GUI.Label(new Rect(panelX + 16f, 59f, panelWidth - 32f, 24f),
                    wideMetricsText, detailStyle);
                if (river != null)
                    GUI.Label(new Rect(panelX + 16f, 81f, panelWidth - 32f, 18f), river.CurrentZoneName, detailStyle);
            }

            if (IsGameOver)
            {
                DrawGameOver(width, height, scale);
                return;
            }

            bool showTouchControls = Application.isMobilePlatform || Touchscreen.current != null;
            if (showTouchControls)
                DrawTouchControls(height, safeArea, 14f + panelHeight);
            else
                GUI.Label(new Rect(16f, height - 38f, width - 32f, 24f),
                    "Steer with A / D or ← / →   •   Boost with W or ↑", detailStyle);
        }

        private void DrawGameOver(float width, float height, float scale)
        {
            float panelWidth = Mathf.Min(width - 36f, 440f);
            float x = (width - panelWidth) * 0.5f;
            float y = height * 0.37f;
            DrawPanel(new Rect(x, y, panelWidth, 164f));

            GUI.Label(new Rect(x + 12f, y + 18f, panelWidth - 24f, 48f), "HULL BROKEN!", overlayStyle);
            GUI.Label(new Rect(x + 12f, y + 67f, panelWidth - 24f, 28f), "Try the run again", detailStyle);

            float buttonWidth = Mathf.Min(panelWidth - 40f, 220f);
            float buttonHeight = Mathf.Max(52f, 60f * scale);
            Rect restartRect = new Rect((width - buttonWidth) * 0.5f, y + 103f, buttonWidth, buttonHeight);
            bool restartPressed = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            if (GUI.Button(restartRect, "RESTART", buttonStyle) || restartPressed)
                RestartRun();
        }

        private void DrawTouchControls(float height, Rect safeArea, float panelBottom)
        {
            Rect leftControl = GetTouchControlRect(safeArea, false);
            Rect rightControl = GetTouchControlRect(safeArea, true);
            Rect boostControl = GetBoostControlRect(safeArea);
            float controlTop = Mathf.Max(leftControl.yMax, boostControl.yMax);
            float touchTop = height - (controlTop - safeArea.yMin);
            DrawTouchControl(leftControl, height, safeArea, "STEER\nLEFT");
            DrawTouchControl(rightControl, height, safeArea, "STEER\nRIGHT");
            DrawTouchControl(boostControl, height, safeArea, boostControlText);

            float instructionY = touchTop - 30f;
            if (instructionY >= panelBottom + 8f && instructionY + 24f < touchTop)
                GUI.Label(new Rect(0f, instructionY, safeArea.width, 24f), "OR DRAG ANYWHERE TO STEER", detailStyle);
        }

        private void DrawTouchControl(Rect screenRect, float height, Rect safeArea, string label)
        {
            Rect guiRect = new Rect(
                screenRect.xMin - safeArea.xMin,
                height - (screenRect.yMin - safeArea.yMin) - screenRect.height,
                screenRect.width,
                screenRect.height);
            GUI.Box(guiRect, label, touchControlStyle);
        }

        private void RefreshHudText()
        {
            int distance = boat == null ? 0 : Mathf.FloorToInt(Mathf.Max(0f, boat.transform.position.z));
            string zone = river == null ? string.Empty : river.CurrentZoneName;
            bool isBoosting = boat != null && boat.IsBoosting;
            bool isBoostReady = boat == null || boat.IsBoostReady;
            int boostSeconds = boat == null ? 0 : Mathf.CeilToInt(Mathf.Max(0f, boat.BoostCooldownRemaining));
            if (distance == cachedDistance && Tokens == cachedTokens && Health == cachedHealth && zone == cachedZone
                && isBoosting == cachedBoosting && isBoostReady == cachedBoostReady && boostSeconds == cachedBoostSeconds)
                return;

            cachedDistance = distance;
            cachedTokens = Tokens;
            cachedHealth = Health;
            cachedZone = zone;
            cachedBoosting = isBoosting;
            cachedBoostReady = isBoostReady;
            cachedBoostSeconds = boostSeconds;
            compactMetricsText = $"DISTANCE  {distance} m     TOKENS  {Tokens}";
            compactHullText = $"HULL  {Health} / {maximumHealth}   ·   {zone}";
            wideMetricsText = $"DISTANCE  {distance} m       TOKENS  {Tokens}       HULL  {Health} / {maximumHealth}";
            boostControlText = isBoosting ? "BOOST\nACTIVE" : isBoostReady ? "BOOST" : $"BOOST\n{boostSeconds}s";
        }

        private static void DrawPanel(Rect rect)
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0.025f, 0.09f, 0.11f, 0.82f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = previousColor;
        }

        private void EnsureStyles(float scale)
        {
            if (titleStyle == null)
                titleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            if (detailStyle == null)
                detailStyle = new GUIStyle(GUI.skin.label);
            if (overlayStyle == null)
                overlayStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            if (buttonStyle == null)
                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            if (touchControlStyle == null)
                touchControlStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };

            titleStyle.fontSize = Mathf.RoundToInt(22f * scale);
            detailStyle.fontSize = Mathf.RoundToInt(16f * scale);
            overlayStyle.fontSize = Mathf.RoundToInt(34f * scale);
            buttonStyle.fontSize = Mathf.RoundToInt(19f * scale);
            touchControlStyle.fontSize = Mathf.RoundToInt(16f * scale);

            titleStyle.normal.textColor = Color.white;
            detailStyle.normal.textColor = new Color(0.86f, 0.94f, 0.92f);
            overlayStyle.normal.textColor = Color.white;
            touchControlStyle.normal.textColor = Color.white;
        }
    }
}
