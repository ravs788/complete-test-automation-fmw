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
        <html><head><title>Contract page</title></head><body>
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
          <button data-testid="duplicate" onclick="document.querySelector('[data-testid=action]').textContent='clicked'">Duplicate</button>
          <button data-testid="duplicate">Duplicate</button>
          <button data-testid="mixed-disabled" disabled>Mixed</button><button data-testid="mixed-disabled">Mixed</button>
          <button data-testid="mixed-hidden" style="display:none">Hidden</button><button data-testid="mixed-hidden">Hidden</button>
          <input data-testid="duplicate-input" oninput="document.querySelector('[data-testid=action]').textContent=this.value">
          <input data-testid="duplicate-input">
          <div data-testid="aria-check" role="checkbox" tabindex="0" aria-checked="false" onclick="this.setAttribute('aria-checked', this.getAttribute('aria-checked') === 'true' ? 'false' : 'true'); document.querySelector('[data-testid=aria-checked-state]').textContent=this.getAttribute('aria-checked')">Custom checkbox</div>
          <p data-testid="aria-checked-state">false</p>
          <div data-testid="aria-check-prevented" role="checkbox" tabindex="0" aria-checked="false">Unchanging custom checkbox</div>
          <p data-testid="action">untouched</p>
          <label for="labelled-input">Contact email</label><input id="labelled-input" data-testid="labelled-input" oninput="document.querySelector('[data-testid=labelled-value]').textContent=this.value">
          <p data-testid="labelled-value"></p>
          <span id="label-first">Account</span><span id="label-last">name</span>
          <input data-testid="aria-labelled" aria-labelledby="label-first label-last" aria-label="Ignored label" oninput="document.querySelector('[data-testid=aria-value]').textContent=this.value">
          <p data-testid="aria-value"></p>
          <button style="display:none" aria-label="Role target">Hidden duplicate</button>
          <div aria-hidden="true"><button aria-label="Role target">ARIA hidden duplicate</button></div>
          <button aria-label="Role target" data-testid="role-target" onclick="document.querySelector('[data-testid=role-result]').textContent='clicked'">Visible target</button>
          <p data-testid="role-result">untouched</p>
          <button style="opacity:0" aria-label="Transparent role">Transparent</button>
          <button role="heading" aria-label="Override role">Overridden</button>
          <input data-testid="already-checked" type="checkbox" checked disabled onclick="document.querySelector('[data-testid=checked-clicks]').textContent='clicked'">
          <p data-testid="checked-clicks">untouched</p>
          <button data-testid="enable-checkbox" onclick="setTimeout(() => { document.querySelector('[data-testid=delayed-check]').disabled=false; }, 750)">Enable checkbox</button>
          <input data-testid="delayed-check" type="checkbox" disabled onchange="document.querySelector('[data-testid=delayed-state]').textContent='checked'">
          <p data-testid="delayed-state">unchecked</p>
          <input data-testid="prevented-check" type="checkbox" onclick="event.preventDefault()">
          <button data-testid="invalid-check" onclick="document.querySelector('[data-testid=checked-clicks]').textContent='clicked'">Not a checkbox</button>
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
        // Arrange: the fixture supplies a session on the contract page.
        // Assert: verify the initial title before interacting with the page.
        await Ui.Expect(TestId("title")).ToHaveTextAsync("Contract page");

        // Act
        await _session.Find(Locate.Placeholder("Your name")).FillAsync("Ann");
        await _session.Find(Locate.Role(UiRole.Button, "Greet")).ClickAsync();

        // Assert
        await Ui.Expect(TestId("greeting")).ToHaveTextAsync("Hello Ann");
        await Ui.Expect(_session.Find(Locate.Css("li.done"))).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task Role_Should_UseAssociatedLabels_And_AriaLabelledByPrecedence()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act
        await _session.Find(Locate.Role(UiRole.Textbox, "Contact email")).FillAsync("labelled");
        await _session.Find(Locate.Role(UiRole.Textbox, "Account name")).FillAsync("aria labelled");

        // Assert
        Assert.Equal(0, await _session.Find(Locate.Role(UiRole.Textbox, "Ignored label")).CountAsync());
        await Ui.Expect(TestId("labelled-value")).ToHaveTextAsync("labelled");
        await Ui.Expect(TestId("aria-value")).ToHaveTextAsync("aria labelled");
    }

    [Fact]
    public async Task Role_Should_ExcludeHiddenMatches_And_HonorExplicitRoles()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act
        var target = _session.Find(Locate.Role(UiRole.Button, "Role target"));

        // Assert
        Assert.Equal(1, await target.CountAsync());

        // Act: click the visible role match.
        await target.ClickAsync();

        // Assert: verify the click result and explicit role behavior.
        await Ui.Expect(TestId("role-result")).ToHaveTextAsync("clicked");
        Assert.Equal(0, await _session.Find(Locate.Role(UiRole.Button, "Override role")).CountAsync());
        Assert.Equal(1, await _session.Find(Locate.Role(UiRole.Heading, "Override role")).CountAsync());
        Assert.Equal(1, await _session.Find(Locate.Role(UiRole.Button, "Transparent role")).CountAsync());
    }

    [Fact]
    public async Task Check_Should_NotClickAnAlreadyCheckedDisabledCheckbox()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act
        await TestId("already-checked").CheckAsync();

        // Assert
        await Ui.Expect(TestId("checked-clicks")).ToHaveTextAsync("untouched");
    }

    [Fact]
    public async Task Check_Should_WaitForAnUncheckedCheckboxToBecomeEnabled()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act
        await TestId("enable-checkbox").ClickAsync();
        await TestId("delayed-check").CheckAsync();

        // Assert
        await Ui.Expect(TestId("delayed-state")).ToHaveTextAsync("checked");
    }

    [Fact]
    public async Task Check_Should_RejectInvalidTargetsWithoutClicking()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act & Assert
        await Assert.ThrowsAsync<UiException>(() => TestId("invalid-check").CheckAsync());

        // Assert: the rejected click leaves the page unchanged.
        await Ui.Expect(TestId("checked-clicks")).ToHaveTextAsync("untouched");
    }

    [Fact]
    public async Task Check_Should_FailWhenTheCheckboxRemainsUnchecked()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act & Assert
        await Assert.ThrowsAsync<UiException>(() => TestId("prevented-check").CheckAsync());
    }

    [Fact]
    public async Task Check_Should_CheckAriaCheckbox_And_PreserveCheckedStateOnRepeat()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act
        await TestId("aria-check").CheckAsync();

        // Assert
        await Ui.Expect(TestId("aria-checked-state")).ToHaveTextAsync("true");

        // Act: check the same element again.
        await TestId("aria-check").CheckAsync();

        // Assert: it remains checked.
        await Ui.Expect(TestId("aria-checked-state")).ToHaveTextAsync("true");
    }

    [Fact]
    public async Task Check_Should_FailWhenAnAriaCheckboxRemainsUnchecked()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act & Assert
        await Assert.ThrowsAsync<UiException>(() => TestId("aria-check-prevented").CheckAsync());
    }

    [Fact]
    public async Task WithChild_Should_KeepOnlyElementsWhoseChildHasTheText()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act
        var banana = TestId("item").WithChild(Locate.TestId("data-testid", "label"), "banana");

        // Assert
        await Ui.Expect(banana).ToHaveCountAsync(1);
        await Ui.Expect(banana).ToHaveClassAsync("done");

        // Act: interact with the element selected by its child text.
        await banana.Find(Locate.Role(UiRole.Button, "Remove")).ClickAsync();

        // Assert: the selected element is still the Banana item.
        Assert.Contains("Banana", await banana.TextAsync());
    }

    [Fact]
    public async Task Collections_Should_SupportFirst_Count_And_AllTexts()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act
        var labels = TestId("item").Find(Locate.TestId("data-testid", "label"));

        // Assert
        Assert.Equal(2, await labels.CountAsync());
        Assert.Equal("Apple", await labels.First.TextAsync());
        await Ui.Expect(labels).ToHaveTextAsync(["Apple", "Banana"]);
        await Ui.Expect(TestId("item").First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Check_And_Press_Should_ActOnTheElement()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act
        await _session.Find(Locate.Role(UiRole.Checkbox, "Agree")).CheckAsync();
        await TestId("key").PressAsync("Enter");

        // Assert
        await Ui.Expect(TestId("agreed")).ToHaveTextAsync("yes");
        await Ui.Expect(TestId("pressed")).ToContainTextAsync("ENTERED", ignoreCase: true);
    }

    [Fact]
    public async Task IsVisible_Should_BeFalse_ForHiddenElements()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act & Assert
        Assert.False(await TestId("hidden").IsVisibleAsync());
        Assert.True(await TestId("title").IsVisibleAsync());
    }

    [Fact]
    public async Task FailedAssertion_Should_Throw_UiAssertionException()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act & Assert
        var ex = await Assert.ThrowsAsync<UiAssertionException>(() => Ui.Expect(TestId("title")).ToHaveTextAsync("Wrong title"));

        // Assert: the failure identifies the expected text.
        Assert.Contains("Wrong title", ex.Message);
    }

    [Fact]
    public async Task ActionOnMissingElement_Should_Throw_UiException()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act & Assert
        await Assert.ThrowsAsync<UiException>(() => TestId("does-not-exist").ClickAsync());
    }

    [Theory]
    [InlineData("duplicate")]
    [InlineData("mixed-disabled")]
    [InlineData("mixed-hidden")]
    public async Task AmbiguousClick_Should_FailWithoutSelectingAnElement(string id)
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act & Assert
        await Assert.ThrowsAsync<UiException>(() => TestId(id).ClickAsync());

        // Assert: no matching element was clicked.
        await Ui.Expect(TestId("action")).ToHaveTextAsync("untouched");
    }

    [Fact]
    public async Task AmbiguousFill_Should_FailWithoutChangingValues()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act & Assert
        await Assert.ThrowsAsync<UiException>(() => TestId("duplicate-input").FillAsync("changed"));

        // Assert: the ambiguous fill changed nothing.
        await Ui.Expect(TestId("action")).ToHaveTextAsync("untouched");

        // Act: explicitly choose the first input.
        await TestId("duplicate-input").First.FillAsync("chosen explicitly");

        // Assert: the explicit selection accepts the value.
        await Ui.Expect(TestId("action")).ToHaveTextAsync("chosen explicitly");
    }

    [Fact]
    public async Task AmbiguousScalarReadsAndAssertions_Should_Fail_WhileCollectionsAndFirstWork()
    {
        // Arrange
        var elements = TestId("duplicate");

        // Act & Assert
        await Assert.ThrowsAsync<UiException>(() => elements.TextAsync());
        await Assert.ThrowsAsync<UiException>(() => elements.IsVisibleAsync());
        await Assert.ThrowsAsync<UiAssertionException>(() => Ui.Expect(elements).ToBeVisibleAsync());
        await Assert.ThrowsAsync<UiAssertionException>(() => Ui.Expect(elements).ToHaveTextAsync("Duplicate"));

        // Assert: collection operations remain supported.
        await Ui.Expect(elements).ToHaveCountAsync(2);
        await Ui.Expect(elements).ToHaveTextAsync(["Duplicate", "Duplicate"]);

        // Act: explicitly select the first match.
        await elements.First.ClickAsync();

        // Assert: the selected element was clicked.
        await Ui.Expect(TestId("action")).ToHaveTextAsync("clicked");
    }

    [Fact]
    public async Task Title_Should_BeThePagesTitle()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act & Assert
        Assert.Equal("Contract page", await _session.TitleAsync());
    }

    [Fact]
    public async Task Capabilities_Should_BeOfferedThroughTryGet()
    {
        // Arrange: the fixture supplies a session on the contract page.
        // Act & Assert
        Assert.True(_session.TryGet<IScreenshots>(out var screenshots));

        // Act: capture a screenshot through the advertised capability.
        var png = await screenshots.CaptureAsync();

        // Assert: the data is PNG and unrelated capabilities are unavailable.
        Assert.Equal([0x89, (byte)'P', (byte)'N', (byte)'G'], png.Take(4).ToArray());

        Assert.False(_session.TryGet<IDisposable>(out _));
    }
}
