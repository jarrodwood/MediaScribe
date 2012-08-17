using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JayDev.MediaScribe.Core
{
    class CsvExporter
    {

        const string Separator = "\t";
        const string NewLine = "\r\n";
        const string CurrentVersion = "1.0";
        public string CreateCsvText(List<Track> tracks, List<Note> notes)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Version");
            builder.Append(Separator);
            builder.Append(CurrentVersion);
            builder.Append(NewLine);

            builder.Append(NewLine);
            builder.Append("Order Number");
            builder.Append(NewLine);
            builder.Append("Track File Size");
            builder.Append(Separator);
            builder.Append("Track File Path");

            foreach (Track track in tracks)
            {
                builder.Append(NewLine);
                builder.Append(track.OrderNumber);
                builder.Append(Separator);
                builder.Append(track.FileSize);
                builder.Append(Separator);
                builder.Append(track.FilePath);
            }
            builder.Append(NewLine);


            builder.Append(NewLine);
            builder.Append("Note");
            builder.Append(NewLine);
            builder.Append("Track File Size");
            builder.Append(Separator);
            builder.Append("Track File Path");

            foreach (Note note in notes)
            {
                builder.Append(note.Start.StringDisplayValue);
                builder.Append(Separator);
                builder.Append(note.End.StringDisplayValue);
                builder.Append(Separator);
                builder.Append(EscapeTextForCsv(note.Body));
                builder.Append(Separator);
                builder.Append(note.Rating);
                builder.Append(Separator);
            }

            return builder.ToString();
        }

        private string EscapeTextForCsv(string input)
        {
            input = input.Replace(@"\", @"\\");
            input = input.Replace(@",", @"\,");
            return input;
        }

        private List<string> GetTextFromCsv(string input, int expectedCellCount)
        {
            List<string> result = new List<string>();
            int index = 0;
            string remainingText = input;
            while ((index = input.IndexOf(',', index)) != -1)
            {
                if (index > 0)
                {

                }
            }

            return null;
        }
    }
}
