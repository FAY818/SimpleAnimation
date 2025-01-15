using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    /// <summary>
    /// 动画节点的行为类，可以根据不同的实现类对外表现出不同的行为
    /// 负责创建适配节点，并将自身注入到适配节点中
    /// 注意区分此类与PlayableBehaviour的区别：
    /// PlayableBehaviour是Playable节点持有的行为类。其中定义的是在Playable不同的生命周期或行为发生时的回调函数。
    /// AnimBehaviour是AnimAdapter（PlayableBehaviour）中封装的行为类
    /// </summary>
    public abstract class AdapterBase
    {
        public bool enable { get; private set; }
        public float remainTime { get; protected set; }
        protected Playable m_adapterPlayable;
        protected float m_enterTime;
        protected float m_animLength;

        public AdapterBase(float enterTime = 0f)
        {
            m_enterTime = enterTime;
        }
        
        public AdapterBase(PlayableGraph graph, float enterTime = 0f)
        {
            m_adapterPlayable = ScriptPlayable<AdapterPlayableBehaviour>.Create(graph); // 创建适配节点
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
            remainTime = remainTime > 0f? remainTime - info.deltaTime: 0f; // 动画的剩余时间 - 帧间隔时间
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
        public void AddInput(AdapterBase behaviour) 
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
