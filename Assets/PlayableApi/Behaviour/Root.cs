﻿using UnityEngine.Playables;

namespace IrisFenrir.AnimationSystem
{
    /// <summary>
    /// 根节点，用以隔离输出与播放节点，避免直接播放playable输入。
    /// </summary>
    public class Root : AnimBehaviour
    {
        public Root(PlayableGraph graph) : base(graph)
        {

        }

        public override void Enable()
        {
            base.Enable();
            for (int i = 0; i < m_adapterPlayable.GetInputCount(); i++)
            {
                AnimHelper.Enable(m_adapterPlayable.GetInput(i));
            }
            m_adapterPlayable.SetTime(0f);
            m_adapterPlayable.Play();
        }

        public override void Disable()
        {
            base.Disable();
            for (int i = 0; i < m_adapterPlayable.GetInputCount(); i++)
            {
                AnimHelper.Disable(m_adapterPlayable.GetInput(i));
            }
            m_adapterPlayable.Pause();
        }

        public override void AddInput(Playable playable)
        {
            m_adapterPlayable.AddInput(playable, 0, 1f);
        }
    }
}
