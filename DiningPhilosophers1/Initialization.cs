using System.Collections.Generic;
using System.Linq;

namespace philosophers_try2
{
    public class Initialization : List<Philosopher>
    {
        private readonly int _philosopherCount = ConfigValue.Inst.PhilosophersCount;
        private readonly int _forkCount = ConfigValue.Inst.ForksCount;

        private List<Fork> CreateForks()
        {
            // список виделок
            var forks = Enumerable.Range(0, _forkCount)
                                   .Select(name => new Fork(name))
                                   .ToList();
            return forks;
        }

        private void CreatePhilosophers(List<Fork> forks)
        {
            // визначення назви лівої виделки для філософа
            int LeftForkName(int philosopherName) => (_forkCount + philosopherName - 1) % _forkCount;

            // визначення назви правої виделки для філософа
            int RightForkName(int philosopherName) => philosopherName;

            // створюємо кожного філософа та додаємо його до списку філософів
            Enumerable.Range(0, _philosopherCount)
                       .ToList()
                       .ForEach(name => Add(new Philosopher(name, forks[LeftForkName(name)], forks[RightForkName(name)], this)));
        }

        public Initialization CreatePhilosophersAndForks()
        {
            var forks = CreateForks();
            CreatePhilosophers(forks);
            return this;
        }
    }
}
