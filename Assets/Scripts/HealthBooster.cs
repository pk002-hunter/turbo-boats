using UnityEngine;

namespace TurboBoats
{
    public sealed class HealthBooster : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
        }

        public void Collect()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RestoreHealth(1);
            Destroy(gameObject);
        }
    }
}
