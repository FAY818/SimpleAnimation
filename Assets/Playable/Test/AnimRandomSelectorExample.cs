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
        private AnimTransition _AnimTransition; 
        private RandomSelector _randomSelector;

        private void Start()
        {
            _graph = PlayableGraph.Create();

            var anim1 = new AnimPlayer(_graph, clips[0], 0.5f);
            _AnimTransition = new AnimTransition(_graph);
            _randomSelector = new RandomSelector(_graph);

            for (int i = 1; i < clips.Length; i++)
            {
                _randomSelector.AddInput(clips[i], 0.5f);
            }
            _AnimTransition.AddInput(anim1);
            _AnimTransition.AddInput(_randomSelector);

            AnimHelper.SetOutput(_graph, GetComponent<Animator>(), _AnimTransition);
            AnimHelper.Start(_graph);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                _randomSelector.Select(); // 随机选择一个动画
                _AnimTransition.TransitionTo(1); // 过渡到当前的随机动画端口
            }
            isTransition = _AnimTransition.isTransition;
            remainingTime = _randomSelector.remainTime;
            if(!_AnimTransition.isTransition && _randomSelector.remainTime <= 0.5f)
            {
                _AnimTransition.TransitionTo(0); // 过渡到默认动画
            }
        }

        private void OnDisable()
        {
            _graph.Destroy();
        }
    }
}