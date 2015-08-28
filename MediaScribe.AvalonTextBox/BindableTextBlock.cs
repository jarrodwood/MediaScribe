using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using AvalonTextBox.Converters;
using System.Windows.Media;
using System.Windows.Threading;
using JayDev.MediaScribe.Common;

namespace AvalonTextBox
{
    public class BindableTextBlock : TextBlock
    {

        public string MarkedupText
        {
            get { return (string)GetValue(MarkedupTextProperty); }
            set { SetValue(MarkedupTextProperty, value); }
        }


        // Using a DependencyProperty as the backing store for MarkedupText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkedupTextProperty =
            DependencyProperty.Register("MarkedupText", typeof(string), typeof(BindableTextBlock), new UIPropertyMetadata(new PropertyChangedCallback((sender, args) => {
                BindableTextBlock textBlock = sender as BindableTextBlock;
                textBlock.lastMarkedupText = (string)args.NewValue;
                SetText(textBlock, args.NewValue as string, textBlock.HighlightSections);
            })));




        public List<HighlightMatch> HighlightSections
        {
            get { return (List<HighlightMatch>)GetValue(HighlightSectionsProperty); }
            set { SetValue(HighlightSectionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HighlightSections.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightSectionsProperty =
            DependencyProperty.Register("HighlightSections", typeof(List<HighlightMatch>), typeof(BindableTextBlock), new UIPropertyMetadata(new PropertyChangedCallback((sender, args) =>
            {
                BindableTextBlock textBlock = sender as BindableTextBlock;
                List<HighlightMatch> highlightSections = (List<HighlightMatch>)args.NewValue;

                SetText(textBlock, textBlock.lastMarkedupText, highlightSections);
            })));


        /// <summary>
        /// Used to return the LOWERCASE, tag-stripped text used for the search functionality
        /// </summary>
        public string OutStrippedText
        {
            get { return (string)GetValue(OutStrippedTextProperty); }
            set { SetValue(OutStrippedTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OutStrippedText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OutStrippedTextProperty =
            DependencyProperty.Register("OutStrippedText", typeof(string), typeof(BindableTextBlock), new PropertyMetadata(null));

        

        public string lastMarkedupText;

        private static void SetText(BindableTextBlock textBlock, string markedupText, List<HighlightMatch> highlightSections)
        {
            textBlock.Dispatcher.BeginInvoke(
                       DispatcherPriority.Loaded,
                       new Action(() =>
                       {
                           if (null == markedupText || string.IsNullOrEmpty((string)markedupText))
                           {
                               textBlock.Text = "Double-click here to write a note...";
                               textBlock.FontStyle = FontStyles.Italic;
                               textBlock.Opacity = 0.5;
                           }
                           else
                           {

                               textBlock.Text = null;
                               textBlock.FontStyle = FontStyles.Normal;
                               textBlock.Opacity = 1;

                               textBlock.Inlines.Clear();

                               List<Section> sections = new MarkedupTextToNoteSectionsConverter().Convert(markedupText, typeof(List<Section>), null, null) as List<Section>;
                               List<Inline> inlines = new NoteSectionsToInlineListConverter().Convert(sections, typeof(List<Inline>), null, null, highlightSections) as List<Inline>;

                               
                               //var converter = new MarkedupTextToInlineListConverter() { HighlightSections = highlightSections };
                               //var inlines = converter.Convert(markedupText, typeof(List<Inline>), null, null) as List<Inline>;
                               //trying to add 'null' throws exceptions
                               if (null != inlines && inlines.Count > 0)
                               {
                                   textBlock.Inlines.AddRange(inlines);
                               }



                               #region HIGHLIGHTING - extract stripped text
                               StringBuilder strippedTextBuilder = new StringBuilder();
                               foreach (var section in sections)
                               {
                                   strippedTextBuilder.Append(section.Text.ToLowerInvariant());
                               }
                               string strippedText = strippedTextBuilder.ToString();
                               textBlock.OutStrippedText = strippedText;
                               #endregion
                           }
                       }));
        }
        

        public IEnumerable<Inline> InlineCollection
        {
            get
            {
                return (IEnumerable<Inline>)GetValue(InlineCollectionProperty);
            }
            set
            {
                SetValue(InlineCollectionProperty, value);
            }
        }

        public static readonly DependencyProperty InlineCollectionProperty = DependencyProperty.Register(
            "InlineCollection",
            typeof(IEnumerable<Inline>),
            typeof(BindableTextBlock),
                new UIPropertyMetadata((PropertyChangedCallback)((sender, args) =>
                {
                    BindableTextBlock textBlock = sender as BindableTextBlock;

                    if (textBlock != null)
                    {
                        textBlock.Inlines.Clear();

                        IEnumerable<Inline> inlines = args.NewValue as IEnumerable<Inline>;

                        if (inlines != null)
                            textBlock.Inlines.AddRange(inlines);
                    }
                })));




        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(BindableTextBlock), new UIPropertyMetadata(OnDefaultTextChanged));



        public static void OnDefaultTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            string text = args.NewValue as string;
            if (null != text && false == string.IsNullOrEmpty(text))
            {
                BindableTextBlock textblock = (BindableTextBlock)obj;
                if (null != text && textblock.Inlines.Count == 0)
                {
                    Run run = new Run(text);
                    run.Foreground = new SolidColorBrush(Color.FromArgb(255, 180, 180, 180));
                    Italic italic = new Italic(run);
                    textblock.Inlines.Add(italic);
                }
            }
        }
    }
}