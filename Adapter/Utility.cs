using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MMD.Adapter
{
    public class Utility
    {
        /// <summary>
        /// MMDの回転をクォータニオンに変換
        /// </summary>
        /// <param name="rotation">MMD回転（YXZ）</param>
        /// <returns>クォータニオン</returns>
        public static Quaternion ToQuaternion(MMD.Format.Common.Vector3 rotation)
        {
            var q = Quaternion.Euler(0, rotation.y * Mathf.Rad2Deg, 0);
            q *= Quaternion.Euler(rotation.x * Mathf.Rad2Deg, 0, 0);
            q *= Quaternion.Euler(0, 0, rotation.z * Mathf.Rad2Deg);
            return q;
        }
    }
}
