using Ninject;
using Ninject.Parameters;
using SpellbookToPdf.Logger;
using SpellbookToPdf.Models.Spells;
using SpellbookToPdf.ProgressList;
using SpellbookToPdf.Writer;
using SpellbookToPdf.Writer.PdfWriter;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SpellbookToPdf
{
    class Test
    {
        private ILogger logger;

        public Test(ILogger logger) => this.logger = logger;
        public void x() => logger.Critical("jest git");
    }

    class Program
    {
        public static IKernel kernel;

        static void Main()
        {
            Program.kernel = new StandardKernel();
            kernel.Bind<ILogger>().To<ConsoleLogger>().InSingletonScope();
            kernel.Bind<IPdfWriter>().To<PdfWriter>();
            //kernel.Bind<ProgressEntry>().ToSelf();

            var pe = new ProgressEntry("To do list", State.IN_PROGRESS) { 
                SubEntries = new List<ProgressEntry>() {
                    new ProgressEntry("Spells", State.IN_PROGRESS) {
                        SubEntries = new List<ProgressEntry>() {
                            new ProgressEntry("Front page", State.IN_PROGRESS)
                        },
                    },
                    new ProgressEntry("Items", State.NOT_DONE)
                } 
            };

            //var pe1 = kernel.Get<ProgressEntry>();
            //pe1.Name = "To do list";
            //pe1.State = State.IN_PROGRESS;

            //var pe2 = kernel.Get<ProgressEntry>();
            //pe2.Name = "Spells";
            //pe2.State = State.IN_PROGRESS;
            //pe1.AddSubEntry(pe2);

            pe.Report();

            //    .Debug("1. Spells [ ]");
            //kernel.Get<ILogger>().Debug("1.1. Create front page [ ]");
            //kernel.Get<ILogger>().Debug("1.2. Create background page [ ]");

            var p = Process.GetProcessesByName("FoxitPDFReader");
            if (p.Length > 0)
            {
                p[0].Kill();
            }

            List<Spell> spells = SpellsLoader.LoadSpells();
            spells.Sort((s1, s2) => s1.Level - s2.Level == 0 ? s1.Name.CompareTo(s2.Name) : s1.Level - s2.Level);

            kernel.Get<IPdfWriter>().Print(spells
                .GetRange(0, 30)
                //.FindAll(s => s.Name.Equals("Wish"))
                .ToArray());

            //var pdfWriter = IWriter.GetWriter(PrintType.PDF);
            //pdfWriter.Print(spells
            //    .GetRange(0, 30)
            //    //.FindAll(s => s.Name.Equals("Wish"))
            //    .ToArray());

            ProcessStartInfo psi = new("D:/output.pdf")
            {
                UseShellExecute = true
            };
            Process.Start(psi);

            Console.ReadKey();
        }
    }
}
