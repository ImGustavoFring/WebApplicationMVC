using WebApplicationMVC.Models;

namespace WebApplicationMVC.Services
{
    public class VisitTrackerService
    {
        private readonly List<VisitRecord> _visits = new();

        public VisitTrackerService()
        {
            GenerateVisitsForFifteenMonths();
        }

        // Генерация данных для 15 месяцев
        private void GenerateVisitsForFifteenMonths()
        {
            var random = new Random();
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-455)); // 15 месяцев назад

            for (int i = 0; i < 15 * 30; i++) // 15 месяцев по 30 дней
            {
                var randomDay = startDate.AddDays(i);
                var userId = random.Next(1, 101);
                var count = random.Next(1, 5);

                var visit = _visits.FirstOrDefault(v => v.UserId == userId && v.Day == randomDay);

                if (visit == null)
                {
                    _visits.Add(new VisitRecord { UserId = userId, Day = randomDay, Count = count });
                }
                else
                {
                    visit.Count += count;
                }
            }
        }

        // Получение всех записей посещений
        public IEnumerable<VisitRecord> GetVisits() => _visits;

        // Получение посещений по месяцам (для 15 месяцев)
        public IEnumerable<(string Month, int TotalCount)> GetMonthlyVisits()
        {
            // Возьмем все посещения за 15 месяцев
            var relevantVisits = _visits
                .Where(v => v.Day >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-455))) // последние 15 месяцев
                .GroupBy(v => new { v.Day.Year, v.Day.Month })
                .Select(g => ($"{g.Key.Year}-{g.Key.Month:00}", g.Sum(v => v.Count)))
                .OrderBy(d => d.Item1);

            return relevantVisits;
        }

        // Метод для записи посещений
        public void RecordVisit(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var visit = _visits.FirstOrDefault(v => v.UserId == userId && v.Day == today);

            if (visit == null)
            {
                _visits.Add(new VisitRecord { UserId = userId, Day = today, Count = 1 });
            }
            else
            {
                visit.Count++;
            }
        }
    }
}
