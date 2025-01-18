using UnityEngine;
using UnityEngine.Playables;

namespace PlayableUtil.AnimationSystem
{
    public class RandomSelector : AnimSelector
    {
        public RandomSelector(PlayableGraph graph) : base(graph)
        {
            
        }

        public override int Select()
        {
            if (clipCount <= 1)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex = Random.Range(0, clipCount);
            }
            
            return currentIndex;
        }
    } 
}
