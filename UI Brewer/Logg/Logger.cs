using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UI_Brewer.Logg
{
    class Logger
    {
        public async void run()
        {

            //// These examples assume a "C:\Users\Public\TestFolder" folder on your machine.
            //// You can modify the path if necessary.


            //// Example #1: Write an array of strings to a file.
            //// Create a string array that consists of three lines.
            //string[] lines = { "First line", "Second line", "Third line" };
            //// WriteAllLines creates a file, writes a collection of strings to the file,
            //// and then closes the file.  You do NOT need to call Flush() or Close().
            //System.IO.File.WriteAllLines(@"C:\Users\USERNAME\AppData\Local\Packages\UI_Brewer\WriteLines.txt", lines);


            //// Example #2: Write one string to a text file.
            //string text = "A class is the most powerful data type in C#. Like a structure, " +
            //               "a class defines the data and behavior of the data type. ";
            //// WriteAllText creates a file, writes the specified string to the file,
            //// and then closes the file.    You do NOT need to call Flush() or Close().
            //System.IO.File.WriteAllText(@"C:\Users\Public\TestFolder\WriteText.txt", text);
            ////Output (to WriteLines.txt):
            //   First line
            //   Second line
            //   Third line
            await saveStringToLocalFile(" ", " ");
            //Output (to WriteText.txt):
            //   A class is the most powerful data type in C#. Like a structure, a class defines the data and behavior of the data type.
        }
        public static async Task saveStringToLocalFile(string filename, string content)
        {
            // saves the string 'content' to a file 'filename' in the app's local storage folder
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());

            // create a file with the given filename in the local folder; replace any existing file with the same name
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            // write the char array created from the content string into the file
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.Write(fileBytes, 0, fileBytes.Length);

            }
        }
            public static async Task AppendTextToFile(string filename, string content)
        {
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            // These examples assume a "C:\Users\Public\TestFolder" folder on your machine.
            // You can modify the path if necessary.


            // Example #1: Write an array of strings to a file.
            // Create a string array that consists of three lines.
            string[] lines = { "First line", "Second line", "Third line" };
            // WriteAllLines creates a file, writes a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            //System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\WriteLines.txt", lines);


            // Example #2: Write one string to a text file.
            string text = "A class is the most powerful data type in C#. Like a structure, " +
                           "a class defines the data and behavior of the data type. ";
            // WriteAllText creates a file, writes the specified string to the file,
            // and then closes the file.    You do NOT need to call Flush() or Close().
            //System.IO.File.WriteAllText(@"C:\Users\Public\TestFolder\WriteText.txt", text);

            // Example #3: Write only some strings in an array to a file.
            // The using statement automatically flushes AND CLOSES the stream and calls 
            // IDisposable.Dispose on the stream object.
            // NOTE: do not use FileStream for text files because it writes bytes, but StreamWriter
            // encodes the output as text.
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                using (System.IO.StreamWriter Streamfile =
                new System.IO.StreamWriter(stream,Encoding.Unicode,64,true))
                {
                    foreach (string line in lines)
                    {
                        // If the line doesn't contain the word 'Second', write the line to the file.
                        if (!line.Contains("Second"))
                        {
                            Streamfile.WriteLine(line);
                        }
                    }
                }
            }

            // Example #4: Append new text to an existing file.
            // The using statement automatically flushes AND CLOSES the stream and calls 
            // IDisposable.Dispose on the stream object.
            //using (System.IO.StreamWriter file =
            //    new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\WriteLines2.txt", true))
            //{
            //    file.WriteLine("Fourth line");
            //}
        }
        
    }
}
