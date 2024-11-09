using System; // Подключаем пространство имен System для Action
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Galaga.Game
{
    public class HeroController : MonoBehaviour
    {
        public float Edge;
        public float Speed;
        public float ShootDelay = 0.9f;
        public float Damage;

        public bool isFreeze;

        private float CurrentShootReloadingTime;
        public Transform HeroSpawnPoint;
        public Transform HeroFollowPoint;
        public GameObject Ship { get; private set; }
        private GameProcessor _gameProcessor;

        private const float ExplosionDelay = 0.6f;
        private const float RespawnDelay = 1.5f;
        private const float ExplParticlesDuration = 2f;
        private const float RocketLifeTime = 2f;

        private Button butLeftMove;
        private Button butRightMove;
        private bool isLeftPressed = false;
        private bool isRightPressed = false;

        void Awake()
        {
            _gameProcessor = GetComponentInParent<GameProcessor>();
            Assert.IsNotNull(_gameProcessor);
        }

        void Start()
        {
            Speed = _gameProcessor.GetShipConfiguration().Speed;
            ShootDelay = _gameProcessor.GetShipConfiguration().ShootDelay;
            Damage = _gameProcessor.GetShipConfiguration().Damage;

            SpawnShip();

            // Находим кнопки на сцене по имени
            butLeftMove = GameObject.Find("LeftButton").GetComponent<Button>();
            butRightMove = GameObject.Find("RightButton").GetComponent<Button>();

            // Назначаем обработчики для событий нажатия и отпускания кнопок
            AddButtonEvents(butLeftMove, () => isLeftPressed = true, () => isLeftPressed = false);
            AddButtonEvents(butRightMove, () => isRightPressed = true, () => isRightPressed = false);
        }

        private void Update()
        {
            ProcessInput();

            if (!isFreeze)
            {
                if (isLeftPressed)
                    MoveLeft();
                else if (isRightPressed)
                    MoveRight();
            }
        }

        private void ProcessInput()
        {
            if (IsDead())
                return;

            CurrentShootReloadingTime += Time.deltaTime;
            if (CurrentShootReloadingTime > ShootDelay && !isFreeze)
            {
                CurrentShootReloadingTime = 0f;
                SpawnRocket();
            }
        }

        private void MoveLeft()
        {
            Move(-1);
        }

        private void MoveRight()
        {
            Move(1);
        }

        private void Move(int direction)
        {
            var deltaPos = Vector3.right * direction * Speed * Time.deltaTime;
            var newPos = HeroFollowPoint.localPosition + deltaPos;
            newPos.x = Mathf.Clamp(newPos.x, -Edge, Edge);
            HeroFollowPoint.localPosition = newPos;
        }

        private void SpawnRocket()
        {
            var rocket = Factory.Create("RocketRed", _gameProcessor.Projectiles,
                Ship.transform.position, RocketLifeTime).GetComponent<Rocket>();
            rocket.Enemies = _gameProcessor.Monsters;
            rocket.Damage = Damage;
            AudioManager.Instance.PlaySound("1");
        }

        private void SpawnShip()
        {
            Ship = Instantiate(PrefabHolder.Instance.Entities["Ship"], HeroSpawnPoint) as GameObject;
            Assert.IsNotNull(Ship);
            HeroFollowPoint.localPosition = new Vector3(0, HeroFollowPoint.localPosition.y, HeroFollowPoint.localPosition.z);
            Ship.GetComponent<Follower>().SetTarget(HeroFollowPoint);
        }

        public Vector3 GetHeroApproxPosition()
        {
            return HeroFollowPoint.position;
        }

        public bool IsDead()
        {
            return Ship == null;
        }

        public void ExplodeShip()
        {
            if (IsDead())
                return;
            StartCoroutine(ExplodingShip());
        }

        private IEnumerator ExplodingShip()
        {
            Factory.Create("Explosion", _gameProcessor.Effects, Ship.transform.position, ExplosionDelay);
            Factory.Create("PfxBoom", _gameProcessor.Effects, Ship.transform.position, ExplParticlesDuration);
            Destroy(Ship);
            AudioManager.Instance.PlaySound("12");

            yield return new WaitForSeconds(RespawnDelay);

            _gameProcessor.Lives--;
            if (_gameProcessor.Lives >= 0)
                SpawnShip();
        }

        public void Freeze(bool flag)
        {
            enabled = !flag;
        }

        // Вспомогательный метод для добавления событий нажатия и отпускания
        private void AddButtonEvents(Button button, Action onPress, Action onRelease)
        {
            var eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener(_ => onPress());
            eventTrigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener(_ => onRelease());
            eventTrigger.triggers.Add(pointerUp);
        }
    }
}


