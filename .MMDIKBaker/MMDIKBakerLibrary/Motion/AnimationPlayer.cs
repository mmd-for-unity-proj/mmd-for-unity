using System;
using System.Collections.Generic;
using System.Text;
using MMDIKBakerLibrary.Model;
using MMDIKBakerLibrary.Misc;

namespace MMDIKBakerLibrary.Motion
{
    class AnimationPlayer
    {
        private MMDBoneManager boneManager;
        private MMDMotionTrack motionTrack;
        private Dictionary<string, SQTTransform> resultPoses = new Dictionary<string, SQTTransform>();
        private List<string> underIKBones = new List<string>();
        Dictionary<string, SQTTransform> BindPoses;
        public AnimationPlayer(MMDBoneManager boneManager)
        {
            this.boneManager = boneManager;
            BindPoses = new Dictionary<string, SQTTransform>();
            for (int i = 0; i < boneManager.Count; ++i)
            {
                BindPoses.Add(boneManager[i].Name, boneManager[i].BindPose);
            }
        }

        public void SetMotion(MMDMotion motionData)
        {
            this.motionTrack = new MMDMotionTrack(motionData);
        }

        public bool Update()
        {
            resultPoses.Clear();
            bool result = motionTrack.Update();
            foreach (KeyValuePair<string, SQTTransform> subpose in motionTrack.SubPoses)
            {
                resultPoses[subpose.Key] = subpose.Value;
                SQTTransform bindPose, sub = subpose.Value, local;
                if (BindPoses.TryGetValue(subpose.Key, out bindPose))
                {
                    SQTTransform.Multiply(ref sub, ref bindPose, out local);
                    boneManager[subpose.Key].LocalTransform = local;
                }
            }
            return result;
        }
        
        
    }
}
