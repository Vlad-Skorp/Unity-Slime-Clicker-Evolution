using UnityEngine;


namespace SlimeRpgEvolution2D.Data
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Config/Entities/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        public string enemyName;
        public int maxHealth;
        public int goldReward;
        public Sprite enemySprite;
    }
}