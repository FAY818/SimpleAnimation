using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    /// <summary>
    /// 动画行为的基类，可以根据不同的实现对外表现出不同的行为。
    /// 负责创建自定义的脚本适配节点（ScriptPlayable<AdapterPlayableBehaviour>），并将自身注入其行为类中。
    /// 注意区分此类与PlayableBehaviour的区别：
    /// PlayableBehaviour是Playable节点持有的行为类。其中定义的是在Playable不同的生命周期或行为发生时的回调函数。
    /// AdapterBase是AdapterPlayableBehaviour中封装的对象。
    /// </summary>
    public abstract class AdapterBase
    {
        public bool enable { get; private set; }
        public float remainTime { get; protected set; }
        /// <summary>
        /// 每一个适配类都会持有一个Playable节点，用于将自己与其他Playable节点连接
        /// </summary>
        protected Playable m_adapterPlayable;
        /// <summary>
        /// 动画过渡的时间
        /// </summary>
        protected float m_enterTime;
        protected float m_animLength;

        public AdapterBase(float enterTime = 0f)
        {
            m_enterTime = enterTime;
        }
        
        public AdapterBase(PlayableGraph graph, float enterTime = 0f)
        {
            m_adapterPlayable = ScriptPlayable<AdapterPlayableBehaviour>.Create(graph); // 创建适配节点
            AnimHelper.AdapterPlayableBehaviour(m_adapterPlayable).Init(this); // 注入自己到适配节点
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
        
        public virtual void OnPrepareFrame(Playable playable, FrameData info)
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
