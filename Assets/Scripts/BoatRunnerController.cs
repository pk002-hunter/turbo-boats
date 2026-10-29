using UnityEngine;
using UnityEngine.InputSystem;

namespace TurboBoats
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BoatRunnerController : MonoBehaviour
    {
        [SerializeField] private float forwardSpeed = 12f;
        [SerializeField] private float steeringSpeed = 7.5f;
        [SerializeField] private float steeringAcceleration = 18f;
        [SerializeField] private float riverHalfWidth = 4.8f;
        [SerializeField] private float smokeOffset = -1.35f;

        private float lateralVelocity;
        private Vector2 touchStart;
        private bool trackingTouch;
        private ParticleSystem damageSmoke;

        public float ForwardSpeed => forwardSpeed;

        private void Awake()
        {
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
                return;

            float steering = ReadSteeringInput();
            lateralVelocity = Mathf.MoveTowards(lateralVelocity, steering * steeringSpeed, steeringAcceleration * Time.deltaTime);

            Vector3 position = transform.position;
            position.z += forwardSpeed * Time.deltaTime;
            position.x = Mathf.Clamp(position.x + lateralVelocity * Time.deltaTime, -riverHalfWidth, riverHalfWidth);
            if (Mathf.Abs(position.x) >= riverHalfWidth)
                lateralVelocity = 0f;
            transform.position = position;

            float bank = -Mathf.Clamp(lateralVelocity * 1.8f, -18f, 18f);
            float heading = Mathf.Clamp(-lateralVelocity * 1.1f, -10f, 10f);
            Quaternion targetRotation = Quaternion.Euler(0f, heading, bank);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 4f);
        }

        public void ResetBoat()
        {
            lateralVelocity = 0f;
            transform.position = new Vector3(0f, 0.15f, 0f);
            transform.rotation = Quaternion.identity;
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
                return Mathf.Clamp(direction, -1f, 1f);
            }

            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                touchStart = touch.position.ReadValue();
                trackingTouch = true;
            }
            if (trackingTouch && touch.press.isPressed)
            {
                float delta = touch.position.ReadValue().x - touchStart.x;
                direction = Mathf.Clamp(delta / Mathf.Max(1f, Screen.width * 0.18f), -1f, 1f);
            }
            if (touch.press.wasReleasedThisFrame)
                trackingTouch = false;

            return Mathf.Clamp(direction, -1f, 1f);
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
