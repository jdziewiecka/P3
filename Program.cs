namespace Task3;

public class Program {
    public static void Main(string[] args) {
        Database database = Database.GetInstance();

        // 1. List people (in alphabetical order by surname, then first name) who do not have a loyalty card.
        // Hints: join...into clause, Count() method
        Console.WriteLine("****** Question 1 ******");
        Console.WriteLine("Provided:");

        var r1 = from x in database.Customers
            join y in database.LoyaltyCards on x.CustomerId equals y.CustomerId into result
            where result.Count() == 0
            orderby x.LastName, x.FirstName
            select x;
        foreach (var item in r1)
        {
            Console.WriteLine($"{item.LastName} {item.FirstName}");
        }
        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Bąk Magdalena");
        Console.WriteLine("Bąk Marek");
        Console.WriteLine("Wiśniewski Artur");

        // 2. Based on the PESEL number, select people who are more than 29 years old, by birth year. Display these people from oldest to youngest.
        // Requirement: do not count the same person multiple times (you may use the let clause)
        // Hint: A PESEL number from 2000 onwards differs in that 20 is added to the month value
        Console.WriteLine();
        Console.WriteLine("****** Question 2 ******");
        Console.WriteLine("Provided:");

        var r2 = from x in database.Customers
            let yy = int.Parse(x.PESEL[0..2])
            let mm = int.Parse(x.PESEL[2..4])
            let century = mm > 20 ? 2000 : 1900
            let birthYear = century + yy
            let age = DateTime.Now.Year - birthYear
            where age > 29
            group x by new {x.LastName, x.FirstName, age } into g
            orderby g.Key.age descending
            select g.Key;
        foreach (var item in r2)
        {
            Console.WriteLine($"{item.LastName} {item.FirstName}  {item.age}");
        }
        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Kowalski Jan 117");
        Console.WriteLine("Marciniak Jerzy 35");
        Console.WriteLine("Antoniewski Michał 32");

        // 3. Determine the 3 people who have been using the store's services the longest.
        Console.WriteLine();
        Console.WriteLine("****** Question 3 ******");
        Console.WriteLine("Provided:");

        var r3 = (from c in database.Customers
                     join o in database.Orders on c.CustomerId equals o.CustomerId into g
                     let earliestDate = g.Min(x => x.Date)
                     orderby earliestDate
                     select new { c.LastName, c.FirstName, Date = earliestDate }).Take(3);

           foreach (var item in r3)
           {
               Console.WriteLine($"{item.LastName} {item.FirstName} {item.Date:dd.MM.yyyy}");
           }

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Nowak Andrzej 02.01.2017");
        Console.WriteLine("Antoniewski Michał 22.03.2017");
        Console.WriteLine("Kowalska Aneta 12.03.2018");

        // 4. Determine the 5 items with the highest total number of ordered units.
        Console.WriteLine();
        Console.WriteLine("****** Question 4 ******");
        Console.WriteLine("Provided:");

        var r4 = (from p in database.Products
                     join oi in database.OrderProducts on p.ProductId equals oi.ProductId
                     group oi by p.Name into g
                     orderby g.Sum(x => x.Quantity) descending
                     select new { Name = g.Key, Total = g.Sum(x => x.Quantity) }).Take(5);

           foreach (var item in r4)
           {
               Console.WriteLine($"{item.Name} {item.Total}");
           }

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Gaming chair 21");
        Console.WriteLine("Marker 11");
        Console.WriteLine("CD 10");
        Console.WriteLine("Pen 10");
        Console.WriteLine("Monitor 5");

        // 5. For each person, determine the total amount they spent in the store (taking into account the discount available to people with a loyalty card).
        // List the customers in alphabetical order by surname, then first name.
        Console.WriteLine();
        Console.WriteLine("****** Question 5 ******");
        Console.WriteLine("Provided:");

        var r5 = from c in database.Customers
            join o in database.Orders on c.CustomerId equals o.CustomerId into orders
            join lc in database.LoyaltyCards on c.CustomerId equals lc.CustomerId into lcs
            from lc in lcs.DefaultIfEmpty()
            let discount = lc != null ? lc.Discount : 0
            let totalSpent = (from o in orders
                join oi in database.OrderProducts on o.OrderId equals oi.OrderId
                join p in database.Products on oi.ProductId equals p.ProductId
                select oi.Quantity * p.Price * (1 - discount)).Sum()
            orderby c.LastName, c.FirstName
            select new {c.LastName, c.FirstName, TotalSpent = totalSpent};
        foreach (var item in r5)
        {
            Console.WriteLine($"{item.LastName} {item.FirstName} {item.TotalSpent}");
        }
        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Antoniewski Michał 1053");
        Console.WriteLine("Bąk Magdalena 2912");
        Console.WriteLine("Bąk Marek 3000");
        Console.WriteLine("Czerwińska Justyna 2593,5");
        Console.WriteLine("Kowalska Aneta 212,5");
        Console.WriteLine("Kowalski Jan 3654");
        Console.WriteLine("Marciniak Jerzy 13600");
        Console.WriteLine("Nowak Alina 1400");
        Console.WriteLine("Nowak Andrzej 2550");
        Console.WriteLine("Wiśniewski Artur 2025");

        Console.WriteLine();
    }
}
