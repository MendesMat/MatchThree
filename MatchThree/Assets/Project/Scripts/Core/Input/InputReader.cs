using MatchThree.Project.Scripts.Core.EventBus;
using MatchThree.Project.Scripts.Core.EventBus.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MatchThree.Project.Scripts.Core.Input
{
    public class InputReader : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        private static InputAction _selectAction;
        private InputAction _fireAction;
        
        public static Vector2 Selected => _selectAction.ReadValue<Vector2>();

        private void Awake() => SetupInput();

        private void SetupInput()
        {
            if(playerInput == null) return;
            _selectAction = playerInput.actions["Select"];
            _fireAction = playerInput.actions["Fire"];
        }

        public void OnFireInput(InputAction.CallbackContext context) 
            => EventBus<FireInputEvent>.Publish(new FireInputEvent());
    }
}