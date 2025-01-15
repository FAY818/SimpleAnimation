using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    /// <summary>
    /// 单个动画播放
    /// </summary>
    public class AnimPlayer : AdapterBase
    {
        private AnimationClipPlayable _animClipPlayable; // 动画可播放项

        public AnimPlayer(PlayableGraph graph, AnimationClip clip, float enterTime = 0f) : base(graph, enterTime)
        {
            _animClipPlayable = AnimationClipPlayable.Create(graph, clip);
            m_animLength = clip.length;
            m_adapterPlayable.AddInput(_animClipPlayable, 0, 1f); // 连接自定义可播放项与动画可播放项
            Disable();
        }
        
        /// <summary>
        /// 通过ScriptObj中定义的AnimParam构造
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="param"></param>
        public AnimPlayer(PlayableGraph graph, AnimParam param) : this(graph, param.clip, param.enterTime) { }

        public override void Enable()
        {
            base.Enable();
            _animClipPlayable.SetTime(0f);
            m_adapterPlayable.SetTime(0f);
            _animClipPlayable.Play();
            m_adapterPlayable.Play();
        }

        public override void Disable()
        {
            base.Disable();
            _animClipPlayable.Pause();
            m_adapterPlayable.Pause();
        }
    }
}
