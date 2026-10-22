using UnityEngine;

namespace TurboBoats
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private BoatRunnerController boat;
        [SerializeField] private EndlessRiverWorld river;

        public bool IsGameOver { get; private set; }
        public int Tokens { get; private set; }

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
        }

        public void Crash()
        {
            IsGameOver = true;
        }

        public void CollectToken()
        {
            Tokens++;
        }

        public void RestartRun()
        {
            IsGameOver = false;
            Tokens = 0;
            if (boat != null)
                boat.ResetBoat();
            if (river != null)
                river.ResetRiver();
        }

        private void OnGUI()
        {
            GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUIStyle details = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                normal = { textColor = Color.white }
            };

            GUI.Label(new Rect(24f, 20f, 420f, 44f), "TURBO BOATS", title);
            float distance = boat == null ? 0f : Mathf.Max(0f, boat.transform.position.z);
            GUI.Label(new Rect(26f, 66f, 420f, 30f), $"Distance  {Mathf.FloorToInt(distance)} m     Tokens  {Tokens}", details);
            if (river != null)
                GUI.Label(new Rect(26f, 96f, 500f, 30f), river.CurrentZoneName, details);

            if (!IsGameOver)
            {
                GUI.Label(new Rect(26f, Screen.height - 48f, 700f, 28f), "Steer: A / D or ← / →     Mobile: swipe left or right", details);
                return;
            }

            GUIStyle overlay = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(0f, Screen.height * 0.34f, Screen.width, 54f), "BOAT DAMAGED!", overlay);
            GUI.Label(new Rect(0f, Screen.height * 0.44f, Screen.width, 36f), "Try the run again", details);
            if (GUI.Button(new Rect(Screen.width * 0.5f - 75f, Screen.height * 0.53f, 150f, 42f), "Restart") || Input.GetKeyDown(KeyCode.R))
                RestartRun();
        }
    }
}
