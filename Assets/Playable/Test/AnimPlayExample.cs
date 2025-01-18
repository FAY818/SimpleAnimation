using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    public class AnimPlayExample : MonoBehaviour
    {
        public AnimationClip clip;

        private PlayableGraph m_graph;
        private AnimPlayer m_AnimPlayer;

        // Use this for initialization
        void Start()
        {
            m_graph = PlayableGraph.Create();
            m_AnimPlayer = new AnimPlayer(m_graph, clip);
            AnimHelper.SetOutput(m_graph, GetComponent<Animator>(), m_AnimPlayer);
            AnimHelper.Start(m_graph);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if (m_AnimPlayer.IsEnable)
                    m_AnimPlayer.Disable();
                else
                    m_AnimPlayer.Enable();
            }
        }

        private void OnDisable()
        {
            m_graph.Destroy();
        }
    }
}