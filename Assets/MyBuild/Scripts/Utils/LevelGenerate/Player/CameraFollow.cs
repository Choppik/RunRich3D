using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Камера следит за игроком с заданным смещением и плавностью.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 5f, -7f);
        public float followSpeed = 5f;
        public float rotationDamp = 3f;

        private Vector3 _velocity;

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPos = target.position + target.TransformDirection(offset);
            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _velocity, 1f / followSpeed);

            Quaternion desiredRot = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotationDamp);
        }
    }
}
