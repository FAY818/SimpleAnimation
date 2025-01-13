using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    [System.Serializable]
    public struct BlendClip2D
    {
        public AnimationClip clip;
        public Vector2 pos;
    }

    public class BlendTree2D : AnimBehaviour
    {
        private struct DataPair
        {
            public float x; // 混合坐标x
            public float y; // 混合坐标y
            public float output; // 计算输出动画权重
        }

        private AnimationMixerPlayable m_mixer;
        private Vector2 m_pointer;
        private float m_total;
        private int m_clipCount;

        private ComputeShader m_computeShader; // 计算权重的shader
        private ComputeBuffer m_computeBuffer; // 传递数据
        private DataPair[] m_datas;
        private int m_kernel;
        private int m_pointerX;
        private int m_pointerY;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="clips"></param>
        /// <param name="enterTime"></param>
        /// <param name="eps">防止分母为0，当其作为分母时，权重经过标准归一化处理会是1</param>
        public BlendTree2D(PlayableGraph graph, BlendClip2D[] clips, float enterTime = 0f, float eps = 1e-5f): base(graph, enterTime)
        {
            m_datas = new DataPair[clips.Length];

            m_mixer = AnimationMixerPlayable.Create(graph);
            m_adapterPlayable.AddInput(m_mixer, 0, 1f);
            for (int i = 0; i < clips.Length; i++)
            {
                m_mixer.AddInput(AnimationClipPlayable.Create(graph, clips[i].clip), 0);
                m_datas[i].x = clips[i].pos.x;
                m_datas[i].y = clips[i].pos.y;
            }

            m_computeShader = AnimHelper.GetComputer("Blend2D");
            m_computeBuffer = new ComputeBuffer(clips.Length, 12); // 4的倍数
            m_kernel = m_computeShader.FindKernel("Compute"); // shader中定义的计算主函数
            m_computeShader.SetBuffer(m_kernel, "dataBuffer", m_computeBuffer);
            m_computeShader.SetFloat("eps", eps);
            m_pointerX = Shader.PropertyToID("pointerX");
            m_pointerY = Shader.PropertyToID("pointerY");
            m_clipCount = clips.Length;
            m_pointer.Set(1, 1);
            SetPointer(0, 0);

            Disable();
        }
        public BlendTree2D(PlayableGraph graph, AnimParam param) : this(graph, param.blendClip, param.enterTime) { }

        public void SetPointer(Vector2 vector)
        {
            SetPointer(vector.x, vector.y);
        }
        public void SetPointer(float x, float y)
        {
            if(m_pointer.x == x && m_pointer.y == y)
            {
                return;
            }
            m_pointer.Set(x, y);

            int i;
            m_computeShader.SetFloat(m_pointerX, x);
            m_computeShader.SetFloat(m_pointerY, y);

            m_computeBuffer.SetData(m_datas);
            m_computeShader.Dispatch(m_kernel, m_clipCount, 1, 1);
            m_computeBuffer.GetData(m_datas);
            for (i = 0; i < m_clipCount; i++)
            {
                m_total += m_datas[i].output;
            }
            for (i = 0; i < m_clipCount; i++)
            {
                m_mixer.SetInputWeight(i, m_datas[i].output / m_total);
            }
            m_total = 0f;
        }

        public override void Enable()
        {
            base.Enable();

            SetPointer(0, 0);
            for (int i = 0; i < m_clipCount; i++)
            {
                m_mixer.GetInput(i).Play();
                m_mixer.GetInput(i).SetTime(0f);
            }
            m_mixer.SetTime(0f);
            m_mixer.Play();
            m_adapterPlayable.SetTime(0f);
            m_adapterPlayable.Play();

            //m_animLength = ((AnimationClipPlayable)m_mixer.GetInput(0)).GetAnimationClip().length;
        }

        public override void Disable()
        {
            base.Disable();
            for (int i = 0; i < m_clipCount; i++)
            {
                m_mixer.GetInput(i).Pause();
            }
            m_mixer.Pause();
            m_adapterPlayable.Pause();
        }

        /// <summary>
        /// 释放 ComputeShader
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            m_computeBuffer.Dispose();
        }
    }
}
