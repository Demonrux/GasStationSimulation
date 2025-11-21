namespace GasStation.Core.Utils
{
    public class EconomyStats
    {
        public decimal Balance { get; set; }
        public int TotalCarsProcessed { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Profit { get; set; }
        public decimal RefuellerSalaries { get; set; }
        public decimal CashierSalaries { get; set; }
        public decimal RefuellerSalaryPerCar { get; set; }
        public decimal CashierSalaryPerCar { get; set; }
    }

    public class EconomyManager
    {
        private readonly object _lock = new();
        private decimal _balance;
        private int _totalCarsProcessed;
        private decimal _totalRevenue;
        private decimal _totalExpenses;

        public decimal Balance => _balance;
        public int TotalCarsProcessed => _totalCarsProcessed;
        public decimal TotalRevenue => _totalRevenue;
        public decimal TotalExpenses => _totalExpenses;

        public decimal FuelPurchasePrice { get; }
        public decimal FuelSellPrice { get; }
        public decimal RefuellerSalary { get; }
        public decimal CashierSalary { get; }

        public EconomyManager(decimal fuelPurchasePrice, decimal fuelSellPrice, decimal refuellerSalary, decimal cashierSalary, decimal initialBalance = 0)
        {
            FuelPurchasePrice = fuelPurchasePrice;
            FuelSellPrice = fuelSellPrice;
            RefuellerSalary = refuellerSalary;
            CashierSalary = cashierSalary;
            _balance = initialBalance;
        }

        public void RecordFuelPurchase(int fuelAmount)
        {
            lock (_lock)
            {
                var cost = fuelAmount * FuelPurchasePrice;
                _balance -= cost;
                _totalExpenses += cost;
            }
        }

        public void RecordFuelSale(int fuelAmount, int carId)
        {
            lock (_lock)
            {
                var revenue = fuelAmount * FuelSellPrice;
                _balance += revenue;
                _totalRevenue += revenue;
                _totalCarsProcessed++;
            }
        }

        public void PayRefuellerSalary(int processedCars)
        {
            lock (_lock)
            {
                var salary = processedCars * RefuellerSalary;
                _balance -= salary;
                _totalExpenses += salary;
            }
        }

        public void PayCashierSalary(int processedCars)
        {
            lock (_lock)
            {
                var salary = processedCars * CashierSalary;
                _balance -= salary;
                _totalExpenses += salary;
            }
        }
        public EconomyStats GetStats()
        {
            lock (_lock)
            {
                return new EconomyStats
                {
                    Balance = _balance,
                    TotalCarsProcessed = _totalCarsProcessed,
                    TotalRevenue = _totalRevenue,
                    TotalExpenses = _totalExpenses,
                    Profit = _totalRevenue - _totalExpenses,
                    RefuellerSalaries = _totalCarsProcessed * RefuellerSalary, 
                    CashierSalaries = _totalCarsProcessed * CashierSalary,  
                    RefuellerSalaryPerCar = RefuellerSalary,
                    CashierSalaryPerCar = CashierSalary
                };
            }
        }
    }
}