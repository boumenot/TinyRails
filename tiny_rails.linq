<Query Kind="Program">
  <NuGetReference>Combinatorics</NuGetReference>
  <Namespace>Combinatorics.Collections</Namespace>
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <RuntimeVersion>7.0</RuntimeVersion>
</Query>

public static BigInteger Factorial(BigInteger n) {
    BigInteger v = 1;
    for (int i = 1; i <= n; i++) {
        v *= i; 
    }
    return v;
}

public static TimeSpan Elapsed(Action action) {
    var sw = Stopwatch.StartNew();
    action();
    sw.Stop();
    
    return sw.Elapsed;
}

// Read a line of CSV text for *my* TinyRails CSV schema.  Generate a Car based on the
// line and the quantity on the line.
public class TinyRailsCsv {

    public Car[] Cars { get; init; }
    
    // *My* definition of the cargo cars.
    //  1. Take the top cars by amount of cargo they can hold.
    //  2. *My* train is limited to 18 cars.
    public Car[] Cargo => this.Cars
        .Where(x => x.IsCargo())
        .OrderByDescending(x => x.Cargo)
        .Take(18)
        .ToArray();
        
    // *My* definition of the passenger cars.  This is trickier to evaluate.  The focus
    // is on comfort, entertainment, facilities, and food
    //  1. Ignore all Cargo cars.
    public Car[] Passengers => this.Cars
        .Where(x => !x.IsCargo())
        // Marble, Pizza, and Seven Seas are excluded, but should they be?
        .Where(x => x.Passengers > 0)
        .Where(x => x.IsInteresting())
        .OrderByDescending(x => x.AttrsTotal)
        .ThenByDescending(x => x.AttrsCount)
        .ThenByDescending(x => x.Level)
        .ToArray();

    public static TinyRailsCsv Load(string filename) {
        var cars = File.ReadAllLines(filename)
            .Skip(1)
            .Where(x => x.Contains(",Car,"))
            .SelectMany(TinyRailsCsv.Parse)
            .Where(x => x.Include)
            .OrderBy(x => x.Name)
            .ThenByDescending(x => x.Level)
            .ToArray();
            
        return new TinyRailsCsv { Cars = cars };
    }
    
    private static IEnumerable<Car> Parse(string line) {
        var xs = line.Split(',');
        var count = int.Parse(xs[2]);
        for (int i=0; i < count; i++) {
            yield return Car.Create(xs);
        }
    }
}

// PriorityQueue uses the minimum value.  If you want the max value, simply flip the comparison around.
public class InverseComparer : IComparer<double> {
    public int Compare(double x, double y) {
        return y.CompareTo(x);
    }
}

public class Car : IEquatable<Car> {
    private static Regex ModifierRx = new Regex(@"\+(\d+) (\S+)(?: (.*))?");

    public string Name { get; set; }
    public int Level { get; set; }
    public int Speed { get; set; }
    public int Weight { get; set; }
    public int Passengers { get; set; }
    public int Cargo { get; set; }
    public int Food { get; set; }
    public int Comfort { get; set; }
    public int Entertainment { get; set; }
    public int Facilities { get; set; }
    public bool Include { get; set; }
    
    public bool IsCargo() => false
        || this.Name.Contains("Cargo")
        || this.Cargo >= 10
        || (this.AttrsCount == 0 && this.Passengers == 0 && this.Cargo > 0);
    
    // XXX: arbitrary, my arbitrary
    public bool IsInteresting() => this.AttrsCount > 0;
    
    public int AttrsCount => 0
        + (this.Food > 0 ? 1 : 0)
        + (this.Comfort > 0 ? 1 : 0)
        + (this.Entertainment > 0 ? 1 : 0)
        + (this.Facilities > 0 ? 1 : 0);
        
    public int AttrsTotal => this.Food + this.Comfort + this.Entertainment + this.Facilities;
    public int Total => this.Passengers + this.Cargo + this.AttrsTotal;
    
