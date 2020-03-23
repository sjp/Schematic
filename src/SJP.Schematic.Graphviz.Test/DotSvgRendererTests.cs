using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;

namespace SJP.Schematic.Graphviz.Tests
{
    [TestFixture]
    internal sealed class DotSvgRendererTests
    {
        private GraphvizTemporaryExecutable GraphvizExe { get; set; }

        private DotSvgRenderer Renderer => new DotSvgRenderer(GraphvizExe.DotExecutablePath);

        [OneTimeSetUp]
        public void Init()
        {
            GraphvizExe = new GraphvizTemporaryExecutable();
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            GraphvizExe.Dispose();
        }

        [Test]
        public void DotRenderer_GivenNullExecutablePath_ThrowsArgumentNullException()
        {
            Assert.That(() => new DotSvgRenderer(null), Throws.ArgumentNullException);
        }

        [Test]
        public void DotRenderer_GivenEmptyExecutablePath_ThrowsArgumentNullException()
        {
            Assert.That(() => new DotSvgRenderer(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public void DotRenderer_GivenWhiteSpaceExecutablePath_ThrowsArgumentNullException()
        {
            Assert.That(() => new DotSvgRenderer("   "), Throws.ArgumentNullException);
        }

        [Test]
        public void DotRenderer_GivenMissingExecutablePath_ThrowsFileNotFoundException()
        {
            Assert.That(() => new DotSvgRenderer("path_not_existing"), Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void RenderToSvg_GivenNullDotDiagram_ThrowsArgumentNullException()
        {
            Assert.That(() => Renderer.RenderToSvg(null), Throws.ArgumentNullException);
        }

        [Test]
        public void RenderToSvg_GivenEmptyDotDiagram_ThrowsArgumentNullException()
        {
            Assert.That(() => Renderer.RenderToSvg(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public void RenderToSvg_GivenWhiteSpaceDotDiagram_ThrowsArgumentNullException()
        {
            Assert.That(() => Renderer.RenderToSvg("   "), Throws.ArgumentNullException);
        }

        [Test]
        public void RenderToSvg_GivenInvalidDot_ThrowsGraphvizException()
        {
            Assert.That(() => Renderer.RenderToSvg("this is not dot"), Throws.TypeOf<GraphvizException>());
        }

        [Test]
        public void RenderToSvg_GivenValidDot_ReturnsValidSvgXml()
        {
            var svg = Renderer.RenderToSvg("digraph g { a -> b }");

            Assert.That(() => _ = XDocument.Parse(svg, LoadOptions.PreserveWhitespace), Throws.Nothing);
        }

        // Note, this is fragile only because it contains date/version comments.
        // It should be very stable otherwise.
        [Test]
        public void RenderToSvg_GivenValidDot_ReturnsExpectedSvg()
        {
            var svg = Renderer.RenderToSvg("digraph g { a -> b }");

            Assert.That(svg, Is.EqualTo(SimpleGraphExpectedSvg));
        }

        [Test]
        public void RenderToSvg_GivenVizJsGraphExample_ReturnsValidSvgXml()
        {
            var svg = Renderer.RenderToSvg(VizJsExample);

            Assert.That(() => _ = XDocument.Parse(svg, LoadOptions.PreserveWhitespace), Throws.Nothing);
        }

        [Test]
        public void RenderToSvgAsync_GivenNullDotDiagram_ThrowsArgumentNullException()
        {
            Assert.That(() => Renderer.RenderToSvgAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public void RenderToSvgAsync_GivenEmptyDotDiagram_ThrowsArgumentNullException()
        {
            Assert.That(() => Renderer.RenderToSvgAsync(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public void RenderToSvgAsync_GivenWhiteSpaceDotDiagram_ThrowsArgumentNullException()
        {
            Assert.That(() => Renderer.RenderToSvgAsync("   "), Throws.ArgumentNullException);
        }

        [Test]
        public void RenderToSvgAsync_GivenInvalidDot_ThrowsGraphvizException()
        {
            Assert.That(async () => await Renderer.RenderToSvgAsync("this is not dot").ConfigureAwait(false), Throws.TypeOf<GraphvizException>());
        }

        [Test]
        public async Task RenderToSvgAsync_GivenValidDot_ReturnsValidSvgXml()
        {
            var svg = await Renderer.RenderToSvgAsync("digraph g { a -> b }").ConfigureAwait(false);

            Assert.That(() => _ = XDocument.Parse(svg, LoadOptions.PreserveWhitespace), Throws.Nothing);
        }

        // Note, this is fragile only because it contains date/version comments.
        // It should be very stable otherwise.
        [Test]
        public async Task RenderToSvgAsync_GivenValidDot_ReturnsExpectedSvg()
        {
            var svg = await Renderer.RenderToSvgAsync("digraph g { a -> b }").ConfigureAwait(false);

            Assert.That(svg, Is.EqualTo(SimpleGraphExpectedSvg));
        }

        [Test]
        public async Task RenderToSvgAsync_GivenVizJsGraphExample_ReturnsValidSvgXml()
        {
            var svg = await Renderer.RenderToSvgAsync(VizJsExample).ConfigureAwait(false);

            Assert.That(() => _ = XDocument.Parse(svg, LoadOptions.PreserveWhitespace), Throws.Nothing);
        }

        private const string VizJsExample = @"digraph G {

    subgraph cluster_0 {
        style=filled;
        color=lightgrey;
        node [style=filled,color=white];
        a0 -> a1 -> a2 -> a3;
        label = ""process #1"";
    }

    subgraph cluster_1
    {
        node [style=filled];
        b0 -> b1 -> b2 -> b3;
        label = ""process #2"";
        color=blue
    }
    start -> a0;
    start -> b0;
    a1 -> b3;
    b2 -> a3;
    a3 -> a0;
    a3 -> end;
    b3 -> end;

    start[shape = Mdiamond];
    end[shape = Msquare];
}";

        private const string SimpleGraphExpectedSvg = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<!DOCTYPE svg PUBLIC ""-//W3C//DTD SVG 1.1//EN""
 ""http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd"">
<!-- Generated by graphviz version 2.38.0 (20140413.2041)
 -->
<!-- Title: g Pages: 1 -->
<svg width=""62pt"" height=""116pt""
 viewBox=""0.00 0.00 62.00 116.00"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"">
<g id=""graph0"" class=""graph"" transform=""scale(1 1) rotate(0) translate(4 112)"">
<title>g</title>
<polygon fill=""white"" stroke=""none"" points=""-4,4 -4,-112 58,-112 58,4 -4,4""/>
<!-- a -->
<g id=""node1"" class=""node""><title>a</title>
<ellipse fill=""none"" stroke=""black"" cx=""27"" cy=""-90"" rx=""27"" ry=""18""/>
<text text-anchor=""middle"" x=""27"" y=""-86.3"" font-family=""Times New Roman,serif"" font-size=""14.00"">a</text>
</g>
<!-- b -->
<g id=""node2"" class=""node""><title>b</title>
<ellipse fill=""none"" stroke=""black"" cx=""27"" cy=""-18"" rx=""27"" ry=""18""/>
<text text-anchor=""middle"" x=""27"" y=""-14.3"" font-family=""Times New Roman,serif"" font-size=""14.00"">b</text>
</g>
<!-- a&#45;&gt;b -->
<g id=""edge1"" class=""edge""><title>a&#45;&gt;b</title>
<path fill=""none"" stroke=""black"" d=""M27,-71.6966C27,-63.9827 27,-54.7125 27,-46.1124""/>
<polygon fill=""black"" stroke=""black"" points=""30.5001,-46.1043 27,-36.1043 23.5001,-46.1044 30.5001,-46.1043""/>
</g>
</g>
</svg>
";
    }
}