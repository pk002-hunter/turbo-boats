using UnityEngine;

namespace TurboBoats
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BoatRunnerController : MonoBehaviour
    {
        [SerializeField] private float forwardSpeed = 12f;
        [SerializeField] private float laneWidth = 3.2f;
        [SerializeField] private float laneChangeSpeed = 8f;

        private int lane = 1;
        private float swipeStartX;
        private bool trackingSwipe;

        public float ForwardSpeed => forwardSpeed;

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            ReadSteeringInput();

            Vector3 position = transform.position;
            position.z += forwardSpeed * Time.deltaTime;
            float targetX = (lane - 1) * laneWidth;
            position.x = Mathf.MoveTowards(position.x, targetX, laneChangeSpeed * Time.deltaTime);
            transform.position = position;

            float turn = targetX - position.x;
            Quaternion targetRotation = Quaternion.Euler(0f, Mathf.Clamp(turn * -4f, -12f, 12f), Mathf.Clamp(turn * 3f, -10f, 10f));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        public void ResetBoat()
        {
            lane = 1;
            transform.position = new Vector3(0f, 0.15f, 0f);
            transform.rotation = Quaternion.identity;
        }

        private void ReadSteeringInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                ChangeLane(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                ChangeLane(1);

            if (Input.touchCount == 0)
            {
                trackingSwipe = false;
                return;
            }

            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                swipeStartX = touch.position.x;
                trackingSwipe = true;
            }
            else if (trackingSwipe && touch.phase == TouchPhase.Ended)
            {
                float swipe = touch.position.x - swipeStartX;
                if (Mathf.Abs(swipe) > Screen.width * 0.08f)
                    ChangeLane(swipe < 0f ? -1 : 1);
                trackingSwipe = false;
            }
        }

        private void ChangeLane(int direction)
        {
            lane = Mathf.Clamp(lane + direction, 0, 2);
        }

        private void OnTriggerEnter(Collider other)
        {
            RiverToken token = other.GetComponent<RiverToken>();
            if (token != null)
            {
                token.Collect();
                return;
            }

            if (other.GetComponent<BoatObstacle>() != null && GameManager.Instance != null)
                GameManager.Instance.Crash();
        }
    }
}
