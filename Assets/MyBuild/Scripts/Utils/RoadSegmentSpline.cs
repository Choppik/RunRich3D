using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MyBuild.Scripts.Utils
{
    public class RoadSegmentSpline : MonoBehaviour
    {
        [Header("Control Points")]
        public Transform p0, p1, p2, p3; // 4 точки для кубического Безье


        public Vector3 GetPoint(float t)
        {
            t = Mathf.Clamp01(t);
            float t2 = t * t;
            float t3 = t2 * t;
            float mt = 1f - t;
            float mt2 = mt * mt;
            float mt3 = mt2 * mt;

            return mt3 * p0.position +
                   3f * mt2 * t * p1.position +
                   3f * mt * t2 * p2.position +
                   t3 * p3.position;
        }

        public Vector3 GetTangent(float t)
        {
            t = Mathf.Clamp01(t);
            float t2 = t * t;
            float mt = 1f - t;
            float mt2 = mt * mt;

            return -3f * mt2 * p0.position +
                   (3f * mt2 - 6f * mt * t) * p1.position +
                   (6f * mt * t - 3f * t2) * p2.position +
                   3f * t2 * p3.position;
        }
        private float _length = -1f; // кэшируем длину

        public float GetLength()
        {
            if (_length < 0f) _length = GetSplineLength();
            return _length;
        }


        public float GetSplineLength(int steps = 64)
        {
            float len = 0f;
            Vector3 prev = GetPoint(0f);
            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector3 curr = GetPoint(t);
                len += Vector3.Distance(prev, curr);
                prev = curr;
            }
            return len;
        }
    }
}
