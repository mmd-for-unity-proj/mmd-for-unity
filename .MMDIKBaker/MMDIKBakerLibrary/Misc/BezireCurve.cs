using System;
using System.Collections.Generic;
using System.Text;

namespace MMDIKBakerLibrary.Misc
{
    struct BezierCurve
    {
        internal const float Epsilon = 1.0e-3f;
        
        /// <summary>
        /// ベジェ曲線に用いる点１
        /// </summary>
        public Vector2 v1;
        /// <summary>
        /// ベジェ曲線に用いる点2
        /// </summary>
        public Vector2 v2;


        /// <summary>
        /// 進行度合から移行度合を取得
        /// </summary>
        /// <param name="Progress">進行度合</param>
        /// <returns>移行度合</returns>
        public float Evaluate(float Progress)
        {
            //ニュートン法による近似
            float t = MathHelper.Clamp(Progress, 0f, 1f);
            float dt;
            do
            {
                dt = -(fx(t) - Progress) / dfx(t);
                
                if (float.IsNaN(dt))
                    break;
                t += MathHelper.Clamp(dt, -1f, 1f);//大幅に移動して別の解に到達するのを防止する用
            } while (Math.Abs(dt) > Epsilon);
            return MathHelper.Clamp(fy(t), 0f, 1f);//念のため、0-1の間に収まるようにした
        }
        //fy(t)を計算する関数
        private float fy(float t)
        {
            //fy(t)=(1-t)^3*0+3*(1-t)^2*t*v1.y+3*(1-t)*t^2*v2.y+t^3*1
            return 3 * (1 - t) * (1 - t) * t * v1.Y + 3 * (1 - t) * t * t * v2.Y + t * t * t;
        }
        //fx(t)を計算する関数
        float fx(float t)
        {
            //fx(t)=(1-t)^3*0+3*(1-t)^2*t*v1.x+3*(1-t)*t^2*v2.x+t^3*1
            return 3 * (1 - t) * (1 - t) * t * v1.X + 3 * (1 - t) * t * t * v2.X + t * t * t;
        }
        //dfx/dtを計算する関数
        float dfx(float t)
        {
            //dfx(t)/dt=-6(1-t)*t*v1.x+3(1-t)^2*v1.x-3t^2*v2.x+6(1-t)*t*v2.x+3t^2
            return -6 * (1 - t) * t * v1.X + 3 * (1 - t) * (1 - t) * v1.X
                - 3 * t * t * v2.X + 6 * (1 - t) * t * v2.X + 3 * t * t;
        }
    }
}
