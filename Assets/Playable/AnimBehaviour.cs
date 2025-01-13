using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    /// <summary>
    /// 动画多态节点的基类，可以根据不同的实现类对外表现出不同的行为
    /// 负责创建适配节点，并将自身注入到适配节点中
    /// </summary>
    public abstract class AnimBehaviour
    {
        public bool enable { get; private set; }
        public float remainTime { get; protected set; }
        
        protected Playable m_adapterPlayable; // 统一在基类中构造适配节点，适配节点是真实的Playable节点，区分与AnimAdapter类
        protected float m_enterTime;
        protected float m_animLength;

        public AnimBehaviour(float enterTime = 0f) { m_enterTime = enterTime; }
        public AnimBehaviour(PlayableGraph graph, float enterTime = 0f)
        {
            m_adapterPlayable = ScriptPlayable<AnimAdapter>.Create(graph); // 创建适配节点
            AnimHelper.GetAdapter(m_adapterPlayable).Init(this); // 注入自己到适配节点
            m_enterTime = enterTime;
            m_animLength = float.NaN;
        }

        public virtual void Enable() 
        { 
            if (enable) return;
            enable = true;
            remainTime = GetAnimLength();
        }
        public virtual void Disable() 
        {
            if (!enable) return;
            enable = false; 
        }
        public virtual void Stop() { }
        
        public virtual void Execute(Playable playable, FrameData info)
        {
            if (!enable) return;
            remainTime = remainTime > 0f? remainTime - info.deltaTime: 0f;
        }

        /// <summary>
        /// 添加输入Playable节点
        /// 将一个Playable节点与自身持有的Playable节点相连
        /// </summary>
        /// <param name="playable"></param>
        public virtual void AddInput(Playable playable) { }
        
        /// <summary>
        /// 添加输入AnimBehaviour节点
        /// 将一个AnimBehaviour持有的Playable节点与自身持有的Playable节点相连
        /// </summary>
        /// <param name="behaviour"></param>
        public void AddInput(AnimBehaviour behaviour) 
        {
            AddInput(behaviour.GetAdapterPlayable());
        }

        public virtual Playable GetAdapterPlayable()
        {
            return m_adapterPlayable;
        }

        public virtual float GetEnterTime()
        {
            return m_enterTime;
        }
        public virtual float GetAnimLength()
        {
            return m_animLength;
        }

    }
}
