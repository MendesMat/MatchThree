using UnityEngine;

namespace MatchThree.Project.Scripts.Gems
{
    public class Gem : MonoBehaviour
    {
        private GemSO _gemType;
        
        public GemSO GetGemType() => _gemType;
        public void SetGemType(GemSO newType)
        {
            if (newType == null) return;
            
            _gemType = newType;
            SetSprite(newType);
        }

        private void SetSprite(GemSO newType)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
            
            spriteRenderer.sprite = newType.sprite;
            spriteRenderer.color = newType.color;
        }
    }
}