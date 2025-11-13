using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MalformedHtmlFix.UnitTest
{
    [TestClass]
    public class HtmlHelper_SanitizeHtmlDocumentTests
    {
        #region Unit Test Focusing on <a> Tag

        [TestMethod]
        public void TestMissingClosingATag()
        {
            string input = "<a href='link'";
            string expected = "<html><body><a href=\"#\"></a></body></html>";

            string actual = HtmlHelper.SanitizeHtmlDocument(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNestedAWithinDiv()
        {
            string input = "<div><a href='link'>Click<div>More</div></div>";
            string expected = "<html><body><div><a href=\"#\">Click<div>More</div></a></div></body></html>";

            string actual = HtmlHelper.SanitizeHtmlDocument(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMultipleATags()
        {
            string input = "<p><a href='link1'>One<p><a href='link2'>Two<p>";
            //TODO: MANH Check if this is the desired output
            string expected = "<html><body><p><a href=\"#\">One</a></p><p><a href=\"#\">Two</a></p></body></html>";

            string actual = HtmlHelper.SanitizeHtmlDocument(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestATagInsideTable()
        {
            string input = "<table><tr><td><a href='link'>Cell1<td>Cell2<tr></table>";
            string expected = "<html><body><table><tbody><tr><td><a href=\"#\">Cell1</a></td><td>Cell2</td></tr><tr></tr></tbody></table></body></html>";

            string actual = HtmlHelper.SanitizeHtmlDocument(input);
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Other HTML Tests

        [TestMethod]
        public void TestSelfClosingTags()
        {
            string input = "<p>Line 1<br>Line 2<img src='image.png'><hr></p>";
            string expected = "<html><body><p>Line 1<br>Line 2<img src=\"image.png\"></p><hr></body></html>";

            string actual = HtmlHelper.SanitizeHtmlDocument(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAlreadyWellFormedHtml()
        {
            string input = "<html><body><p>Hello</p><div>Content</div></body></html>";
            string expected = "<html><body><p>Hello</p><div>Content</div></body></html>";

            string actual = HtmlHelper.SanitizeHtmlDocument(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestEmptyHtml()
        {
            string input = "";
            string expected = "<html><body></body></html>";

            string actual = HtmlHelper.SanitizeHtmlDocument(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNullHtml()
        {
            string input = null;
            string expected = "<html><body></body></html>";

            string actual = HtmlHelper.SanitizeHtmlDocument(input);
            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
