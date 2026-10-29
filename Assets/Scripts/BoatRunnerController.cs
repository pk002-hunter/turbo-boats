using UnityEngine;
using UnityEngine.InputSystem;

namespace TurboBoats
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BoatRunnerController : MonoBehaviour
    {
        [SerializeField] private float forwardSpeed = 12f;
        [SerializeField, Min(1f)] private float boostSpeedMultiplier = 1.75f;
        [SerializeField, Min(0.1f)] private float boostDuration = 2f;
        [SerializeField, Min(0f)] private float boostCooldown = 5f;
        [SerializeField] private float steeringSpeed = 7.5f;
        [SerializeField] private float steeringAcceleration = 18f;
        [SerializeField] private float riverHalfWidth = 4.8f;
        [SerializeField] private float smokeOffset = -1.35f;

        private Rigidbody boatBody;
        private float lateralVelocity;
        private float steeringInput;
        private float boostTimeRemaining;
        private float boostCooldownRemaining;
        private bool boostActivationPending;
        private Vector2 touchStart;
        private bool trackingTouch;
        private bool touchStartedOnBoost;
        private int touchButtonDirection;
        private ParticleSystem damageSmoke;

        public bool IsBoosting => boostTimeRemaining > 0f;
        public bool IsBoostReady => !IsBoosting && boostCooldownRemaining <= 0f;
        public float BoostCooldownRemaining => boostCooldownRemaining;
        public float ForwardSpeed => IsBoosting ? forwardSpeed * boostSpeedMultiplier : forwardSpeed;

        private void Awake()
        {
            boatBody = GetComponent<Rigidbody>();
            boatBody.interpolation = RigidbodyInterpolation.Interpolate;

            // The prototype's original root collider covered the whole boat. Replace it
            // with bow/stern zones so the stern can take damage without ending the run.
            foreach (Collider collider in GetComponents<Collider>())
                collider.enabled = false;

            EnsureCollisionZone("Bow Collision Zone", new Vector3(0f, 0f, 1.15f), new Vector3(1.65f, 0.72f, 1.5f), false);
            EnsureCollisionZone("Stern Collision Zone", new Vector3(0f, 0f, -1.55f), new Vector3(1.45f, 0.72f, 1.25f), true);
            CreateDamageSmoke();
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                steeringInput = 0f;
                boostActivationPending = false;
                return;
            }

            steeringInput = ReadSteeringInput();
            if (ReadBoostPressedThisFrame())
                boostActivationPending = true;
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            boostCooldownRemaining = Mathf.Max(0f, boostCooldownRemaining - Time.fixedDeltaTime);
            if (boostActivationPending)
            {
                boostActivationPending = false;
                if (boostTimeRemaining <= 0f && boostCooldownRemaining <= 0f)
                {
                    boostTimeRemaining = boostDuration;
                    boostCooldownRemaining = boostCooldown;
                }
            }

            Vector3 position = boatBody.position;
            float boundedSteering = steeringInput;
            if ((position.x <= -riverHalfWidth && boundedSteering < 0f)
                || (position.x >= riverHalfWidth && boundedSteering > 0f))
            {
                boundedSteering = 0f;
            }

            lateralVelocity = Mathf.MoveTowards(
                lateralVelocity,
                boundedSteering * steeringSpeed,
                steeringAcceleration * Time.fixedDeltaTime);

            float currentForwardSpeed = ForwardSpeed;
            if (boostTimeRemaining > 0f)
            {
                boostTimeRemaining = Mathf.Max(0f, boostTimeRemaining - Time.fixedDeltaTime);
            }

            position.z += currentForwardSpeed * Time.fixedDeltaTime;
            position.x = Mathf.Clamp(position.x + lateralVelocity * Time.fixedDeltaTime, -riverHalfWidth, riverHalfWidth);
            if ((position.x <= -riverHalfWidth && lateralVelocity < 0f)
                || (position.x >= riverHalfWidth && lateralVelocity > 0f))
            {
                lateralVelocity = 0f;
            }

            float bank = -Mathf.Clamp(lateralVelocity * 1.8f, -18f, 18f);
            float heading = Mathf.Clamp(-lateralVelocity * 1.1f, -10f, 10f);
            Quaternion targetRotation = Quaternion.Euler(0f, heading, bank);
            boatBody.MovePosition(position);
            boatBody.MoveRotation(Quaternion.Slerp(boatBody.rotation, targetRotation, Time.fixedDeltaTime * 4f));
        }

        public void ResetBoat()
        {
            lateralVelocity = 0f;
            steeringInput = 0f;
            boostTimeRemaining = 0f;
            boostCooldownRemaining = 0f;
            boostActivationPending = false;
            boatBody.position = new Vector3(0f, 0.15f, 0f);
            boatBody.rotation = Quaternion.identity;
            if (damageSmoke != null)
                damageSmoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        internal void HandleCollision(Collider other, bool isStern)
        {
            if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
                return;

            HealthBooster booster = other.GetComponent<HealthBooster>();
            if (booster != null)
            {
                booster.Collect();
                return;
            }

            RiverToken token = other.GetComponent<RiverToken>();
            if (token != null)
            {
                token.Collect();
                return;
            }

            if (other.GetComponent<BoatObstacle>() == null)
                return;

            if (isStern)
            {
                if (GameManager.Instance.TakeDamage())
                    EmitDamageSmoke();
            }
            else
            {
                GameManager.Instance.Crash();
            }
        }

        private float ReadSteeringInput()
        {
            float direction = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) direction -= 1f;
                if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) direction += 1f;
            }
            if (Gamepad.current != null)
                direction += Gamepad.current.leftStick.x.ReadValue();

            if (Touchscreen.current == null)
            {
                trackingTouch = false;
                touchStartedOnBoost = false;
                touchButtonDirection = 0;
                return Mathf.Clamp(direction, -1f, 1f);
            }

            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                touchStart = touch.position.ReadValue();
                trackingTouch = true;
                touchStartedOnBoost = GameManager.GetBoostControlRect(GetTouchSafeArea()).Contains(touchStart);
                touchButtonDirection = GetTouchButtonDirection(touchStart);
            }
            if (trackingTouch && touch.press.isPressed)
            {
                if (touchStartedOnBoost)
                {
                    direction = 0f;
                }
                else if (touchButtonDirection != 0)
                {
                    direction = touchButtonDirection;
                }
                else
                {
                    float delta = touch.position.ReadValue().x - touchStart.x;
                    direction = Mathf.Clamp(delta / Mathf.Max(1f, Screen.width * 0.18f), -1f, 1f);
                }
            }
            if (touch.press.wasReleasedThisFrame)
            {
                trackingTouch = false;
                touchStartedOnBoost = false;
                touchButtonDirection = 0;
            }

            return Mathf.Clamp(direction, -1f, 1f);
        }

        private static int GetTouchButtonDirection(Vector2 touchPosition)
        {
            Rect safeArea = GetTouchSafeArea();

            if (GameManager.GetTouchControlRect(safeArea, false).Contains(touchPosition))
                return -1;
            if (GameManager.GetTouchControlRect(safeArea, true).Contains(touchPosition))
                return 1;
            return 0;
        }

        private bool ReadBoostPressedThisFrame()
        {
            bool pressed = Keyboard.current != null
                && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame);

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return pressed;

            Rect boostControl = GameManager.GetBoostControlRect(GetTouchSafeArea());
            for (int i = 0; i < touchscreen.touches.Count; i++)
            {
                var touch = touchscreen.touches[i];
                if (touch.press.wasPressedThisFrame && boostControl.Contains(touch.position.ReadValue()))
                    return true;
            }

            return pressed;
        }

        private static Rect GetTouchSafeArea()
        {
            Rect safeArea = Screen.safeArea;
            if (safeArea.width <= 0f || safeArea.height <= 0f)
                safeArea = new Rect(0f, 0f, Screen.width, Screen.height);
            return safeArea;
        }

        private void EnsureCollisionZone(string zoneName, Vector3 localPosition, Vector3 size, bool isStern)
        {
            Transform existing = transform.Find(zoneName);
            GameObject zone = existing != null ? existing.gameObject : new GameObject(zoneName);
            zone.transform.SetParent(transform, false);
            zone.transform.localPosition = localPosition;
            zone.transform.localRotation = Quaternion.identity;
            // Use Unity's overloaded null check here. Null-coalescing (??) only checks
            // CLR null and can keep a destroyed/missing Unity component wrapper.
            if (!zone.TryGetComponent(out BoxCollider box) || box == null)
                box = zone.AddComponent<BoxCollider>();
            if (box == null)
            {
                Debug.LogError($"Could not create the BoxCollider for {zoneName}.", zone);
                return;
            }
            box.size = size;
            box.isTrigger = true;
            if (!zone.TryGetComponent(out BoatCollisionZone handler) || handler == null)
                handler = zone.AddComponent<BoatCollisionZone>();
            if (handler == null)
            {
                Debug.LogError($"Could not create the collision handler for {zoneName}.", zone);
                return;
            }
            handler.Initialize(this, isStern);
        }

        private void CreateDamageSmoke()
        {
            GameObject smokeObject = new GameObject("Stern Damage Smoke");
            smokeObject.transform.SetParent(transform, false);
            smokeObject.transform.localPosition = new Vector3(0f, 0.45f, smokeOffset);
            damageSmoke = smokeObject.AddComponent<ParticleSystem>();
            var main = damageSmoke.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 1.1f;
            main.startSpeed = 1.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.16f, 0.42f);
            main.startColor = new Color(0.2f, 0.2f, 0.2f, 0.72f);
            var emission = damageSmoke.emission;
            emission.enabled = false;
            var shape = damageSmoke.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.15f;
        }

        private void EmitDamageSmoke()
        {
            if (damageSmoke != null)
                damageSmoke.Emit(18);
        }
    }
}
