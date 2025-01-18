using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    public class AnimRandomSelectorExample : MonoBehaviour
    {
        public bool isTransition;
        public float remainingTime;
        public AnimationClip[] clips;

        private PlayableGraph _graph;
        private AnimMixer m_AnimMixer; 
        private RandomSelector _randomSelector;

        private void Start()
        {
            _graph = PlayableGraph.Create();

            var anim1 = new AnimPlayer(_graph, clips[0], 0.5f);
            m_AnimMixer = new AnimMixer(_graph);
            _randomSelector = new RandomSelector(_graph);

            for (int i = 1; i < clips.Length; i++)
            {
                _randomSelector.AddInput(clips[i], 0.5f);
            }
            m_AnimMixer.AddInput(anim1);
            m_AnimMixer.AddInput(_randomSelector); 

            AnimHelper.SetOutput(_graph, GetComponent<Animator>(), m_AnimMixer);
            AnimHelper.Start(_graph);
        }

        private void Update()
        {
            isTransition = m_AnimMixer.IsTransition;
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if (m_AnimMixer.CurrentIndex == 1)
                {
                    _randomSelector.Disable();
                    _randomSelector.Select();
                    _randomSelector.Enable();
                }
                else
                {
                    // 如果运行时连续点击空格，频繁调用一下方法会造成_randomSelector的索引被修改，这会造成权重设置的混乱，需要以上if分支
                    _randomSelector.Select();
                    m_AnimMixer.TransitionTo(1); // 过渡到当前的随机动画端口
                }
            }
            
            isTransition = m_AnimMixer.IsTransition;
            remainingTime = _randomSelector.RemainTime;
            if(!m_AnimMixer.IsTransition && _randomSelector.RemainTime <= 0.5f)
            {
                // 不在过渡中，并且随机动画播放时间小于0.5f
                m_AnimMixer.TransitionTo(0);
            }
        }

        private void OnDisable()
        {
            _graph.Destroy();
        }
    }
}