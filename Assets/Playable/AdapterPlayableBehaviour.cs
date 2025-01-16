using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    /// <summary>
    /// 自定义的ScriptPlayable<AdapterPlayableBehaviour>对象所持有的行为类
    /// AnimAdapter是对于AnimBehaviour类型的封装，AnimBehaviour通过子类的不同实现表现多态；
    /// 注意AnimAdapter并不是真实的Playable节点，他是父Playable节点的行为类，子Playable节点在AnimBehaviour中创建；
    /// </summary>
    public class AdapterPlayableBehaviour : PlayableBehaviour
    {
        private AdapterBase m_behaviour;

        public void Init(AdapterBase behaviour)
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
            m_behaviour?.OnPrepareFrame(playable, info);
        }

        public T GetAnimBehaviour<T>() where T:AdapterBase
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
