using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;

// Author: Orrett Coke
// Date: 04-19-2020
// Based on webscrapping tutorial from https://dev.to/rachelsoderberg/create-a-simple-web-scraper-in-c-1l1m

namespace Graphics_and_Networking___Scraper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            ScrapeWebsite();
        }


        private string Title { get; set; }
        private string Url { get; set; }
        private string siteUrl = "https://www.oceannetworks.ca/news/stories";

        public string[] GetQueryTerms()
        {
            // Return terms from search box
            return textBox1.Text.Split(',');
        }

        internal async void ScrapeWebsite()
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage request = await httpClient.GetAsync(siteUrl);
            cancellationToken.Token.ThrowIfCancellationRequested();

            Stream response = await request.Content.ReadAsStreamAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();

            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(response);

            // Parse results from the initial bulk document scrape
            GetScrapeResults(document);
        }

        private void GetScrapeResults(IHtmlDocument document)
        {
            IEnumerable<IElement> articleLink = null;

            // Use the query terms to find results in HTML and print each
            foreach (var term in GetQueryTerms())
            {
                // Find each element using specified class names and query terms
                articleLink = document.All.Where(x =>
                    x.ClassName == "views-field views-field-nothing" &&
                    (x.ParentElement.InnerHtml.Contains(term) || x.ParentElement.InnerHtml.Contains(term.ToLower()))).Skip(1);

                // Print results if found
                if (articleLink.Any())
                {
                    PrintResults(articleLink);
                }
            }
        }

        public void PrintResults(IEnumerable<IElement> articleLink)
        {
            // Clean each scrapped result and print to the textbox
            foreach (var element in articleLink)
            {
                CleanUpResults(element);

                // Print each result to the text box
                richTextBox1.AppendText($"{Title} - {Url}{Environment.NewLine}");
            }
        }

        private void CleanUpResults(IElement result)
        {
            // Clean the results by replacing extra unnecessary HTML chunks
            string htmlResult = result.InnerHtml.ReplaceFirst("        <span class=\"field-content\"><div><a href=\"", @"https://www.oceannetworks.ca");
            htmlResult = htmlResult.ReplaceFirst("\">", "*");
            htmlResult = htmlResult.ReplaceFirst("</a></div>\n<div class=\"article-title-top\">", "-");
            htmlResult = htmlResult.ReplaceFirst("</div>\n<hr></span>  ", "");

            // Split each result into a article title and link. This allows for easing printing in PrintResults()
            SplitResults(htmlResult);
        }

        private void SplitResults(string htmlResult)
        {
            string[] splitResults = htmlResult.Split('*');
            Url = splitResults[0];
            Title = splitResults[1];
        }
    }
}
