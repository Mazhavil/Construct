using Construct;
using Construct.Components;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Player
{
    public sealed class XRPlayerInteraction : PlayerConnection
    {
        [SerializeField] private InputActionProperty _downloadAction;
        [SerializeField] private InputActionProperty _switchInteractionAction;
        [SerializeField] private XRDirectInteractor _rightDirectInteractor;
        [SerializeField] private XRRayInteractor _rightRayInteractor;

        private bool _isDirectMode = true;
        private CharacterController _characterController;
        private CharacterControllerDriver _characterControllerDriver;
        private XROrigin _xrOrigin;
        private ActionBasedController _rayInteractorController;
        private ActionBasedController _directInteractorController;

        void Start()
        {
            _downloadAction.action.Enable();
            _switchInteractionAction.action.Enable();

            _characterController = GetComponent<CharacterController>();
            _characterControllerDriver = GetComponent<CharacterControllerDriver>();
            _rayInteractorController = _rightRayInteractor.GetComponent<ActionBasedController>();
            _directInteractorController = _rightDirectInteractor.GetComponent<ActionBasedController>();
            _xrOrigin = GetComponent<XROrigin>();

            _rightRayInteractor.enabled = false;
            _rightDirectInteractor.enabled = true;
            _rayInteractorController.hideControllerModel = true;
            _directInteractorController.hideControllerModel = false;

            _downloadAction.action.started += _ => 
            {
                var entity = World.NewEntity();
                ref var conventus = ref World.GetPool<LoadConventus>().Add(entity);
                conventus.Id = 1;
            };

            _switchInteractionAction.action.started += _ =>
            {
                _isDirectMode = !_isDirectMode;

                _rightDirectInteractor.enabled = _isDirectMode;
                _rightRayInteractor.enabled = !_isDirectMode;
                _directInteractorController.hideControllerModel = !_isDirectMode;
                _rayInteractorController.hideControllerModel = _isDirectMode;
            };
        }

        private void Update()
        {
            UpdateCharacterController();
        }

        private void UpdateCharacterController()
        {
            if (_xrOrigin == null || _characterController == null) return;

            var height = Mathf.Clamp(
                _xrOrigin.CameraInOriginSpaceHeight,
                _characterControllerDriver.minHeight,
                _characterControllerDriver.maxHeight);

            var center = _xrOrigin.CameraInOriginSpacePos;
            center.y = height / 2.0f + _characterController.skinWidth;

            _characterController.height = height;
            _characterController.center = center;
        }
    }
}