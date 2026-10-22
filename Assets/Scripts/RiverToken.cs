using UnityEngine;

namespace TurboBoats
{
    public sealed class RiverToken : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(0f, 100f * Time.deltaTime, 0f, Space.World);
        }

        public void Collect()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.CollectToken();
            Destroy(gameObject);
        }
    }
}
