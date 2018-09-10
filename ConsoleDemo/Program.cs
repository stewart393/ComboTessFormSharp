using System;
using System.Diagnostics;

namespace Tesseract.ConsoleDemo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string testImagePath;

            if (args.Length > 0)
            { testImagePath = args[0]; }
            else
            { testImagePath = "./phototest.tif"; }

            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    using (var imgPages = PixArray.LoadMultiPageTiffFromFile(testImagePath))
                    {
                        int fileNum = 1;
                        System.IO.FileInfo mfile = new System.IO.FileInfo(testImagePath);                        
                        foreach (var img in imgPages)
                        {
                            string hocrPath = mfile.DirectoryName + "\\" + System.IO.Path.GetFileNameWithoutExtension(testImagePath) + "_p" + fileNum.ToString("000") + ".xhtml";
                            using (var page = engine.Process(img))
                            {
                                string hocr = page.GetHOCRText(1, false);
                                System.IO.File.AppendAllText(hocrPath, hocr);
                                var text = page.GetText();
                                Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());
                                Console.WriteLine("Text (GetText): \r\n{0}", text);
                                Console.WriteLine("Text (iterator):");
                                using (var iter = page.GetIterator())
                                {
                                    iter.Begin();
                                    do
                                    {
                                        do
                                        {
                                            do
                                            {
                                                do
                                                {
                                                    if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                                    {
                                                        Console.WriteLine("<BLOCK>");
                                                        Rect currentBlock;
                                                        iter.TryGetBoundingBox(PageIteratorLevel.Block, out currentBlock) ;
                                                        Console.WriteLine("(" + currentBlock.X1.ToString() + "," +  currentBlock.Y1.ToString() + ")  ("   + currentBlock.X2.ToString() + "," + currentBlock.Y2.ToString() + ")");                                                        
                                                        Console.WriteLine("");
                                                    }

                                                    Console.Write(iter.GetText(PageIteratorLevel.Word));
                                                    Console.Write(" ");

                                                    if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                                                    {
                                                        Console.WriteLine();
                                                    }
                                                } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));

                                                if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))
                                                {
                                                    Console.WriteLine();
                                                }
                                            } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                                        } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));                                        
                                    } while (iter.Next(PageIteratorLevel.Block));
                                }
                            }
                            fileNum++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Console.WriteLine("Unexpected Error: " + e.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(e.ToString());
            }
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
    }
}