    private static void Update(Car car, string notes) {
        if (string.IsNullOrWhiteSpace(notes)) {
            return;
        }
        
        var m = Car.ModifierRx.Match(notes);
        if (!m.Success) {
            return;
        }
        
        // IGNORE: +2 Food if using Black & Red Diesel Engine & Caboose
        // IGNORE: +4 Entertainment when equipped in Asia
        if (m.Groups[3].ValueSpan.Length > 0) {
            return;
        }

        // MATCH: +5 Facilities
        // MATCH: +5 Passengers
        var value = int.Parse(m.Groups[1].ValueSpan);
        switch (m.Groups[2].ValueSpan) {
            case "Cargo":
                car.Cargo += value;
                break;
            case "Comfort":
                car.Comfort += value;
                break;
            case "Entertainment":
                car.Entertainment += value;
                break;
            case "Facilities":
                car.Facilities += value;
                break;
            case "Food":
                car.Food += value;
                break;
            case "Passengers":
                car.Passengers += value;
                break;
            default:
                throw new ArgumentException(notes);
        }
    }
    
    public static Car Create(string[] xs) {
        var car = new Car {
            Name = xs[0],
            Include = string.IsNullOrWhiteSpace(xs[3]) ? false : int.Parse(xs[3]) == 1,
            Level = int.Parse(xs[4]),
            Speed = int.Parse(xs[5]),
            Weight = int.Parse(xs[6]),
            Passengers = int.Parse(xs[7]),
            Cargo = int.Parse(xs[8]),
            Food = int.Parse(xs[9]),
            Comfort = int.Parse(xs[10]),
            Entertainment = int.Parse(xs[11]),
            Facilities = int.Parse(xs[12]),
        };
        
        Car.Update(car, xs[13]);
        return car;
    }

    public bool Equals(Car other) {
        return this.Name == other.Name && this.Level == other.Level;
    }

    public override bool Equals(object o) {
        var rhs = o as Car;
        return rhs == null ? false : this.Equals(rhs);
    }
    
    public override int GetHashCode() {
        unchecked {
            int hash = 17;
            hash = hash * 23 + this.Name.GetHashCode();
            hash = hash * 23 + this.Level.GetHashCode();
            return hash;
        }
    }
}

public interface IScorer {
    PriorityQueue<int[], double> CreatePQ();
    double Score(Car[] cars);
}

// XXX: this method does not produce good results, use another.
public class TheChrisMethod : IScorer {
    public TheChrisMethod(Car[] cars) {
        this.Count = cars.Length;
        this.MaxCargo = cars.Max(x => x.Cargo);
        this.MaxPassengers = cars.Max(x => x.Passengers);
        this.MaxFood = cars.Max(x => x.Food);
        this.MaxComfort = cars.Max(x => x.Comfort);
        this.MaxEntertainment = cars.Max(x => x.Entertainment);
        this.MaxFacilities = cars.Max(x => x.Facilities);
    }
    
    public double Count { get; set; }
    public int MaxCargo { get; set; }
    public int MaxPassengers { get; set; }
    public int MaxFood { get; set; }
    public int MaxComfort { get; set; }
    public int MaxEntertainment { get; set; }
    public int MaxFacilities { get; set; }
    
    private double Score(Car car) {
        double d = 0.0;
        
        d += (double)car.Cargo / this.MaxCargo;
        d += (double)car.Passengers / this.MaxPassengers;
        
        d += (double)car.Food / this.MaxFood;
        d += (double)car.Comfort / this.MaxComfort;
        d += (double)car.Entertainment / this.MaxEntertainment;
        d += (double)car.Facilities / this.MaxFacilities;
        
        return d;
    }

    public double Score(Car[] cars) {
        return cars.Sum(x => this.Score(x));
    }

    public PriorityQueue<int[], double> CreatePQ() {
        return new PriorityQueue<int[], double>();
    }
}

