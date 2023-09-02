# [TinyRails][TinyRails]

`Fun with the game TinyRails`

I play [TinyRails][TinyRailsWiki], and I thought it would be fun to
write a program to help me figure out the optimal train configuration
for passengers.  This is my attempt.

## Requirements

1. [LINQPad v7](https://www.linqpad.net/) - the free version should work fine
1. tiny_rais.csv - CSV file with a list of train cars.


## tiny_rails.csv

tiny_rails.csv is the main input to the script.  There is a sample in
the repository.  The file describe the train cars to be considered
when picking the optimal set.  The file has the following columns in
order from left to right.  This is tedious to create and maintain.
The [Tiny Rails wiki][TinyRailsWiki] has a more complete list, but not
in a format that this script can use.

> NOTE: all values are integers.  TinyRails reports floats and
> integers, but only integers are used here.

| Column        | Description                                                                          |
|---------------|--------------------------------------------------------------------------------------|
| Name          | The name.                                                                            |
| Type          | Car, Engine, or Caboose                                                              |
| Quantity      | The number of cars owned.  Starts at zero.                                           |
| Include       | Include these cars when finding the optimal set, 0 to not include or 1 to include.. |
| Level         | The level of the car - 1, 2, or 3.                                                   |
| Speed         | Speed of the car. Not used.                                                          |
| Weight        | Weight of the car. Not used.                                                         |
| Passengers    | Amount of passengers.                                                                |
| Cargo         | Amount of cargo.                                                                     |
| Food          | Food bonus.                                                                          |
| Comfort       | Comfort bonus.                                                                       |
| Entertainment | Entertainment bonus.                                                                 |
| Facilities    | Facilities bonus.                                                                    |
| Notes         | May be blank.  Enter values exactly.                                                 |

The `Notes` column is parsed by the script to adjust any bonuses, but
the parser is dumb.  It specifically looks for columns with values
like `+5 Entertainment`.  If the value is like `+4 Food when equipped
on Tuesday` it is ignored.

## How it Works

Brute force.

The script tries every combination of cars listed in tiny_rails.csv
and some user values The following controls are exposed, and need to
be set.

1. PASSENGER\_COUNT - set this to the number of passengers your
   train supports without any cars equipped.
1. CAR\_SET\_COUNT - set this to the number of train cars to consider.
1. COMBINATIONS\_PER\_SEC (optional) - **estimate** of combinations
   per second this computer can execute.  Varies by machine.  Execute
   the script a few times, and pick an average.  This script will
   print out an [expected end time](https://en.wikipedia.org/wiki/Halting_problem).

This is a brute force algorithm.  [Factorials][factorial] are used
when considering how many different combinations that need to be
considered.

<center>

$\left(\!
    \begin{array}{c}
      n \\
      r
    \end{array}
  \!\right) = \frac{n!}{r!(n-r)!}$

</center>

The number of combinations to explore quickly becomes untenable.  For
example, on my machine I can evaluate 2,750,000 combinations per
second (roughly).  Lets *plug-and-chug* (ha!) using the above formula
to figure out how long certain values for `n` and `r` would take.

| CAR\_COUNT(n) | CAR\_SET\_COUNT(r) |             Total | Estimate (hh:mm:ss) |
|--------------:|-------------------:|------------------:|--------------------:|
|            50 |                  3 |            19,600 |             0:00:00 |
|            50 |                  7 |        99,884,400 |             0:00:40 |
|            50 |                 10 |       10272278170 |             1:08:29 |
|            50 |                 15 | 2,250,829,575,120 |           250:05:32 |
|            60 |                  7 |       386,206,920 |             0:02:34 |
|            70 |                  7 |     1,198,774,720 |             0:08:00 |
|            80 |                  7 |     3,176,716,400 |             0:21:11 |
|            90 |                  7 |     7,471,375,560 |             0:49:49 |
|           100 |                  7 |    16,007,560,800 |             1:46:43 |

These numbers were randomly picked from an Excel spreadsheet to
illustrate the point that this is a brute force algorithm.  The
CAR\_SET\_COUNT has an outsized impact on the number combinations to
evaluate.  Pick reasonable values if you want answers in a reasonable
amount of time.

CAR\_COUNT is determined based on the CSV file.

1. Type must be `Car`.
1. Include must be **1**.
1. CAR\_COUNT plus the value of the `Quantity` column.  If the
   quantity is 3, then CAR\_COUNT is plus 3.

I recommend you only include high-value cars.  Avoid cars like Cargo
cars, low-level, or cars with mediocre stats, e.g. Old West Passenger
Car.

### Algorithm

The bonus percentage is based on the number of passengers and the four
attributes: food, comfort, entertainment, and facilities.  The goal is
to maximize all four of these values.  If the train can support 50
passengers, the train needs 50 food, 50, comfort, 50 entertainment,
and 50 facilities to get the maximum bonus.

Given a list of cars, the algorithm emits every combination of cars.
Each combination is scored to determine the bonus based on the
passenger count and four attributes.  The cars selected will most
likely increase the PASSENGER\_COUNT.  The algorithm considers bonus
percentage not the dollar amount (another TODO).. The combination is
enqueued into a [priority queue][pq].  Once all of combinations have
been tested, the top 10 combinations are dumped in order from highest
to lowest bonus.

The [priority queue][pq] tracks the top 1,000 combinations.  It
evaluates these 1,000 combinations and add up all of the cars that
appeared in these *winning* combinations.  The list is dumped in order
from most selected to least selected.  The intent is to make it easier
to determine what cars to include or exclude in future runs.  This
will improve the execution time of the algorithm.

## TODO

1. Engines and cabooses are not considered, only cars.  Some engines
   maximize an attribute.
1. The algorithm maximize the bonus percentage, but it should maximize
   the dollar amount per passenger.
1. Combination generation is single threaded, but modern machines have
   multiple cores.  Find a way to divide and conquer across multiple
   threads.  (Seems easy-ish, but probably hard [very hard].)
1. Algorithm is way to brute force, what can be done to reduce the
   search space sooner.
1. Import the Tiny Rails cars from the wiki to make updates a
   **little** less painful.

## Bit Much?

Yes, but it is all in the name of fun! 😆🚂


[factorial]: https://en.wikipedia.org/wiki/Factorial
[pq]: https://en.wikipedia.org/wiki/Priority_queue
[TinyRails]: https://www.tinytitanstudios.com/games/tiny-rails
[TinyRailsWiki]: https://tinyrails.fandom.com/wiki/Tiny_Rails_Wiki
[WikiCars]: https://tinyrails.fandom.com/wiki/Cars
