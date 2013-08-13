using System;
using System.Collections.Generic;
using System.Text;
using MMDIKBakerLibrary.Model;

namespace MMDIKBakerLibrary.Misc
{
    interface IIKLimitter
    {
        /// <summary>
        /// 制限の適用
        /// </summary>
        /// <param name="bone">対象となるボーン</param>
        void Adjust(MMDBone bone);

        /// <summary>
        /// 回転軸制限の適用
        /// </summary>
        /// <param name="boneName">対象となるボーン名</param>
        /// <param name="rotationAxis">回転軸</param>
        void Adjust(string boneName, ref Vector3 rotationAxis);
    }
}
