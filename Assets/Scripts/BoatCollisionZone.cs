using UnityEngine;

namespace TurboBoats
{
    public sealed class BoatCollisionZone : MonoBehaviour
    {
        private BoatRunnerController boat;
        private bool isStern;

        internal void Initialize(BoatRunnerController owner, bool stern)
        {
            boat = owner;
            isStern = stern;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (boat != null)
                boat.HandleCollision(other, isStern);
        }
    }
}
