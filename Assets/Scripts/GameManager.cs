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
            GUI.Label(new Rect(26f, 66f, 500f, 30f), $"Distance  {Mathf.FloorToInt(distance)} m     Tokens  {Tokens}     Hull  {Health}/{maximumHealth}", details);
            if (river != null)
                GUI.Label(new Rect(26f, 96f, 500f, 30f), river.CurrentZoneName, details);

            if (!IsGameOver)
            {
                GUI.Label(new Rect(26f, Screen.height - 48f, 850f, 28f), "Steer: hold A / D or ← / →     Mobile: drag left or right", details);
                return;
            }

            GUIStyle overlay = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(0f, Screen.height * 0.34f, Screen.width, 54f), "HULL BROKEN!", overlay);
            GUI.Label(new Rect(0f, Screen.height * 0.44f, Screen.width, 36f), "Try the run again", details);
            bool restartPressed = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            if (GUI.Button(new Rect(Screen.width * 0.5f - 75f, Screen.height * 0.53f, 150f, 42f), "Restart") || restartPressed)
                RestartRun();
        }
    }
}
