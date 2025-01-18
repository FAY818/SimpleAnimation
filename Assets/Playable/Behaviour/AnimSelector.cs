using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    /// <summary>
    /// 动画选择,通过索引来播放对应输入源的动画
    /// </summary>
    public class AnimSelector : AdapterBase
    {
        public int currentIndex { get; protected set; } // 当前端口
        
        public int clipCount { get; protected set; } 

        protected AnimationMixerPlayable _mixerPlayable; // 动画混合器
        private List<float> _clipLength;
        private List<float> _clipEnterTime;

        public AnimSelector(PlayableGraph graph) : base(graph)
        {
            _mixerPlayable = AnimationMixerPlayable.Create(graph);
            m_adapterPlayable.AddInput(_mixerPlayable, 0, 1f); // 将自身的自定义可播放项与动画混合器连接

            currentIndex = -1;
            _clipLength = new List<float>();
            _clipEnterTime = new List<float>();
        }
        
        public AnimSelector(PlayableGraph graph, AnimParam param) : this(graph)
        {
            foreach (var clip in param.infoGroup)
            {
                AddInput(clip.clip, clip.enterTime);
            }
        }

        public override void AddInput(Playable playable)
        {
            base.AddInput(playable); // todo 检测是否是调用2遍
            _mixerPlayable.SetInputCount(clipCount + 1);
            _mixerPlayable.ConnectInput(clipCount, playable, 0, 0f);
            clipCount++;
        }
        public void AddInput(AnimationClip clip, float enterTime = 0f)
        {
            _clipLength.Add(clip.length);
            _clipEnterTime.Add(enterTime);
            AddInput(new AnimPlayer(m_adapterPlayable.GetGraph(), clip, enterTime));
        }

        /// <summary>
        /// todo：如果在enable时修改内部索引，可能会导致不能正常断开旧连接
        /// </summary>
        /// <param name="index"></param>
        public virtual void Select(int index)
        {
            currentIndex = index;
        }
        public virtual int Select()
        {
            currentIndex = 0;
            return currentIndex;
        }

        public override void Enable()
        {
            base.Enable();

            if (currentIndex < 0) return;
            
            _mixerPlayable.SetInputWeight(currentIndex, 1f);
            AnimHelper.Enable(_mixerPlayable, currentIndex);

            _mixerPlayable.SetTime(0f);
            _mixerPlayable.Play();
            m_adapterPlayable.SetTime(0f);
            m_adapterPlayable.Play();
        }

        public override void Disable()
        {
            base.Disable();
            
            if (currentIndex < 0 || currentIndex >= clipCount) return;
            _mixerPlayable.SetInputWeight(currentIndex, 0f);
            AnimHelper.Disable(_mixerPlayable, currentIndex);
            currentIndex = -1;
            _mixerPlayable.Pause();
            m_adapterPlayable.Pause();
        }

        public override float GetEnterTime()
        {
            if (currentIndex < 0 || currentIndex >= clipCount) return 0f;

            return _clipEnterTime[currentIndex];
        }

        public override float GetAnimLength()
        {
            if (currentIndex < 0 || currentIndex >= clipCount) return 0f;
            return _clipLength[currentIndex];
        }
    }
}
