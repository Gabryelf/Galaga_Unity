using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI; // Не забудьте добавить это для работы с UI

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

        private Button butLeftMove; // Кнопка для движения влево
        private Button butRightMove; // Кнопка для движения вправо

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

            // Ищем кнопки на сцене по имени
            butLeftMove = GameObject.Find("LeftButton").GetComponent<Button>();
            butRightMove = GameObject.Find("RightButton").GetComponent<Button>();

            // Назначаем обработчики событий для кнопок
            butLeftMove.onClick.AddListener(() => StartMoveLeft());
            butRightMove.onClick.AddListener(() => StartMoveRight());

        }

        private void Update()
        {
            ProcessInput();

            if (isFreeze == false)
            {
                // Движение при удержании кнопок
                if (isLeftPressed)
                {
                    MoveLeft();
                }
                else if (isRightPressed)
                {
                    MoveRight();
                }
            }
        }

        private void ProcessInput()
        {
            if (IsDead())
                return;

            // Обрабатываем стрельбу
            CurrentShootReloadingTime += Time.deltaTime;
            //if (Input.GetKey(KeyCode.Space) || (CurrentShootReloadingTime > ShootDelay && Input.GetMouseButtonDown(0)))
            if (CurrentShootReloadingTime > ShootDelay && isFreeze == false)
            {
                CurrentShootReloadingTime = 0f;
                SpawnRocket();
            }
        }

        private void StartMoveLeft()
        {
            isLeftPressed = true;
            isRightPressed = false;  // Прекращаем движение вправо
        }

        private void StartMoveRight()
        {
            isRightPressed = true;
            isLeftPressed = false;  // Прекращаем движение влево
        }

        private void MoveLeft()
        {
            if (isFreeze == false)
            {
                Move(-1);
            }
        }

        private void MoveRight()
        {
            if (isFreeze == false)
            {
                Move(1);
            }
        }

        private void Move(int direction)
        {
            var deltaPos = Vector3.right * direction * Speed * Time.deltaTime;
            var newPos = HeroFollowPoint.localPosition + deltaPos;
            newPos.x = Mathf.Clamp(newPos.x, -Edge, Edge); // Ограничиваем движение по оси X
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
            // Это позволит отключать или включать управление кораблем
            enabled = !flag;
        }
    }
}

