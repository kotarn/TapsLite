using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Telnet
{
    class TextFormatter
    {
        static Regex page;
        static Regex whisper;
        static Regex notify;

        public Run FormattedRun { get; private set; }
        public bool IsPage { get; private set; }
        public bool IsWhisper { get; private set; }
        public bool IsNotify { get; private set; }
        public bool IsConnect { get; private set; }

        static TextFormatter()
        {
            page = new Regex(@"^(?:\S+ page)|(In a page)");
            whisper = new Regex(@"^\S+ whisper");
            notify = new Regex(@"^<");
        }

        public TextFormatter(string message)
        {
            IsPage = IsWhisper = IsNotify = false;

            FormattedRun = new Run(message);

            if (page.IsMatch(message))
            {
                TextRange range = new TextRange(FormattedRun.ContentStart, FormattedRun.ContentEnd);

                range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
                range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.ExtraBold);

                IsPage = true;
            }
            else if (whisper.IsMatch(message))
            {
                TextRange range = new TextRange(FormattedRun.ContentStart, FormattedRun.ContentEnd);

                range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkBlue);
                range.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);

                IsWhisper = true;
            }
            else if (message[0] == '#')
            {
                TextRange range = new TextRange(FormattedRun.ContentStart, FormattedRun.ContentEnd);

                range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.LightGray);
            }
            else if (message[0] == '<')
            {
                TextRange range = new TextRange(FormattedRun.ContentStart, FormattedRun.ContentEnd);

                range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Cyan);
                range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.ExtraBold);

                IsNotify = true;
            }
            else if (message.IndexOf("Somewhere on the muck") == 0)
            {
                TextRange range = new TextRange(FormattedRun.ContentStart, FormattedRun.ContentEnd);

                range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkSlateGray);

                IsConnect = true;
            }
        }
    }
}
