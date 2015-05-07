using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuoteEntity
{
    class Program
    {
        #region Properties & Parameters

        public struct Quote
        {
            public long Id { get; set;}
            public string CustomerName	{get; set;}
            public decimal Amount {get; set;}
            public DateTime CreatedAt { get; set; }
            public int ThreadID { get; set; }
        }

        public struct Calculation
        {
            public decimal min;
            public decimal max;
            public decimal sum;
            public decimal avg;
        }

        // will be used to be able to add items to list objects
        private static object locker = new object();
        #endregion

        #region Process
        
        static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now;
            Console.WriteLine("[QuoteEntity] Start of Quote Entity");
            
            // try to get argument as count
            int count;
            if (args.Length == 0)
            {
                System.Console.WriteLine("No numeric argument found. Count is set to 1 million");
                count = 1000000;
            }
            else
            {
                int.TryParse(args[0], out count);
            }

            // Divide the count of quotes with processor count of local computer
            // to accelerate the process 
            Console.WriteLine("The number of processors on this computer is '{0}'", Environment.ProcessorCount);
            int[] caller = new int[Environment.ProcessorCount];

            for (int i = 0; i < caller.Length; i++)
            {
                caller[i] = count / caller.Length;
            }

            List<Quote> allQuotes = new List<Quote>();
            Calculation calcution = new Calculation();

            // 1. Generate 1 million random quotes
            // 2. Load them to a list
            // 3. Find sum, max and min from them
            // Operations will be done in parallel programming using Parallel.ForEach
            Parallel.ForEach(caller, index =>
                {
                    List<decimal> amounts = null;
                    AddToGlobalList(ref allQuotes, QuoteGenerator(index, out amounts), amounts, ref calcution);
                }
            );

            // 3. Find the average
            calcution.avg = calcution.sum / allQuotes.Count;

            Console.WriteLine("Min = {0}, Max = {1}, Sum = {2}, Avg = {3}", calcution.min, calcution.max, calcution.sum, calcution.avg);
            Console.WriteLine("[QuoteEntity] Processing complete");
            
            //calculate the process duration
            DateTime endTime = DateTime.Now;
            Console.WriteLine("Start time: {0}", startTime);
            Console.WriteLine("End time: {0}", endTime);
            Console.WriteLine("Duration: {0}", endTime-startTime);
            
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add list contents returned from threads to a global list
        /// and calculate minumum, maximum and sum for sub-lists
        /// to accelerate the calculation 
        /// </summary>
        /// <param name="globalList"></param>
        /// <param name="currentList"></param>
        /// <param name="listOfAmounts"></param>
        /// <param name="calc"></param>
        private static void AddToGlobalList(ref List<Quote> globalList, List<Quote> currentList, List<decimal> listOfAmounts, ref Calculation calc)
        {
            lock (locker)
            {
                globalList.AddRange(currentList);
                calc.max = Math.Max(calc.max, listOfAmounts.Max());
                calc.min = Math.Min(calc.min, listOfAmounts.Min());
                calc.sum += listOfAmounts.Sum();
            }
        }

        /// <summary>
        /// Generate quotes
        /// </summary>
        /// <param name="count"></param>
        /// <param name="amounts"></param>
        /// <returns>List of Quotes</returns>
        static private List<Quote> QuoteGenerator(int count, out List<decimal> amounts)
        {
            Console.WriteLine("\t[QuoteGenerator] Generating '{0}' random quotes by thread {1} ...", count, Thread.CurrentThread.ManagedThreadId.ToString());
            List<Quote> quotes = new List<Quote>();
            amounts = new List<decimal>();
            Quote quote = new Quote();

            var random = new Random();
            for (int i = 0; i < count; i++)
            {
                quote.Id = random.Next();
                quote.CustomerName = random.Next().ToString();
                quote.Amount = random.Next();
                quote.CreatedAt = DateTime.Now;
                quote.ThreadID = Thread.CurrentThread.ManagedThreadId;

                quotes.Add(quote);
                amounts.Add(quote.Amount);
            }

            Console.WriteLine("\t[QuoteGenerator] Random quotes are created for thread id {0}", quote.ThreadID.ToString());

            return quotes;
        }

        #endregion
    }
}
