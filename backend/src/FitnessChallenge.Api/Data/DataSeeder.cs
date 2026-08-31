using FitnessChallenge.Api.Models;
using FitnessChallenge.Api.Services;

namespace FitnessChallenge.Api.Data;

public static class DataSeeder
{
    private static readonly string[] FirstNames =
    {
        "Marko", "Ana", "Ivan", "Petra", "Luka", "Marija", "Josip", "Ivana",
        "Filip", "Sara", "Nikola", "Katarina", "Tomislav", "Dora", "Antonio",
        "Lucija", "Domagoj", "Elena", "Karlo", "Nina", "Matej", "Klara",
        "Stjepan", "Lara", "Mislav",
    };

    private static readonly string[] LastNames =
    {
        "Kovacevic", "Horvat", "Babic", "Juric", "Novak", "Knezevic", "Markovic",
        "Vukovic", "Petrovic", "Matic", "Tomic", "Blazevic", "Grgic", "Pavic",
        "Radic", "Simic", "Bosnjak", "Kovac", "Vidovic", "Peric", "Barisic",
        "Lovric", "Filipovic", "Brkic", "Crnkovic",
    };

    private static readonly Sport[] DistanceSports = { Sport.Running, Sport.Walking, Sport.Cycling };
    private static readonly Sport[] DurationSports = { Sport.Gym, Sport.Swimming };

    public static void Seed(AppDbContext db, IScoringService scoringService)
    {
        if (db.Users.Any())
        {
            return;
        }

        var random = new Random(20260831);
        var now = DateTime.UtcNow;

        for (var i = 0; i < FirstNames.Length; i++)
        {
            var user = new User
            {
                FirstName = FirstNames[i],
                LastName = LastNames[i],
            };

            var activityCount = random.Next(8, 31);
            for (var j = 0; j < activityCount; j++)
            {
                user.Activities.Add(CreateRandomActivity(random, now, scoringService));
            }

            db.Users.Add(user);
        }

        db.SaveChanges();
    }

    private static Activity CreateRandomActivity(Random random, DateTime now, IScoringService scoringService)
    {
        var daysAgo = random.Next(0, 75);
        var secondsIntoDay = random.Next(0, 86_400);
        var when = now.Date.AddDays(-daysAgo).AddSeconds(secondsIntoDay);

        var roll = random.Next(0, 10);

        Sport? sport = null;
        decimal? distance = null;
        string? duration = null;
        int? steps = null;

        if (roll < 5)
        {
            sport = DistanceSports[random.Next(DistanceSports.Length)];
            distance = Math.Round((decimal)(random.NextDouble() * 29 + 1), 1);
        }
        else if (roll < 8)
        {
            sport = DurationSports[random.Next(DurationSports.Length)];
            duration = $"{random.Next(5, 91)}:{random.Next(0, 60):00}";
        }
        else
        {
            steps = random.Next(800, 28_000);
        }

        var points = scoringService.CalculatePoints(sport, distance, duration, steps);

        return new Activity
        {
            DateTimeUtc = when,
            Sport = sport,
            Distance = distance,
            Duration = duration,
            Steps = steps,
            Points = points,
        };
    }
}
