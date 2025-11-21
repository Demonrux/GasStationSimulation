namespace GasStation.Core.Models
{
    public class FuelTank
    {
        private readonly object _lock = new();
        private int _currentLevel;

        public int CurrentLevel { get { lock (_lock) return _currentLevel; } }
        public int Capacity { get; }
        public int TotalFuelDelivered = 0;

        public event Action<int> FuelLevelChanged;

        public FuelTank(int capacity, int initialLevel)
        {
            Capacity = capacity;
            _currentLevel = Math.Clamp(initialLevel, 0, capacity);
        }

        public bool TryReserveFuel(int amount)
        {
            lock (_lock)
            {
                if (_currentLevel < amount)
                    return false;
        
                _currentLevel -= amount;
                FuelLevelChanged?.Invoke(_currentLevel);
                return true;
            }
        }

        public void Refuel(int amount)
        {
            lock (_lock)
            {
                var oldLevel = _currentLevel;
                _currentLevel = Math.Clamp(_currentLevel + amount, 0, Capacity);

                if (_currentLevel > oldLevel)
                { 
                    TotalFuelDelivered += (_currentLevel - oldLevel);
                    FuelLevelChanged?.Invoke(_currentLevel);
                }
            }
        }

        public bool HasEnoughFuel(int requiredAmount)
        {
            lock (_lock) return _currentLevel >= requiredAmount;
        }
    }
}
