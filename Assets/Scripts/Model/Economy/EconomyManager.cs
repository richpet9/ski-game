using System;

namespace SkiGame.Model.Economy
{
    public class EconomyManager
    {
        public Action<int> OnMoneyChanged;
        public int Money { get; private set; }

        public bool TrySpendMoney(int amount)
        {
            if (Money >= amount)
            {
                Money -= amount;
                OnMoneyChanged?.Invoke(Money);
                return true;
            }
            return false;
        }

        public void AddMoney(int amount)
        {
            Money += amount;
            OnMoneyChanged?.Invoke(Money);
        }
    }
}
