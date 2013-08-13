using System;
using System.Collections.Generic;
using System.Text;

namespace MMDIKBakerLibrary.Motion
{
    static class MotionHelper
    {
        internal static Dictionary<string, List<MMDBoneKeyFrame>> SplitBoneMotion(MMDBoneKeyFrame[] keyframes)
        {
            Dictionary<string, List<MMDBoneKeyFrame>> result = new Dictionary<string, List<MMDBoneKeyFrame>>();
            foreach (var keyframe in keyframes)
            {
                if (!result.ContainsKey(keyframe.BoneName))
                    result.Add(keyframe.BoneName, new List<MMDBoneKeyFrame>());
                result[keyframe.BoneName].Add(keyframe);
            }
            foreach (var boneframes in result)
            {
                boneframes.Value.Sort((x, y) => (int)((long)x.FrameNo - (long)y.FrameNo));
            }
            return result;
        }

        internal static Dictionary<string, List<MMDFaceKeyFrame>> SplitFaceMotion(MMDFaceKeyFrame[] keyframes)
        {
            Dictionary<string, List<MMDFaceKeyFrame>> result = new Dictionary<string, List<MMDFaceKeyFrame>>();
            foreach (var keyframe in keyframes)
            {
                if (!result.ContainsKey(keyframe.FaceName))
                    result.Add(keyframe.FaceName, new List<MMDFaceKeyFrame>());
                result[keyframe.FaceName].Add(keyframe);
            }
            foreach (var boneframes in result)
            {
                boneframes.Value.Sort((x, y) => (int)((long)x.FrameNo - (long)y.FrameNo));
            }
            return result;
        }

        internal static MMDBoneKeyFrame[] ImplodeBoneMotion(Dictionary<string, List<MMDBoneKeyFrame>> keyframes)
        {
            List<MMDBoneKeyFrame> result = new List<MMDBoneKeyFrame>();
            Dictionary<string, IEnumerator<MMDBoneKeyFrame>> iterators = new Dictionary<string, IEnumerator<MMDBoneKeyFrame>>();
            foreach (KeyValuePair<string, List<MMDBoneKeyFrame>> keyframe in keyframes)
            {
                iterators.Add(keyframe.Key, keyframe.Value.GetEnumerator());
                iterators[keyframe.Key].MoveNext();
            }
            List<string> endKeys = new List<string>();
            for (uint frame = 0; iterators.Count > 0; ++frame)
            {
                foreach (KeyValuePair<string, IEnumerator<MMDBoneKeyFrame>> it in iterators)
                {
                    if (it.Value.Current.FrameNo <= frame)
                    {
                        result.Add(it.Value.Current);
                        if (!it.Value.MoveNext())
                        {
                            endKeys.Add(it.Key);
                        }
                    }
                }
                foreach (string key in endKeys)
                {
                    iterators.Remove(key);
                }
            }
            return result.ToArray();
        }

        internal static MMDFaceKeyFrame[] ImplodeFaceMotion(Dictionary<string, List<MMDFaceKeyFrame>> keyframes)
        {
            List<MMDFaceKeyFrame> result = new List<MMDFaceKeyFrame>();
            Dictionary<string, IEnumerator<MMDFaceKeyFrame>> iterators = new Dictionary<string, IEnumerator<MMDFaceKeyFrame>>();
            foreach (KeyValuePair<string, List<MMDFaceKeyFrame>> keyframe in keyframes)
            {
                iterators.Add(keyframe.Key, keyframe.Value.GetEnumerator());
                iterators[keyframe.Key].MoveNext();
            }
            List<string> endKeys = new List<string>();
            for (uint frame = 0; iterators.Count > 0; ++frame)
            {
                foreach (KeyValuePair<string, IEnumerator<MMDFaceKeyFrame>> it in iterators)
                {
                    if (it.Value.Current.FrameNo <= frame)
                    {
                        result.Add(it.Value.Current);
                        if (!it.Value.MoveNext())
                        {
                            endKeys.Add(it.Key);
                        }
                    }
                }
                foreach (string key in endKeys)
                {
                    iterators.Remove(key);
                }
            }
            return result.ToArray();
        }
    }
}
