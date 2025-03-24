using UnityEngine;

namespace MatchThree.Project.Scripts.Gems
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Gem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
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
            if (spriteRenderer == null) return;
            
            spriteRenderer.sprite = newType.sprite;
            spriteRenderer.color = newType.color;
        }
        
        public void DestroyGem() => Destroy(gameObject);
    }
}