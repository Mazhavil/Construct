using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Player
{
    public sealed class XRPlayerMovement : MonoBehaviour
    {
        [Header("Input actions")]
        [SerializeField] private InputActionProperty _teleportActivate;
        [SerializeField] private InputActionProperty _teleportDeactivate;
        [SerializeField] private InputActionProperty _thumbstick;
        [SerializeField] private InputActionProperty _gripMode;
        [Header("Other components")]
        [SerializeField] private XRRayInteractor _rayInteractor;
        [SerializeField] private TeleportationProvider _teleportationProvider;
        [SerializeField] private InteractionLayerMask _teleportingLayerMask;

        private bool _isTeleporting;
        private InteractionLayerMask _initialInterectaionLayerMask;

        private void Start()
        {
            _teleportActivate.action.Enable();
            _teleportDeactivate.action.Enable();
            _thumbstick.action.Enable();
            _gripMode.action.Enable();

            _teleportActivate.action.performed += _ =>
            {
                if (_gripMode.action.inProgress) return;

                _rayInteractor.enabled = true;
                _rayInteractor.lineType = XRRayInteractor.LineType.ProjectileCurve;
                _rayInteractor.interactionLayers = _teleportingLayerMask;
                _isTeleporting = true;
            };

            _teleportDeactivate.action.performed += _ => DisableTeleportation();

            _initialInterectaionLayerMask = _rayInteractor.interactionLayers;
            _rayInteractor.enabled = false;
        }

        private void Update()
        {
            if (!_isTeleporting) return;

            if (_thumbstick.action.inProgress) return;

            if (!_rayInteractor.TryGetCurrent3DRaycastHit(out var hit)) {
                DisableTeleportation();
                return;
            }

            if (!hit.transform.TryGetComponent<TeleportationArea>(out _)) {
                DisableTeleportation();
                return;
            }

            _teleportationProvider.QueueTeleportRequest(new() {
                destinationPosition = hit.point
            });

            DisableTeleportation();
        }

        private void DisableTeleportation()
        {
            _rayInteractor.enabled = false;
            _rayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
            _rayInteractor.interactionLayers = _initialInterectaionLayerMask;
            _isTeleporting = false;
        }
    }
}