// XXX: unclear if this is needed.  It should take into account the passenger count
//      for a given car group.
public class Euclidean : IScorer {
    private int clamp;

    public Euclidean(int passenger_count, int car_count) {
        this.clamp = passenger_count * car_count;
    }
    
    public PriorityQueue<int[], double> CreatePQ() => new(new InverseComparer());

    public double Score(Car[] cars) {
        int com = 0;
        int ent = 0;
        int fac = 0;
        int foo = 0;
    
        foreach (var car in cars) {
            com += car.Comfort;
            ent += car.Entertainment;
            fac += car.Facilities;
            foo += car.Food;
        }
        
        double d = 0.0;
        d += Math.Pow(this.clamp - this.Clamp(com), 2);
        d += Math.Pow(this.clamp - this.Clamp(ent), 2);
        d += Math.Pow(this.clamp - this.Clamp(fac), 2);
        d += Math.Pow(this.clamp - this.Clamp(foo), 2);
        
        return Math.Sqrt(d);
    }
    
    private int Clamp(int value) {
        return value <= this.clamp ? value : this.clamp;
    }
}

public class PassengerBonus : IScorer {
    private int passenger_count;

    public PassengerBonus(int passenger_count) {
        this.passenger_count = passenger_count;
    }
    
    public PriorityQueue<int[], double> CreatePQ() => new();

    public double Score(Car[] cars) {
        int com = 0;
        int ent = 0;
        int fac = 0;
        int foo = 0;
        int pas = 0;
    
        foreach (var car in cars) {
            com += car.Comfort;
            ent += car.Entertainment;
            fac += car.Facilities;
            foo += car.Food;
            
            pas += car.Passengers;
        }
        
        int n = 0;
        n += this.Clamp(pas, com);
        n += this.Clamp(pas, ent);
        n += this.Clamp(pas, fac);
        n += this.Clamp(pas, foo);
        
        double total_passenger_expectations = 4.0 * (this.passenger_count + pas);
        
        return n / total_passenger_expectations;
    }
    
    private int Clamp(int car_group_passenger_count, int value) {
        int clamp = car_group_passenger_count + this.passenger_count;    
        return value <= clamp ? value : clamp;
    }
}

public class ScoredCollection {
    public double Score { get; set; }
    public double Bonus { get; set; }
    public Car[] Cars { get; set; }
}

public class Score {
    public int CarCount { get; init; }
    public BigInteger CombinationsTested { get; init; }
    public BigInteger CombinationsSecTested { get; init; }
    public ScoredCollection[] Collection { get; init; }
    public Car[] TopSelectedCars { get; set; }
    
    public static Score Create(
        Car[] cars,
        int passenger_count,
        int car_count,
        PriorityQueue<int[], double> pq,
        TimeSpan elapsed) {
        
        var combs_count = Factorial(cars.Length) / (Factorial(car_count) * Factorial(cars.Length - car_count));
        var combs_sec = (1000 * (combs_count / (long)elapsed.TotalMilliseconds));
        
        var scored_collection = new ScoredCollection[10];
        var top_selected_cars_index = new int[cars.Length];
        
        // Only the top 10 values are considered when reporting.  If there are more than
        // 10 values simply ignore them.
        while (pq.Count > 10) {
            var xs = pq.Dequeue();
            foreach (var x in xs) {
                top_selected_cars_index[x]++;
            }
        }
        
        while (pq.Count > 0) {
            pq.TryDequeue(out int[] xs, out double score);
            foreach (var x in xs) {
                top_selected_cars_index[x]++;
            }
            
            int index = pq.Count;
            
            var group_cars = xs.Select(x => cars[x]).ToArray();
            var total_attrs = group_cars.Sum(x => x.AttrsTotal);
            var group_passenger_count = group_cars.Sum(x => x.Passengers);
            var total_passenger_expectations = 4 * (passenger_count + group_passenger_count);
            
            double bonus = (double)total_attrs / total_passenger_expectations;
            bonus = Math.Round(100 * bonus, 2);
            
            scored_collection[index] = new ScoredCollection {
                Cars = group_cars,
                Score = score,
                Bonus = bonus,
            };
        }
        
        var top_selected_cars = top_selected_cars_index.Select((x, i) => new {
                Car = cars[i],
                Count = x,
            })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Car)
            .Take(50)
            .ToArray();
        
