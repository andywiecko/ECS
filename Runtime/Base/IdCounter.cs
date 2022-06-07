using andywiecko.BurstCollections;

namespace andywiecko.ECS
{
    public class IdCounter<T>
    {
        private int count = 0;
        public Id<T> GetNext() => (Id<T>)count++;
    }
}