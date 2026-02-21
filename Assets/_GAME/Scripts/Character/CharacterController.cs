using UnityEngine;

namespace Aventra.Game
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private float speed = 5.0f;
        
        private const string HORIZONTAL_INPUT = "Horizontal";
        private const string VERTICAL_INPUT = "Vertical";

        public float Horizontal = Input.GetAxis(HORIZONTAL_INPUT);
        public float Vertical = Input.GetAxis(VERTICAL_INPUT);

        public void Move()
        {
            Vector3 movementDirection = new Vector3(Horizontal, 0, Vertical).normalized;
            transform.Translate(movementDirection * speed * Time.deltaTime, Space.World);
        }
    }
}