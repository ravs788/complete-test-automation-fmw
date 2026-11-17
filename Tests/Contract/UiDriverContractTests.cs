using Core.Ui;
using Core.Ui.Capabilities;

namespace Tests.Contract;

/// <summary>
/// The behaviour every UI driver adapter must have (R42, R46). Each driver's test class inherits these tests.
/// </summary>
public abstract class UiDriverContractTests<TFactory>(UiDriverFixture<TFactory> fixture) : IClassFixture<UiDriverFixture<TFactory>>, IAsyncLifetime
    where TFactory : IUiDriverFactory, new()
{
    private const string Html = """
        <!doctype html>
        <html><body>
          <h1 data-testid="title">Contract page</h1>
          <input data-testid="name" placeholder="Your name">
          <button onclick="document.querySelector('[data-testid=greeting]').textContent = 'Hello ' + document.querySelector('[data-testid=name]').value">Greet</button>
          <p data-testid="greeting"></p>
          <ul>
            <li data-testid="item"><span data-testid="label">Apple</span> <button>Remove</button></li>
            <li data-testid="item" class="done active"><span data-testid="label">Banana</span> <button>Remove</button></li>
          </ul>
          <label><input type="checkbox" aria-label="Agree" onchange="document.querySelector('[data-testid=agreed]').textContent = this.checked ? 'yes' : 'no'"> Agree</label>
          <p data-testid="agreed">no</p>
          <input data-testid="key" onkeydown="if (event.key === 'Enter') document.querySelector('[data-testid=pressed]').textContent = 'entered'">
          <p data-testid="pressed"></p>
          <p data-testid="hidden" style="display:none">secret</p>
        </body></html>
        """;

    private IUiSession _session = null!;

    private IElement TestId(string id) => _session.Find(Locate.TestId("data-testid", id));

    public async Task InitializeAsync()
    {
        _session = await fixture.NewSessionAsync();
        await _session.NavigateAsync("data:text/html," + Uri.EscapeDataString(Html));
    }

    public async Task DisposeAsync() => await _session.DisposeAsync();

    [Fact]
    public async Task Elements_Should_BeFound_ByTestId_Role_Placeholder_And_Css()
    {
        await Ui.Expect(TestId("title")).ToHaveTextAsync("Contract page");
        await _session.Find(Locate.Placeholder("Your name")).FillAsync("Ann");
        await _session.Find(Locate.Role(UiRole.Button, "Greet")).ClickAsync();

        await Ui.Expect(TestId("greeting")).ToHaveTextAsync("Hello Ann");
        await Ui.Expect(_session.Find(Locate.Css("li.done"))).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task WithChild_Should_KeepOnlyElementsWhoseChildHasTheText()
    {
        var banana = TestId("item").WithChild(Locate.TestId("data-testid", "label"), "banana");

        await Ui.Expect(banana).ToHaveCountAsync(1);
        await Ui.Expect(banana).ToHaveClassAsync("done");
        await banana.Find(Locate.Role(UiRole.Button, "Remove")).ClickAsync();
        Assert.Contains("Banana", await banana.TextAsync());
    }

    [Fact]
    public async Task Collections_Should_SupportFirst_Count_And_AllTexts()
    {
        var labels = TestId("item").Find(Locate.TestId("data-testid", "label"));

        Assert.Equal(2, await labels.CountAsync());
        Assert.Equal("Apple", await labels.First.TextAsync());
        await Ui.Expect(labels).ToHaveTextAsync(["Apple", "Banana"]);
        await Ui.Expect(TestId("item").First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Check_And_Press_Should_ActOnTheElement()
    {
        await _session.Find(Locate.Role(UiRole.Checkbox, "Agree")).CheckAsync();
        await TestId("key").PressAsync("Enter");

        await Ui.Expect(TestId("agreed")).ToHaveTextAsync("yes");
        await Ui.Expect(TestId("pressed")).ToContainTextAsync("ENTERED", ignoreCase: true);
    }

    [Fact]
    public async Task IsVisible_Should_BeFalse_ForHiddenElements()
    {
        Assert.False(await TestId("hidden").IsVisibleAsync());
        Assert.True(await TestId("title").IsVisibleAsync());
    }

    [Fact]
    public async Task FailedAssertion_Should_Throw_UiAssertionException()
    {
        var ex = await Assert.ThrowsAsync<UiAssertionException>(() => Ui.Expect(TestId("title")).ToHaveTextAsync("Wrong title"));

        Assert.Contains("Wrong title", ex.Message);
    }

    [Fact]
    public async Task ActionOnMissingElement_Should_Throw_UiException()
    {
        await Assert.ThrowsAsync<UiException>(() => TestId("does-not-exist").ClickAsync());
    }

    [Fact]
    public async Task Capabilities_Should_BeOfferedThroughTryGet()
    {
        Assert.True(_session.TryGet<IScreenshots>(out var screenshots));
        var png = await screenshots.CaptureAsync();
        Assert.Equal([0x89, (byte)'P', (byte)'N', (byte)'G'], png.Take(4).ToArray());

        Assert.False(_session.TryGet<IDisposable>(out _));
    }
}
