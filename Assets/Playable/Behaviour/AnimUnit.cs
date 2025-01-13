using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    /// <summary>
    /// 动画播放单元
    /// </summary>
    public class AnimUnit : AnimBehaviour
    {
        private AnimationClipPlayable m_animClip;

        public AnimUnit(PlayableGraph graph, AnimationClip clip, float enterTime = 0f) : base(graph, enterTime)
        {
            m_animClip = AnimationClipPlayable.Create(graph, clip);
            m_animLength = clip.length;
            m_adapterPlayable.AddInput(m_animClip, 0, 1f); // 连接动画节点和适配器节点
            Disable();
        }
        public AnimUnit(PlayableGraph graph, AnimParam param) : this(graph, param.clip, param.enterTime) { }

        public override void Enable()
        {
            base.Enable();
            m_animClip.SetTime(0f);
            m_adapterPlayable.SetTime(0f);
            m_animClip.Play();
            m_adapterPlayable.Play();
        }

        public override void Disable()
        {
            base.Disable();
            m_animClip.Pause();
            m_adapterPlayable.Pause();
        }
    }
}
