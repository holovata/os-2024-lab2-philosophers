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
        private readonly Initialization _allPhilosophers;
        private readonly Random _random;
        private PhilosopherStatus Status { get; set; } = PhilosopherStatus.Thinking;

        public Philosopher(int name, Fork leftFork, Fork rightFork, Initialization allPhilosophers)
		{
			Name = name;
			LeftFork = leftFork;
			RightFork = rightFork;
            _random = new Random(Name); // used to assign eating time
            _allPhilosophers = allPhilosophers;
		}

        private static readonly object locker = new object(); // об'єкт-локер для синхронізації доступу до ресурсів
        /*Використання блокування гарантує, що лише один потік може виконувати
         * операції взяття і покладання виделок одночасно*/
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
            lock (locker)
            {
                LeftFork.PutDown();
                RightFork.PutDown();
                Status = PhilosopherStatus.Thinking;
                Console.WriteLine($"<<< Філософ {Name} поклав виделки {LeftFork.Name} та {RightFork.Name}");
            }
        }

		public int EatingTimesCount { get; private set; }
		public int TotalEatingTime { get; private set; }

        private IEnumerable<Philosopher> PhilosophersEatingNow()
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

        private readonly int _maxThinkDuration = ConfigValue.Inst.MaxThinkDuration;//визначає верхню межу випадково згенерованої тривалості часу в мілісекундах, яку філософ буде проводити в роздумах.
        private readonly int _minThinkDuration = ConfigValue.Inst.MinThinkDuration;//визначає нижню межу цієї випадкової тривалості часу.
        //визначають максимальну та мінімальну тривалість часу, яку філософ проводить у стані роздумів перед тим, як розпочати їсти.

        private void Eat()
        {
            //генерація випадкової тривалості часу роздумів. якщо _minThinkDuration = 1000 (1 секунда), а _maxThinkDuration = 3000 (3 секунди),
            //то філософ буде розмірковувати випадкову кількість часу від 1 до 3 секунд перед тим, як почати їсти
            var eatingDuration = _random.Next(_maxThinkDuration) + _minThinkDuration;// тривалість прийому їжі випадково генерується

            var eatingPhilosophers = PhilosophersEatingNow().Select(p => p.Name).ToList();
            Console.WriteLine($"||| Філософ {Name} їсть                           " +
                              $"Їдять {eatingPhilosophers.Count} філософів: " +
                              $"{string.Join(", ", eatingPhilosophers.Select(p => $"{p}"))}");

            Thread.Sleep(eatingDuration); //імітація процесу обіду

            Console.WriteLine($"??? Філософ {Name} їв" +
                              $" {eatingDuration} мілісекунд і тепер думає");

            EatingTimesCount++;
            TotalEatingTime += eatingDuration;
        }

        //обмеження к-ті філософів, які їдять одночасно
        static readonly SemaphoreSlim GetEatingPermissionSlip = new SemaphoreSlim(ConfigValue.Inst.MaxPhilsophersEatingSimultaneously);

        public void DiningProcess(CancellationToken stopDining)
        {
            // після отримання дозволу на їжу філософ буде чекати протягом durationBeforeRequstEatPermission перед наступним запитом на дозвіл на їжу
            var timeBeforeRequestingEatPermission = ConfigValue.Inst.DurationBeforeAskingPermissionToEat;

            int i = 0;
            while (true)
            {
                // якщо викликаюча процедура просить зупинити обід
                if (stopDining.IsCancellationRequested)
                {
                    try
                    {
                        stopDining.ThrowIfCancellationRequested();
                    }
                    catch (OperationCanceledException)
                    {
                        // Обработка исключения, если операция была отменена
                        Console.WriteLine($"          Філософ {Name} припиняє обід.");
                        break; 
                    }
                }

                try
                {
                    // очікування дозволу на їжу; після отримання дозволу на їжу філософ повинен перевірити доступність лівої та правої виделок
                    GetEatingPermissionSlip.WaitAsync().Wait();
                    Console.WriteLine($"/// Філософ {Name} пробує почати їсти");

                    bool isReadyToEat;
                    lock (locker)
                    {
                        isReadyToEat = AreBothForksAvailable();
                        if (isReadyToEat)
                            GrabForks();
                    }
                    if (isReadyToEat)
                    {
                        Eat();
                        PutDownForks();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"!!!ПОМИЛКА!!!    Філософ {Name} створив помилку {ex.Message} " +
                                      $" {new string('.', 20)}");
                    throw;
                }
                finally
                {
                    //закінчив їсти
                    GetEatingPermissionSlip.Release();
                }

                // очікування протягом timeBeforeRequestingEatPermission перед наступним запитом на дозвіл на їжу
                Task.Delay(timeBeforeRequestingEatPermission).Wait();
                i++;
            }
        }

    }
}