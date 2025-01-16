using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    public class RandomSelectDebug : MonoBehaviour
    {
        public AnimationClip[] clips;

        private PlayableGraph _graph;
        private RandomSelector _randomSelector;

        private void Start()
        {
            _graph = PlayableGraph.Create();
            
            _randomSelector = new RandomSelector(_graph);

            for (int i = 0; i < clips.Length; i++)
            {
                _randomSelector.AddInput(clips[i], 0.5f);
            }
            
            AnimHelper.SetOutput(_graph, GetComponent<Animator>(), _randomSelector);
            AnimHelper.Start(_graph);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _randomSelector.Disable();
                _randomSelector.Select(); // 随机选择一个动画
                _randomSelector.Enable();
            }
        }

        private void OnDisable()
        {
            _graph.Destroy();
        }
    }
}