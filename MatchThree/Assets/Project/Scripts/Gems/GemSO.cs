using UnityEngine;

namespace MatchThree.Project.Scripts.Gems
{
    [CreateAssetMenu(fileName = "RegularGem", menuName = "Gems/Regular Gem")]
    public class GemSO : ScriptableObject
    {
        public Sprite sprite;
        public Color color;
        public int score;
    }
}