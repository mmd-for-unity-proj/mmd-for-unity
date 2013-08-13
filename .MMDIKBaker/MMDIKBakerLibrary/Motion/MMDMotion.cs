using System;
using System.Collections.Generic;
using System.Text;

namespace MMDIKBakerLibrary.Motion
{
    class MMDMotion
    {
        /// <summary>
        /// ボーンモーションデータ
        /// </summary>
        /// <remarks>ボーンごとに時系列順</remarks>
        public Dictionary<string, List<MMDBoneKeyFrame>> BoneFrames;
        /// <summary>
        /// フェイスモーションデータ
        /// </summary>
        /// <remarks>表情ごとに時系列順</remarks>
        public Dictionary<string, List<MMDFaceKeyFrame>> FaceFrames;
    }
}
