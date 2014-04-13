using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MMD;
using UnityEngine;

namespace MMD
{
    namespace VMD
    {
        // 任意の型のvalueを持つキーフレーム
        public abstract class CustomKeyframe<Type>
        {
            public CustomKeyframe(float time, Type value)
            {
                this.time = time;
                this.value = value;
            }
            public float time { get; set; }
            public Type value { get; set; }

            // ベジェハンドルを取得する
            // 0～127の値を 0f～1fとして返す
            static Vector2 GetBezierHandle(byte[] interpolation, int type, int ab)
            {
                // 0=X, 1=Y, 2=Z, 3=R
                // abはa?かb?のどちらを使いたいか
                Vector2 bezierHandle = new Vector2((float)interpolation[ab * 8 + type], (float)interpolation[ab * 8 + 4 + type]);
                return bezierHandle / 127f;
            }

            // p0:(0f,0f),p3:(1f,1f)のベジェ曲線上の点を取得する
            // tは0～1の範囲
            static Vector2 SampleBezier(Vector2 bezierHandleA, Vector2 bezierHandleB, float t)
            {
                Vector2 p0 = Vector2.zero;
                Vector2 p1 = bezierHandleA;
                Vector2 p2 = bezierHandleB;
                Vector2 p3 = new Vector2(1f, 1f);

                Vector2 q0 = Vector2.Lerp(p0, p1, t);
                Vector2 q1 = Vector2.Lerp(p1, p2, t);
                Vector2 q2 = Vector2.Lerp(p2, p3, t);

                Vector2 r0 = Vector2.Lerp(q0, q1, t);
                Vector2 r1 = Vector2.Lerp(q1, q2, t);

                Vector2 s0 = Vector2.Lerp(r0, r1, t);
                return s0;
            }

            // 補間曲線が線形補間と等価か
            public static bool IsLinear(byte[] interpolation, int type)
            {
                byte ax = interpolation[0 * 8 + type];
                byte ay = interpolation[0 * 8 + 4 + type];
                byte bx = interpolation[1 * 8 + type];
                byte by = interpolation[1 * 8 + 4 + type];
                return (ax == ay) && (bx == by);
            }

            protected static void AddKeyframe<T>(byte[] interpolation, int type, T prev_keyframe, T cur_keyframe, int interpolationQuality, ref T[] keyframes, ref int index)
                where T : CustomKeyframe<Type>
            {
                if (prev_keyframe == null || IsLinear(interpolation, type))
                {
                    keyframes[index++] = cur_keyframe;
                }
                else
                {
                    SamplingBezierKeyframes(interpolation, type, interpolationQuality, ref keyframes, ref index, prev_keyframe, cur_keyframe);
                }
            }

            private static void SamplingBezierKeyframes<T>(byte[] interpolation, int type, int interpolationQuality, ref T[] keyframes, ref int index, T prev_keyframe, T cur_keyframe)
                where T : CustomKeyframe<Type>
            {
                Vector2 bezierHandleA = GetBezierHandle(interpolation, type, 0);
                Vector2 bezierHandleB = GetBezierHandle(interpolation, type, 1);
                int sampleCount = interpolationQuality;
                for (int j = 0; j < sampleCount; j++)
                {
                    AddingSampledBezierKeyframe(j, sampleCount, bezierHandleA, bezierHandleB, ref keyframes, ref index, prev_keyframe, cur_keyframe);
                }
            }

            public abstract CustomKeyframe<Type> Lerp(CustomKeyframe<Type> to, Vector2 t);

            private static void AddingSampledBezierKeyframe<T>(int j, int sampleCount, Vector2 bezierHandleA, Vector2 bezierHandleB, ref T[] keyframes, ref int index, T prev_keyframe, T cur_keyframe)
                where T : CustomKeyframe<Type>
            {
                float t = (j + 1) / (float)sampleCount;
                Vector2 sample = SampleBezier(bezierHandleA, bezierHandleB, t);
                keyframes[index++] = (T)prev_keyframe.Lerp(cur_keyframe, sample);
            }
        }

        // float型のvalueを持つキーフレーム
        public class FloatKeyframe : CustomKeyframe<float>
        {
            public FloatKeyframe(float time, float value)
                : base(time, value)
            {
            }

            public override CustomKeyframe<float> Lerp(CustomKeyframe<float> to, Vector2 t)
            {
                return new FloatKeyframe(
                    Mathf.Lerp(time, to.time, t.x),
                    Mathf.Lerp(value, to.value, t.y));
            }

            // ベジェを線形補間で近似したキーフレームを追加する
            public static void AddBezierKeyframes(byte[] interpolation, int type,
                FloatKeyframe prev_keyframe, FloatKeyframe cur_keyframe, int interpolationQuality,
                ref FloatKeyframe[] keyframes, ref int index)
            {
                AddKeyframe<FloatKeyframe>(interpolation, type, prev_keyframe, cur_keyframe, interpolationQuality, ref keyframes, ref index);
            }
        }

        // Quaternion型のvalueを持つキーフレーム
        public class QuaternionKeyframe : CustomKeyframe<Quaternion>
        {
            public QuaternionKeyframe(float time, Quaternion value)
                : base(time, value)
            {
            }

            public override CustomKeyframe<Quaternion> Lerp(CustomKeyframe<Quaternion> to, Vector2 t)
            {
                return new QuaternionKeyframe(
                    Mathf.Lerp(time, to.time, t.x),
                    Quaternion.Slerp(value, to.value, t.y));
            }

            // ベジェを線形補間で近似したキーフレームを追加する
            public static void AddBezierKeyframes(byte[] interpolation, int type,
                QuaternionKeyframe prev_keyframe, QuaternionKeyframe cur_keyframe, int interpolationQuality,
                ref QuaternionKeyframe[] keyframes, ref int index)
            {
                AddKeyframe<QuaternionKeyframe>(interpolation, type, prev_keyframe, cur_keyframe, interpolationQuality, ref keyframes, ref index);
            }
        }
    }
}