        return new Score {
            CarCount = cars.Length,
            CombinationsTested = combs_count,
            CombinationsSecTested = combs_sec,
            Collection = scored_collection,
            TopSelectedCars = top_selected_cars,
        };
    }
}

public class ScoreBoard {
    private readonly int top;
    private readonly Func<int, int, IScorer> factory;
    
    public ScoreBoard(Func<int, int, IScorer> factory) 
        : this(1000, factory) {
    }
    
    public ScoreBoard(int top, Func<int, int, IScorer> factory) {
        this.top = top;
        this.factory = factory;
    }

    public Score Run(Car[] cars, int passenger_count, int car_count) {
        var scorer = this.factory(passenger_count, car_count);
        var pq = scorer.CreatePQ();
        
        var combs = new Combinations<int>(
            Enumerable.Range(0, cars.Count()).ToArray(),
            car_count);
        
        var elapsed_ms = Elapsed(
            () => this.Do(combs, pq, cars, scorer));
        
        return Score.Create(cars, passenger_count, car_count, pq, elapsed_ms);
    }
    
    private void Do(Combinations<int> combs, PriorityQueue<int[], double> pq, Car[] cars, IScorer scorer) {
        foreach (var row in combs) {
            var xs = row.ToArray();
            var score = scorer.Score(
                xs.Select(x => cars[x]).ToArray());

            pq.Enqueue(xs, score);
            if (pq.Count > this.top) {
                pq.Dequeue();
            }
        }
    }
}

void Main() {
    var tiny_rails = TinyRailsCsv.Load(
        Path.Combine(LINQPad.Util.CurrentQuery.Location, @"tiny_rails.csv"));
        
    //tiny_rails.Cars.Where(x => !x.IsCargo && x.Passengers == 0).Dump("Not Cargo and Not Passengers");
    //tiny_rails.Cargo.Dump("CARGO");
        
    var passenger_cars = tiny_rails.Passengers
        //.Dump("PASSENGERS")
        ;

    const int PASSENGER_COUNT = 47; // Base passenger count + engine passenger count.  Does not include car passenger counts.
    const int CAR_COUNT = 3;        // Number of cars to evaluate.
    const int COMBINATIONS_PER_SEC = 2_750_000; // estimated, see output from a few runs and adjust accordingly
    
    var combos_to_evaluate = Factorial(passenger_cars.Length) / (Factorial(CAR_COUNT) * Factorial(passenger_cars.Length - CAR_COUNT));
    var estimate = TimeSpan.FromSeconds((long)(combos_to_evaluate / COMBINATIONS_PER_SEC));
    Console.WriteLine($"Estimated to complete in {estimate} or around {DateTime.Now.Add(estimate):t}.");

    var score_board1 = new ScoreBoard((x,y) => new Euclidean(x, y));
    var score_board2 = new ScoreBoard((x,y) => new PassengerBonus(PASSENGER_COUNT));

    var score = score_board2.Run(passenger_cars, PASSENGER_COUNT, CAR_COUNT);
    //score.Dump(exclude:"TopSelectedCars");
    score.Dump();
    
    var passenger_cars_set = new HashSet<Car>(passenger_cars);
    passenger_cars_set
        .ExceptWith(score.TopSelectedCars);
    passenger_cars_set
        .OrderBy(x => x.Name)
        .ThenByDescending(x => x.Level)
        .Dump("Cars to Sell");
}
