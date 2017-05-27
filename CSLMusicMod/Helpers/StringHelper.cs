using System;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace CSLMusicMod
{
    public static class StringHelper
    {
        public static string CutoffAfter(string text, UIFontRenderer renderer, float size)
        {
            if(renderer.MeasureString(text).x > size)
            {
                StringBuilder b = new StringBuilder(text.Length);

                float current_width = 0;
                float[] char_widths = renderer.GetCharacterWidths(text);

                for (int i = 0; i < char_widths.Length && current_width < size; ++i)
                {
                    b.Append(text[i]);
                    current_width += char_widths[i];
                }

                b.Append(" ...");

                return b.ToString();
            }
            else
            {
                return text;
            }
        }

        public static string Wrap(string text, int size)
        {
            string[] originalLines = text.Split(new string[] { " " }, StringSplitOptions.None);

            StringBuilder wrappedLines = new StringBuilder();

            StringBuilder actualLine = new StringBuilder();
            double actualWidth = 0;

            foreach (var item in originalLines)
            {
                actualLine.Append(item + " ");
                actualWidth += item.Length;

                if (actualWidth > size)
                {
                    wrappedLines.Append(actualLine.ToString()).Append("\n");
                    actualLine.Length = 0;
                    actualWidth = 0;
                }
            }

            if (actualLine.Length > 0)
                wrappedLines.Append(actualLine.ToString()).Append("\n");

            return wrappedLines.ToString();
        }
    }
}
