using System.Collections.Generic;
using System.Linq;

namespace philosophers_try2
{
    public class Philosophers : List<Philosopher>
    {
        private readonly int _philosopherCount = ConfigValue.Inst.PhilosopherCount;
        private readonly int _forkCount = ConfigValue.Inst.ForkCount;

        public Philosophers InitializePhilosophers()
        {
            var forks = InitializeForks();
            InitializePhilosopher(forks);
            return this;
        }

        private List<Fork> InitializeForks()
        {
            // список виделок за допомогою LINQ
            var forks = Enumerable.Range(0, _forkCount)
                                   .Select(name => new Fork(name))
                                   .ToList();
            return forks;
        }

        private void InitializePhilosopher(List<Fork> forks)
        {
            // визначення назви лівої виделки для філософа
            int LeftForkName(int phName) => (_forkCount + phName - 1) % _forkCount;

            // визначення назви правої виделки для філософа
            int RightForkName(int phName) => phName;

            // створюємо кожного філософа та додаємо його до списку філософів
            Enumerable.Range(0, _philosopherCount)
                       .ToList()
                       .ForEach(name => Add(new Philosopher(name, forks[LeftForkName(name)], forks[RightForkName(name)], this)));
        }
    }
}
