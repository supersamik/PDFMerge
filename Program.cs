using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
namespace PDFMerge
{
    class Program
    {
        /// <summary>
        /// Applicazione che combina più file pdf in un unico file
        /// </summary>
        /// <param name="args">
        /// Argomenti da passare alla chiamata 
        /// Dest: stringa che definisce il nome compensivo di path del file risultato del merge
        /// 
        /// Esempio di chiamata:
        /// -Dest:c:\destinazione.pdf
        /// -A:c:\primo.pdf 
        /// -B:c:\secondo.pdf
        /// </param>
        static void Main(string[] args)
        {
            //Initiate logging based on web.config file
            log4net.Config.XmlConfigurator.Configure();
            // Create a logger for use in this class
            var logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            var commandLine = new Arguments(args);
            string destFile = string.Empty;
            try
            {
                var errorList = new List<Exception>();

                if (commandLine["Dest"] != null)
                {
                    Console.WriteLine("Parametro destinazione (Dest): " + commandLine["Dest"]);
                    destFile = commandLine["Dest"];
                }
                else errorList.Add(new Exception("Parametro mancante: \"Dest\"\n"));

                if (commandLine["A"] != null)
                {
                    Console.WriteLine("Parametro destinazione (A): " + commandLine["A"]);
                    if (!File.Exists(commandLine["A"]))
                        errorList.Add(new Exception("File A non trovato: " + commandLine["A"] + "\n"));
                }
                else errorList.Add(new Exception("Parametro mancante: \"A\"\n"));

                if (commandLine["B"] != null)
                {
                    Console.WriteLine("Parametro destinazione (B): " + commandLine["B"]);
                    if (!File.Exists(commandLine["B"]))
                        errorList.Add(new Exception("File A non trovato: " + commandLine["B"] + "\n"));
                }
                else errorList.Add(new Exception("Parametro mancante: \"B\"\n"));

                logger.Debug("Parametro destinazione (Dest): " + commandLine["Dest"]);
                logger.Debug("Parametro destinazione (A): " + commandLine["A"]);
                logger.Debug("Parametro destinazione (B): " + commandLine["B"]);



                if (errorList.Any())
                {
                    var errorMessage = string.Concat((from s in errorList select s.Message).ToArray());
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Errore:");
                logger.Error(e);
                Console.WriteLine(e.Message);
                Console.WriteLine("");

                Console.WriteLine("-----------  Help  -----------");
                Console.WriteLine("Dest: file pdf di destinazione");
                Console.WriteLine("A: primo file sorgente");
                Console.WriteLine("B: secondo file sorgente");
                Console.WriteLine("------------------------------");
                return;
            }

            logger.Debug("Inizializzazione applicazione effettuata correttamente");
            try
            {
                try
                {
                    byte[] dest = null;
                    var fileList = new List<string> { commandLine["A"], commandLine["B"] };
                    MergePdfFiles(ref dest, fileList);
                    File.WriteAllBytes(destFile, dest);
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }

            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }


        static void MergePdfFiles(ref byte[] destination, List<string> sourceFilesUri)
        {
            var f = 0;
            var reader = new PdfReader(sourceFilesUri[f]);
            var document = new Document(reader.GetPageSizeWithRotation(1));
            try
            {
                var n = reader.NumberOfPages;
                Console.WriteLine("There are " + Convert.ToString(n) + " pages in the original file.");

                var m = new MemoryStream();
                var writer = PdfWriter.GetInstance(document, m);

                document.Open();
                var cb = writer.DirectContent;

                while (f < sourceFilesUri.Count)
                {
                    dynamic i = 0;

                    while (i < n)
                    {
                        i += 1;
                        document.SetPageSize(reader.GetPageSizeWithRotation(i));
                        document.NewPage();
                        PdfImportedPage page = writer.GetImportedPage(reader, i);
                        int rotation = reader.GetPageRotation(i);

                        if (rotation == 90 || rotation == 270)
                        {
                            cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                        }
                        else
                        {
                            cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                        }

                        Console.WriteLine("Processed page " + Convert.ToString(i));
                    }
                    f += 1;
                    if (f < sourceFilesUri.Count)
                    {
                        reader = new PdfReader(sourceFilesUri[f]);
                        n = reader.NumberOfPages;
                        Console.WriteLine("There are " + Convert.ToString(n) + " pages in the original file.");
                    }
                }
                document.Close();
                destination = m.ToArray();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                document.Close();
            }
        }
    }
}
