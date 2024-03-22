using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace philosophers_try2
{
    public enum PhilosopherStatus { Thinking, Eating };
    public class Philosopher
	{
        public int Name { get; }
        private Fork LeftFork { get; }
        private Fork RightFork { get; }
        private readonly Philosophers _allPhilosophers;
        private readonly Random _random;

        public Philosopher(int name, Fork leftFork, Fork rightFork, Philosophers allPhilosophers)
		{
			Name = name;
			LeftFork = leftFork;
			RightFork = rightFork;
            _random = new Random(Name); // used to assign eating time
            _allPhilosophers = allPhilosophers;
		}

        private static readonly object locker = new object(); // об'єкт-локер для синхронізації доступу до ресурсів
        private PhilosopherStatus Status { get; set; } = PhilosopherStatus.Thinking;

        void GrabForks()
        {
            lock (locker) // блокування для забезпечення взаємовиключності
            {
                LeftFork.PickUp(this);
                RightFork.PickUp(this);
                Status = PhilosopherStatus.Eating;
                Console.WriteLine($">>> Філософ {Name} взяв виделки {LeftFork.Name} та {RightFork.Name}");
            }
        }

        void PutDownForks()
        {
            lock (locker) // блокування для забезпечення взаємовиключності
            {
                LeftFork.PutDown();
                RightFork.PutDown();
                Status = PhilosopherStatus.Thinking;
                Console.WriteLine($"<<< Філософ {Name} поклав виделки {LeftFork.Name} та {RightFork.Name}");
            }
        }

        private readonly int _maxThinkDuration = ConfigValue.Inst.MaxThinkDuration;
        private readonly int _minThinkDuration = ConfigValue.Inst.MinThinkDuration;

        static readonly SemaphoreSlim AquireEatPermissionSlip = new SemaphoreSlim(ConfigValue.Inst.MaxPhilsophersToEatSimultaneously);

		// How many times thinking permission was granted but one of the needed forks was not available
		public int EatingConflictCount { get; private set; }

		// How many times this philosopher was given a go ahead to eat
		public int EatCount { get; private set; }

		// Total duration of eating in milliseconds
		public int TotalEatingTime { get; private set; }

        private IEnumerable<Philosopher> PhilosphersEatingNow()
        {
            lock (locker)
                return _allPhilosophers.Where(p => p.Status == PhilosopherStatus.Eating);
        }

        private bool AreBothForksAvailable()
        {
            lock (locker)
            {
                if (LeftFork.IsBeingUsed)
                {
                    Console.WriteLine($"--- Філософ {Name} не може їсти, " +
                    $"тому що ліва виделка ({LeftFork.Name}) використовується Філософом {LeftFork.BeingUsedBy.Name}");
                    return false;
                }

                if (RightFork.IsBeingUsed)
                {
                    Console.WriteLine($"--- Філософ {Name} не може їсти, " +
                    $"тому що права виделка ({RightFork.Name}) використовується Філософом {RightFork.BeingUsedBy.Name}");
                    return false;
                }
            }
            // обидві виделки доступні => філософ може розпочати їсти
            return true;
        }

        private void Eat()
        {
            var eatingDuration = _random.Next(_maxThinkDuration) + _minThinkDuration;// тривалість прийому їжі випадково генерується

            var eatingPhilosophers = PhilosphersEatingNow().Select(p => p.Name).ToList();
            Console.WriteLine($"||| Філософ {Name} їсть.                           " +
                              $"Їдять {eatingPhilosophers.Count} філософів: " +
                              $"{string.Join(", ", eatingPhilosophers.Select(p => $"{p}"))}");

            Thread.Sleep(eatingDuration);

            Console.WriteLine($"??? Філософ {Name} думає " +
                              $" {eatingDuration} мілісекунд");

            EatCount++;
            TotalEatingTime += eatingDuration;
        }

        public void DiningProcess(CancellationToken stopDining)
        {
            // після отримання дозволу на їжу філософ буде чекати протягом durationBeforeRequstEatPermission перед наступним запитом на дозвіл на їжу
            var durationBeforeRequstEatPermission = ConfigValue.Inst.DurationBeforeAskingPermissionToEat;

            int i = 0;
            while (true)
            {
                // якщо викликаюча процедура просить зупинити обід
                if (stopDining.IsCancellationRequested)
                {
                    Console.WriteLine($"            Філософ {Name} ПРОСИТЬ ЗУПИНИТИ ОБІД");
                    stopDining.ThrowIfCancellationRequested();
                }

                try
                {
                    // очікування дозволу на їжу; після отримання дозволу на їжу філософ повинен перевірити доступність лівої та правої виделок
                    AquireEatPermissionSlip.WaitAsync().Wait();
                    Console.WriteLine($"/// Філософ {Name} пробує їсти (Спроба №: {i})");

                    bool isOkToEat;
                    lock (locker)
                    {
                        isOkToEat = AreBothForksAvailable();
                        if (isOkToEat)
                            GrabForks();
                    }

                    if (isOkToEat)
                    {
                        Eat();
                        PutDownForks();
                    }
                    else
                        ++EatingConflictCount;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"!!!ПОМИЛКА!!!    Філософ {Name} створив помилку {ex.Message} " +
                                      $" {new string('.', 20)}");
                    throw;
                }
                finally
                {
                    AquireEatPermissionSlip.Release();
                }

                // очікування протягом durationBeforeRequstEatPermission перед наступним запитом на дозвіл на їжу
                Task.Delay(durationBeforeRequstEatPermission).Wait();
                i++;
            }
        }

    }
}