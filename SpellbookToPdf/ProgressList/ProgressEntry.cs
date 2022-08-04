using SpellbookToPdf.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellbookToPdf.ProgressList
{
    public enum State
    {
        NOT_DONE,
        IN_PROGRESS,
        DONE,
        ABANDONED
    }
    class ProgressEntry
    {
        public string Name { get; set; }
        public State State { get; set; }
        public IList<ProgressEntry> SubEntries { get; set; } = new List<ProgressEntry>();

        //private ILogger logger;

        public ProgressEntry(string name, State state)
        {
            Name = name;
            State = state;
        }

        public void Report(int index = 1, string previous = "")
        {
            var color = Console.ForegroundColor;
            try
            {
                switch (State)
                {
                    case State.NOT_DONE: Console.ForegroundColor = ConsoleColor.Red; break;
                    case State.ABANDONED: Console.ForegroundColor = ConsoleColor.Yellow; break;
                    case State.DONE: Console.ForegroundColor = ConsoleColor.Green; break;
                    case State.IN_PROGRESS: Console.ForegroundColor = ConsoleColor.Blue; break;
                }

                Console.WriteLine("{0} {1,-30}[{2,20}]", previous + index.ToString(), Name, State);
                //logger.Debug(string.Format("{0} [{1}]", Name, State));

                for (int i = 0; i < SubEntries.Count; i++)
                {
                    SubEntries[i].Report(i+1, previous + index.ToString() + ".");
                }
                //foreach (var entry in SubEntries)
                //{
                //    entry.Report();
                //}
            } finally
            {
                Console.ForegroundColor = color;
            }
        }

        public void AddSubEntry(ProgressEntry entry)
        {
            SubEntries.Add(entry);
        }
    }
}
