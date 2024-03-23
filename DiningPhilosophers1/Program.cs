using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace philosophers_try2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = UTF8Encoding.UTF8;

            var philosophers = new Initialization().CreatePhilosophersAndForks();
            var eatingTasks = new List<Task>();

            // використання токена скасування для управління процесом обіду
            using (var stopDiningTokenSource = new CancellationTokenSource()) //створення токена
            {
                var stopDiningToken = stopDiningTokenSource.Token;

                // створення задачі для кожного філософа
                foreach (var philosopher in philosophers)
                    eatingTasks.Add(
                        // запуск DiningProcess кожного філософа у власному потоці з використанням токена скасування
                        Task.Factory.StartNew(() => philosopher.DiningProcess(stopDiningToken), stopDiningToken)
                            // Обробка помилок
                            .ContinueWith(_ => {
                                Console.WriteLine($"!!!Помилка!!!    Філософ {philosopher.Name} втратив привілеї обіду");
                            }, TaskContinuationOptions.OnlyOnFaulted)
                            // Обробка відміни задачі
                            .ContinueWith(_ => {
                                Console.WriteLine($"             Філософ {philosopher.Name} покинув столик");
                            }, TaskContinuationOptions.OnlyOnCanceled)
                    );

                // дозвіл пообідати протягом певного часу
                Task.Delay(ConfigValue.Inst.DinnerDuration).Wait();

                try
                {
                    // по закінченню часу, відміна усіх задач і очікування їх завершення
                    stopDiningTokenSource.Cancel();
                    Task.WaitAll(eatingTasks.ToArray());
                }
                catch (AggregateException ae)
                {
                    foreach (var ex in ae.Flatten().InnerExceptions)
                        Console.WriteLine($"{ex.GetType().Name}:  {ex.Message}");
                }
            }
            Console.WriteLine();
            Console.WriteLine("Кінець обіду. СТАТИСТИКА:");

            // статистика
            
            var totalEatCount = philosophers.Sum(p => p.EatingTimesCount);
            var totalEatingTime = philosophers.Sum(p => p.TotalEatingTime);
            foreach (var philosopher in philosophers)
                Console.WriteLine($"Філософ {philosopher.Name} пообідав {philosopher.EatingTimesCount,3} разів; " +
                    $"Сумарний час обіду філософа: {philosopher.TotalEatingTime:#,##0} мілісекунд; "
                    //+ $"Кількість конфліктів: {philosopher.EatingConflictCount}.");
                    );
            Console.WriteLine($"Разом філософи пообідали {totalEatCount} разів; " +
                $"Загальний час обіду: {totalEatingTime:#,##0} мілісекунд; " /*+
                $"Кількість конфліктів: {totalEatingConflicts}"*/);

            Console.WriteLine();
            Console.WriteLine("Натисніть будь-яку клавішу для виходу");
            Console.ReadKey();
        }
    }
}
