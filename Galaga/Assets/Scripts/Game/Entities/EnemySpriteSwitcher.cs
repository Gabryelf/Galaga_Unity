using System.Collections;
using UnityEngine;

namespace Galaga.Game
{
    public class EnemySpriteSwitcher : MonoBehaviour
    {
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite alternateSprite;

        private SpriteRenderer spriteRenderer;
        private BaseEnemy enemy;
        private Coroutine spriteSwitchCoroutine;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            enemy = GetComponent<BaseEnemy>();
        }

        private void OnEnable()
        {
            StartSpriteSwitcher();
        }

        private void OnDisable()
        {
            StopSpriteSwitcher();
        }

        private void StartSpriteSwitcher()
        {
            if (spriteSwitchCoroutine != null)
                StopCoroutine(spriteSwitchCoroutine);

            spriteSwitchCoroutine = StartCoroutine(SwitchSpriteRoutine());
        }

        private void StopSpriteSwitcher()
        {
            if (spriteSwitchCoroutine != null)
            {
                StopCoroutine(spriteSwitchCoroutine);
                spriteSwitchCoroutine = null;
            }
        }

        private IEnumerator SwitchSpriteRoutine()
        {
            while (true)
            {
                switch (enemy.CurrentState)  // Используем CurrentState для проверки состояния
                {
                    case BaseEnemy.State.Waiting:
                    case BaseEnemy.State.StayOnGrid:
                        spriteRenderer.sprite = defaultSprite;
                        spriteRenderer.flipY = false;  // Отключаем FlipY в этих состояниях
                        break;

                    case BaseEnemy.State.Circling:
                        spriteRenderer.sprite = alternateSprite;
                        spriteRenderer.flipY = true;  // Включаем FlipY в состоянии Circling
                        break;

                    case BaseEnemy.State.FlyToHero:
                    case BaseEnemy.State.FlyToBottom:
                        spriteRenderer.sprite = defaultSprite;
                        spriteRenderer.flipY = true;  // Включаем FlipY в состояниях FlyToHero и FlyToBottom
                        yield return new WaitForSeconds(0.25f);
                        spriteRenderer.sprite = alternateSprite;
                        yield return new WaitForSeconds(0.25f);
                        break;
                }

                yield return null;
            }
        }


    }
}


