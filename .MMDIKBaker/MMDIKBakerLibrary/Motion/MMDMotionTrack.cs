using System;
using System.Collections.Generic;
using System.Text;
using MMDIKBakerLibrary.Misc;

namespace MMDIKBakerLibrary.Motion
{
    class MMDMotionTrack
    {
        /// <summary>
        /// デフォルトモーション再生FPS
        /// </summary>
        public const decimal DefaultFPS = 30m;
        decimal m_NowFrame = 0;
        decimal m_MaxFrame = 0;
        //モーションデータ
        Dictionary<string, List<MMDBoneKeyFrame>> boneFrames;
        //モーションデータの読み出し位置
        Dictionary<string, int> bonePos = new Dictionary<string, int>();
        //トラックから抽出されたボーンの差分一覧
        Dictionary<string, SQTTransform> subPoses;
        /// <summary>
        /// モーション再生用FPS
        /// </summary>
        public decimal FramePerSecond { get; set; }
        /// <summary>
        /// 現在のボーン差分一覧
        /// </summary>
        public Dictionary<string, SQTTransform> SubPoses { get { return subPoses; } }
        
        public MMDMotionTrack(MMDMotion motionData)
        {
            //ボーンの配列抜き出し
            boneFrames = motionData.BoneFrames;
            //モーションのFPS=30
            FramePerSecond = DefaultFPS;
            //差分一覧を作成
            subPoses = new Dictionary<string, SQTTransform>(motionData.BoneFrames.Count);
            //現在の再生位置を設定&最大フレーム数のチェック
            foreach (KeyValuePair<string,List<MMDBoneKeyFrame>> it in motionData.BoneFrames)
            {
                bonePos.Add(it.Key, 0);
                foreach (MMDBoneKeyFrame it2 in it.Value)
                {
                    if (it2.FrameNo > m_MaxFrame)
                        m_MaxFrame = it2.FrameNo;
                }
            }
        }

        //終了したらfalseを返す
        public bool Update()
        {
            bool result = !TimeUpdate();
            SubPoses.Clear();
            //ボーンの更新
            foreach (KeyValuePair<string, List<MMDBoneKeyFrame>> frameList in boneFrames)
            {
                //カーソル位置の更新
                int CursorPos = bonePos[frameList.Key];
                for (; CursorPos < frameList.Value.Count && frameList.Value[CursorPos].FrameNo < m_NowFrame; ++CursorPos) ;
                for (; CursorPos > 0 && frameList.Value[CursorPos - 1].FrameNo > m_NowFrame; --CursorPos) ;
                bonePos[frameList.Key] = CursorPos;
                if (CursorPos == frameList.Value.Count)
                {//通常再生時の最終フレーム
                    SQTTransform subPose;
                    frameList.Value[CursorPos - 1].GetSQTTransform(out subPose);
                    SubPoses.Add(frameList.Key, subPose);
                }
                else
                {
                    //時間経過取得
                    decimal Progress = (m_NowFrame - frameList.Value[CursorPos - 1].FrameNo) / (frameList.Value[CursorPos].FrameNo - frameList.Value[CursorPos - 1].FrameNo);
                    SQTTransform subPose;
                    MMDBoneKeyFrame pose1 = frameList.Value[CursorPos - 1], pose2 = frameList.Value[CursorPos];
                    MMDBoneKeyFrame.Lerp(pose1, pose2, Progress, out subPose);
                    SubPoses.Add(frameList.Key, subPose);
                }
            }
            return result;
        }
        private bool TimeUpdate()
        {
            decimal elapsedSeconds = 1m / 30m;
            bool result = false;
            m_NowFrame += elapsedSeconds * FramePerSecond;
            if (m_NowFrame > m_MaxFrame)
            {
                result = true;
                m_NowFrame = m_MaxFrame;
            }
            return result;
        }
    }
}
