using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TuesPechkin.Core.ConsoleTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var pdfconvertor = new TuesPechkinePdf();
                for (int i= 0;i< 10;i++)
                {
                    pdfconvertor.CreatePdf($"pdf-{i}.pdf");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("convertor is compeleted!");
            Console.ReadLine();
        }

    }
}
