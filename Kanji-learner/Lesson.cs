﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Kanji_learner
{
    class Lesson
    {
        private List<String> kanji;
        private List<String> original_list;
        private Dictionary<String, String> strokes = new Dictionary<String, String>();
        private Dictionary<String, String> meaning = new Dictionary<String, String>();
        private Dictionary<String, String> onyomi = new Dictionary<String, String>();
        private Dictionary<String, String> kunyomi = new Dictionary<String, String>();
        private String currentStrokes;
        private String currentMeaning;
        private String currentOnyomi;
        private String currentKunyomi;
        private String fileName;
        private int originalCount;
        private int currentKanjiNumber;
        private bool isKanji;
        Random random = new Random();
        Regex wordMatch = new Regex(@"[\w\-]+");
        Regex letterMatch = new Regex(@"^[\w]+");

        public Lesson(String fileName, bool isKanji)
        {
            this.isKanji = isKanji;
            this.fileName = fileName;
   
            kanji = new List<String>();
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNode dataset = doc.SelectSingleNode("data-set");

            String currentKanji;
            String[] currentValues;

            foreach (XmlNode entryNode in dataset.ChildNodes)
            {
                currentValues = new String[4];
                currentKanji = entryNode.SelectSingleNode("Kanji").InnerText;
                kanji.Add(currentKanji);
                strokes.Add(currentKanji, entryNode.SelectSingleNode("Strokes") != null ? entryNode.SelectSingleNode("Strokes").InnerText : "");
                meaning.Add(currentKanji, entryNode.SelectSingleNode("Meaning") != null ? entryNode.SelectSingleNode("Meaning").InnerText : "");
                onyomi.Add(currentKanji, entryNode.SelectSingleNode("Onyomi") != null ? entryNode.SelectSingleNode("Onyomi").InnerText : "");
                kunyomi.Add(currentKanji, entryNode.SelectSingleNode("Kunyomi") != null ? entryNode.SelectSingleNode("Kunyomi").InnerText : "");
            }
            original_list = kanji;
            setCurrentStrings();
            originalCount = kanji.Count;
        }

        private void setCurrentStrings()
        {
            currentStrokes = strokes[kanji[currentKanjiNumber]];
            currentMeaning = meaning[kanji[currentKanjiNumber]];
            currentOnyomi = onyomi[kanji[currentKanjiNumber]];
            currentKunyomi = kunyomi[kanji[currentKanjiNumber]];
        }

        public bool isKanjiLesson()
        {
            return isKanji;
        }

        public String getFileName()
        {
            return this.fileName;
        }

        public int getRemainingCount()
        {
            return kanji.Count;
        }

        public int getOriginalCount()
        {
            return originalCount;
        }

        public void selectRandomKanji()
        {
            if (kanji.Count == 0)
            {
                return;
            }
            int number = random.Next(kanji.Count);
            currentKanjiNumber = number;
            setCurrentStrings();
        }

        public void selectNextKanji()
        {
            if (kanji.Count == 0)
            {
                return;
            }
            currentKanjiNumber++;
            if (currentKanjiNumber >= kanji.Count)
            {
                currentKanjiNumber = 0;
            }
            setCurrentStrings();
        }

        public String getKanji()
        {
            if (kanji.Count == 0)
            {
                return "";
            }
            return kanji[currentKanjiNumber];
        }

        public String getStrokesForCurrentKanji()
        {
            return currentStrokes;
        }

        public String getMeaningForCurrentKanji()
        {
            return currentMeaning;
        }

        public String getOnyomiForCurrentKanji()
        {
            return currentOnyomi;
        }

        public String getKunyomiForCurrentKanji()
        {
            return currentKunyomi;
        }

        public bool currentKanjiPassed()
        {
            kanji.RemoveAt(currentKanjiNumber);
            currentKanjiNumber--;
            return kanji.Count != 0;
        }

        private bool correctString(String inputString, String passString)
        {
            foreach (Match currentPassString in wordMatch.Matches(passString))
            {
                if (string.Compare(inputString, currentPassString.Value, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool correctJapaneseString(String inputString, String passString)
        {
            inputString = inputString.Replace("ū", "uu");
            inputString = inputString.Replace("ō", "ou");
            inputString = inputString.Replace("oo", "ou");
            passString = passString.Replace("ū", "uu");
            passString = passString.Replace("ō", "ou");
            foreach (Match currentPassString in wordMatch.Matches(passString))
            {
                if (string.Compare(inputString, currentPassString.Value, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0)
                {
                    return true;
                } 
                else if (string.Compare(inputString, letterMatch.Match(passString).ToString(), CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0)
                {
                    return true;
                }
                else if (string.Compare(inputString, passString.Replace("-", ""), CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool correctOnyomi(String onyomiInput)
        { 
            if (!wordMatch.IsMatch(onyomiInput))
            {
                return false;
            }
            
            foreach (Match onyomiString in wordMatch.Matches(onyomiInput))
            {
                if (!correctJapaneseString(onyomiString.Value, this.getOnyomiForCurrentKanji()))
                {
                    return false;
                }
            }

            return true;
        }

        public bool correctKunyomi(String kunyomiInput)
        {
            if (!wordMatch.IsMatch(kunyomiInput))
            {
                return false;
            }

            foreach (Match kunyomiString in wordMatch.Matches(kunyomiInput))
            {
                if (!correctJapaneseString(kunyomiString.Value, this.getKunyomiForCurrentKanji()))
                {
                    return false;
                }
            }

            return true;
        }

        public bool correctRomaji(String romajiInput)
        {

            if (!wordMatch.IsMatch(romajiInput))
            {
                return false;
            }

            foreach (Match romajiString in wordMatch.Matches(romajiInput))
            {
                if (!correctJapaneseString(romajiString.Value, this.getOnyomiForCurrentKanji()) && !correctJapaneseString(romajiString.Value, this.getKunyomiForCurrentKanji()))
                {
                    return false;
                }
            }
            return true;
        }
        
        public bool correctTranslation(String translationInput)
        {
            if (!wordMatch.IsMatch(translationInput))
            {
                return false;
            }

            foreach (Match translationString in wordMatch.Matches(translationInput))
            {
                if (!correctString(translationString.Value, this.getMeaningForCurrentKanji()))
                {
                    return false;
                }
            }

            return true;
        }

        public void failAtCurrentKanji()
        {

        }

    }
}
