using WebApplicationMVC.Models;

namespace WebApplicationMVC.Services
{
    public class VisitTrackerService
    {
        private readonly List<VisitRecord> _visits = new();

        public VisitTrackerService()
        {
            GenerateRandomVisits();
        }

        private void GenerateRandomVisits()
        {
            var random = new Random();
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-365));

            for (int i = 0; i < 10000; i++)
            {
                var randomDay = startDate.AddDays(random.Next(0, 365));
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

        public IEnumerable<VisitRecord> GetVisits() => _visits;

        public IEnumerable<(DateOnly Day, int TotalCount)> GetDailyVisits()
        {
            return _visits
                .GroupBy(v => v.Day)
                .Select(g => (g.Key, g.Sum(v => v.Count)))
                .OrderBy(d => d.Key);
        }

        public IEnumerable<(string Month, int TotalCount)> GetMonthlyVisits()
        {
            return _visits
                .GroupBy(v => new { v.Day.Year, v.Day.Month })
                .Select(g => ($"{g.Key.Year}-{g.Key.Month:00}", g.Sum(v => v.Count)))
                .OrderBy(d => d.Item1);
        }
    }
}
