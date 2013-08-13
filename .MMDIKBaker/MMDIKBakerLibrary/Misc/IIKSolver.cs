using System;
using System.Collections.Generic;
using System.Text;
using MMDIKBakerLibrary.Model;

namespace MMDIKBakerLibrary.Misc
{
    interface IIKSolver
    {
        /// <summary>
        /// IKのソルブ
        /// </summary>
        /// <param name="ik">対象IK</param>
        /// <param name="BoneManager">ボーンマネージャ</param>
        /// <returns>呼び出し側でUpdateGlobalをもう一度呼ぶ場合はtrue</returns>
        bool Solve(MMDIK ik, MMDBoneManager BoneManager);
    }
}
