using System;
using UnityEngine;

namespace MyBuild.Scripts.Utils
{
    [ExecuteAlways]
    [AddComponentMenu("UI/World Space Billboard")]
    public class WorldSpaceBillboard : MonoBehaviour
    {
        [Tooltip("Камера, на которую смотреть. Если null — будет использована Camera.main")]
        public Camera targetCamera;

        [Tooltip("Плавность поворота (0 = мгновенно)")]
        [Range(0f, 50f)]
        public float smooth = 12f;

        [Tooltip("Если true — сохранять вертикальную ось (не наклонять)")]
        public bool keepUpright = true;

        [Tooltip("Если true — поворачивать только по Y")]
        public bool onlyYRotation = false;

        Transform _t;
        Transform _camTransform;

        void Awake()
        {
            _t = transform;
            if (targetCamera == null) targetCamera = Camera.main;
            _camTransform = targetCamera != null ? targetCamera.transform : null;
        }

        void LateUpdate()
        {
            if (_t == null) _t = transform;
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return;

            _camTransform = targetCamera.transform;

            Vector3 dir = _camTransform.position - _t.position;
            if (keepUpright)
            {
                dir = Vector3.ProjectOnPlane(dir, Vector3.up);
                if (dir.sqrMagnitude < 1e-6f) return;
            }

            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

            if (onlyYRotation)
            {
                var e = targetRot.eulerAngles;
                targetRot = Quaternion.Euler(0f, e.y, 0f);
            }

            if (smooth <= 0f)
                _t.rotation = targetRot;
            else
                _t.rotation = Quaternion.Slerp(_t.rotation, targetRot, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        }
    }
}
