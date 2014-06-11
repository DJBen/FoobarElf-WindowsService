using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace FoobarElfTest
{
    [TestClass]
    public class HTMLExtractionTest
    {

        public static string[] htmls;

        [ClassInitialize()]
        public static void InitTest(TestContext testContext)
        {
             htmls = new string[] { Properties.Resources.Stopped, Properties.Resources.Aijiu_playing, Properties.Resources.Shuyu_playing, Properties.Resources.Westlife };
        }

        [TestMethod]
        public void testParsing()
        {
            int index = 0;
            List<FoobarElf.Song> songs = new List<FoobarElf.Song>();

            foreach (string statusHtml in htmls)
            {
                // Test for every html that contains player status
                FoobarElf.FoobarPlayer player = new FoobarElf.FoobarPlayer();
                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(statusHtml);
                player.loadDocument(document);

                if (index != 0) Assert.IsNotNull(player.currentSong);
                Console.WriteLine(player);
                index++;
            }
        }
    }
}
