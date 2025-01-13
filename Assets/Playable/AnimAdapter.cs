using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    /// <summary>
    /// 动画适配器节点，将继承的 PlayableBehaviour 接口和持有的自定义的AnimBehaviour类适配以实现节点的多态；
    /// 注意AnimAdapter并不是真实的Playable节点，真实的Playable节点在AnimBehaviour中创建；
    /// </summary>
    public class AnimAdapter : PlayableBehaviour
    {
        private AnimBehaviour m_behaviour;

        public void Init(AnimBehaviour behaviour)
        {
            m_behaviour = behaviour;
        }
        
        public void Enable()
        {
            m_behaviour?.Enable();
        }

        public void Disable()
        { 
            m_behaviour?.Disable();
        }
        
        /// <summary>
        /// 此方法在每个帧开始时被调用，用于准备播放图（playable）的行为。它允许你在每一帧更新之前执行一些自定义逻辑
        /// </summary>
        /// <param name="playable">当前行为关联的播放图对象</param>
        /// <param name="info"></param>
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            m_behaviour?.Execute(playable, info);
        }

        public T GetAnimBehaviour<T>() where T:AnimBehaviour
        {
            return m_behaviour as T;
        }

        public float GetAnimEnterTime()
        {
            return m_behaviour.GetEnterTime();
        }

        public override void OnGraphStop(Playable playable)
        {
            m_behaviour?.Stop();
        }
    }
}
