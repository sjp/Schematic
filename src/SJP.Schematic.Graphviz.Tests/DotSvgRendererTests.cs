using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;

namespace SJP.Schematic.Graphviz.Tests
{
    [TestFixture]
    internal sealed class DotSvgRendererTests
    {
        private IGraphvizExecutable GraphvizExe { get; set; }

        private DotSvgRenderer Renderer => new DotSvgRenderer(GraphvizExe.DotPath);

        [OneTimeSetUp]
        public void Init()
        {
            var factory = new GraphvizExecutableFactory();
            GraphvizExe = factory.GetExecutable();
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            GraphvizExe.Dispose();
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public void DotRenderer_GivenNullOrWhiteSpaceExecutablePath_ThrowsArgumentNullException(string exePath)
        {
            Assert.That(() => new DotSvgRenderer(exePath), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public void RenderToSvg_GivenNullOrWhiteSpaceDotDiagram_ThrowsArgumentNullException(string dot)
        {
            Assert.That(() => Renderer.RenderToSvg(dot), Throws.ArgumentNullException);
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

        [Test]
        public void RenderToSvg_GivenVizJsGraphExample_ReturnsValidSvgXml()
        {
            var svg = Renderer.RenderToSvg(VizJsExample);

            Assert.That(() => _ = XDocument.Parse(svg, LoadOptions.PreserveWhitespace), Throws.Nothing);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public void RenderToSvgAsync_GivenNullOrWhiteSpaceDotDiagram_ThrowsArgumentNullException(string dot)
        {
            Assert.That(() => Renderer.RenderToSvgAsync(dot), Throws.ArgumentNullException);
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
    }
}