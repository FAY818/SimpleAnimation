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
    
    public class BlendTree2D : AdapterBase
    {
        /// <summary>
        /// 对应shader中的计算结构
        /// </summary>
        private struct DataPair
        {
            public float x; // 对应BlendClip2D.pos.x
            public float y; // 对应BlendClip2D.pos.y
            public float output; // 输出动画权重
        }

        private AnimationMixerPlayable _mixer;
        /// <summary>
        /// 当前坐标位置
        /// </summary>
        private Vector2 _pointer;
        /// <summary>
        /// 所有输入源动画的权重和
        /// </summary>
        private float _total;
        private int _clipCount;

        /// <summary>
        /// 计算权重的shader
        /// </summary>
        private ComputeShader _computeShader;
        /// <summary>
        /// 与动画相关的计算数据结构
        /// </summary>
        private DataPair[] _datas;
        /// <summary>
        /// 与shader传递数据的缓冲区
        /// </summary>
        private ComputeBuffer _computeBuffer;
        /// <summary>
        /// shader中的计算主函数Compute
        /// </summary>
        private int _kernel;
        private int _pointerX;
        private int _pointerY;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="clips">自定义的动画结构，包含AnimationClip以及对应的坐标</param>
        /// <param name="enterTime"></param>
        /// <param name="eps">防止分母为0，当其作为分母时，权重经过标准归一化处理会是1</param>
        public BlendTree2D(PlayableGraph graph, BlendClip2D[] clips, float enterTime = 0f, float eps = 1e-5f): base(graph, enterTime)
        {
            _datas = new DataPair[clips.Length];

            _mixer = AnimationMixerPlayable.Create(graph);
            m_adapterPlayable.AddInput(_mixer, 0, 1f);
            for (int i = 0; i < clips.Length; i++)
            {
                _mixer.AddInput(AnimationClipPlayable.Create(graph, clips[i].clip), 0);
                _datas[i].x = clips[i].pos.x;
                _datas[i].y = clips[i].pos.y;
            }

            _computeShader = AnimHelper.GetComputer("Blend2D");
            _computeBuffer = new ComputeBuffer(clips.Length, 12); // 4的倍数
            _kernel = _computeShader.FindKernel("Compute");
            _computeShader.SetBuffer(_kernel, "dataBuffer", _computeBuffer);
            _computeShader.SetFloat("eps", eps);
            _pointerX = Shader.PropertyToID("pointerX");
            _pointerY = Shader.PropertyToID("pointerY");
            _clipCount = clips.Length;
            _pointer.Set(1, 1);
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
            if(_pointer.x == x && _pointer.y == y)
            {
                return;
            }
            _pointer.Set(x, y);

            int i;
            _computeShader.SetFloat(_pointerX, x);
            _computeShader.SetFloat(_pointerY, y);

            _computeBuffer.SetData(_datas);
            _computeShader.Dispatch(_kernel, _clipCount, 1, 1);
            _computeBuffer.GetData(_datas);
            for (i = 0; i < _clipCount; i++)
            {
                _total += _datas[i].output;
            }
            for (i = 0; i < _clipCount; i++)
            {
                _mixer.SetInputWeight(i, _datas[i].output / _total);
            }
            _total = 0f;
        }

        public override void Enable()
        {
            base.Enable();

            SetPointer(0, 0);
            for (int i = 0; i < _clipCount; i++)
            {
                _mixer.GetInput(i).Play();
                _mixer.GetInput(i).SetTime(0f);
            }
            _mixer.SetTime(0f);
            _mixer.Play();
            m_adapterPlayable.SetTime(0f);
            m_adapterPlayable.Play();
        }

        public override void Disable()
        {
            base.Disable();
            for (int i = 0; i < _clipCount; i++)
            {
                _mixer.GetInput(i).Pause();
            }
            _mixer.Pause();
            m_adapterPlayable.Pause();
        }

        /// <summary>
        /// 释放 ComputeShader
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            _computeBuffer.Dispose();
        }
    }
}
