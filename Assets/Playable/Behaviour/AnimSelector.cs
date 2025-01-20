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
        public int CurrentIndex { get; protected set; } // 当前端口
        
        public int ClipCount { get; protected set; } 

        protected AnimationMixerPlayable _mixerPlayable; // 动画混合器
        private List<float> _clipLength;
        private List<float> _clipEnterTime;

        public AnimSelector(PlayableGraph graph) : base(graph)
        {
            _mixerPlayable = AnimationMixerPlayable.Create(graph);
            m_adapterPlayable.AddInput(_mixerPlayable, 0, 1f); // 将自身的自定义可播放项与动画混合器连接

            CurrentIndex = -1;
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
            base.AddInput(playable);
            _mixerPlayable.SetInputCount(ClipCount + 1);
            _mixerPlayable.ConnectInput(ClipCount, playable, 0, 0f);
            ClipCount++;
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
            CurrentIndex = index;
        }
        public virtual int Select()
        {
            CurrentIndex = 0;
            return CurrentIndex;
        }

        public override void Enable()
        {
            base.Enable();

            if (CurrentIndex < 0) return;
            
            _mixerPlayable.SetInputWeight(CurrentIndex, 1f);
            AnimHelper.Enable(_mixerPlayable, CurrentIndex);

            _mixerPlayable.SetTime(0f);
            _mixerPlayable.Play();
            m_adapterPlayable.SetTime(0f);
            m_adapterPlayable.Play();
        }

        public override void Disable()
        {
            base.Disable();
            
            if (CurrentIndex < 0 || CurrentIndex >= ClipCount) return;
            _mixerPlayable.SetInputWeight(CurrentIndex, 0f);
            AnimHelper.Disable(_mixerPlayable, CurrentIndex);
            CurrentIndex = -1;
            _mixerPlayable.Pause();
            m_adapterPlayable.Pause();
        }

        public override float GetEnterTime()
        {
            if (CurrentIndex < 0 || CurrentIndex >= ClipCount) return 0f;

            return _clipEnterTime[CurrentIndex];
        }

        public override float GetAnimLength()
        {
            if (CurrentIndex < 0 || CurrentIndex >= ClipCount) return 0f;
            return _clipLength[CurrentIndex];
        }
    }
}
