using MatchThree.Project.Scripts.Core.EventBus;
using MatchThree.Project.Scripts.Core.EventBus.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MatchThree.Project.Scripts.Core.Input
{
    public class InputReader : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        private InputAction _mouseOverAction;
        
        public Vector2 MousePosition => _mouseOverAction.ReadValue<Vector2>();

        private void Awake() => SetupInput();

        private void SetupInput()
        {
            if(playerInput == null) return;
            _mouseOverAction = playerInput.actions["MouseOver"];
        }

        public void OnFireInput(InputAction.CallbackContext context)
        {
            if(context.performed) EventBus<SelectInputEvent>.Publish(new SelectInputEvent());
        }
    }
}