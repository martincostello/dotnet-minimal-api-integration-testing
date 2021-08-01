// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using TodoApp.Models;

namespace TodoApp;

[Collection(TodoAppCollection.Name)]
public class ApiTests
{
    public ApiTests(TodoAppFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;

        // Route output from the fixture's logs to xunit's output
        OutputHelper = outputHelper;
        Fixture.SetOutputHelper(OutputHelper);
    }

    private TodoAppFixture Fixture { get; }

    private ITestOutputHelper OutputHelper { get; }

    [Fact]
    public async Task Can_Manage_Todo_Items_With_Api()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act - Get all the items
        var items = await client.GetFromJsonAsync<TodoListViewModel>("/api/items");

        // Assert - There should be no items
        items.ShouldNotBeNull();
        items.Items.ShouldNotBeNull();

        var beforeCount = items.Items.Count;

        // Arrange
        var text = "Buy eggs";
        var newItem = new CreateTodoItemModel { Text = text };

        // Act - Add a new item
        using var createdResponse = await client.PostAsJsonAsync("/api/items", newItem);

        // Assert - An item was created
        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createdResponse.Headers.Location.ShouldNotBeNull();

        using var createdJson = await createdResponse.Content.ReadFromJsonAsync<JsonDocument>();

        // Arrange - Get the new item's URL and Id
        var itemUri = createdResponse.Headers.Location;
        var itemId = createdJson!.RootElement.GetProperty("id").GetString();

        // Act - Get the item
        var item = await client.GetFromJsonAsync<TodoItemModel>(itemUri);

        // Assert - Verify the item was created correctly
        item.ShouldNotBeNull();
        item.Id.ShouldBe(itemId);
        item.IsCompleted.ShouldBeFalse();
        item.LastUpdated.ShouldNotBeNull();
        item.Text.ShouldBe(text);

        // Act - Mark the item as being completed
        using var completedResponse = await client.PostAsJsonAsync(itemUri + "/complete", new { });

        // Assert - The item was completed
        completedResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        item = await client.GetFromJsonAsync<TodoItemModel>(itemUri);

        item.ShouldNotBeNull();
        item.Id.ShouldBe(itemId);
        item.Text.ShouldBe(text);
        item.IsCompleted.ShouldBeTrue();

        // Act - Get all the items
        items = await client.GetFromJsonAsync<TodoListViewModel>("/api/items");

        // Assert - The item was completed
        items.ShouldNotBeNull();
        items.Items.ShouldNotBeNull();
        items.Items.Count.ShouldBe(beforeCount + 1);
        item = items.Items.Last();

        item.ShouldNotBeNull();
        item.Id.ShouldBe(itemId);
        item.Text.ShouldBe(text);
        item.IsCompleted.ShouldBeTrue();
        item.LastUpdated.ShouldNotBeNull();

        // Act - Delete the item
        using var deletedResponse = await client.DeleteAsync(itemUri);

        // Assert - The item no longer exists
        deletedResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        items = await client.GetFromJsonAsync<TodoListViewModel>("/api/items");

        items.ShouldNotBeNull();
        items.Items.ShouldNotBeNull();
        items.Items.Count.ShouldBe(beforeCount);
        items.Items.ShouldNotContain(p => p.Id == itemId);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetFromJsonAsync<TodoItemModel>(itemUri));

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Cannot_Create_Todo_Item_With_No_Text()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var item = new CreateTodoItemModel { Text = string.Empty };

        // Act
        var response = await client.PostAsJsonAsync("/api/items", item);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.ShouldNotBeNull();
        problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problem.Title.ShouldBe("Bad Request");
        problem.Detail.ShouldBe("No item text specified.");
        problem.Type.ShouldBe("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        problem.Instance.ShouldBeNull();
    }

    [Fact]
    public async Task Cannot_Complete_Todo_Item_Multiple_Times()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var item = new CreateTodoItemModel { Text = "Something" };

        using var createdResponse = await client.PostAsJsonAsync("/api/items", item);
        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createdResponse.Headers.Location.ShouldNotBeNull();

        var itemUri = createdResponse.Headers.Location;

        using var completedResponse = await client.PostAsJsonAsync(itemUri + "/complete", new { });
        completedResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Act
        using var response = await client.PostAsJsonAsync(itemUri + "/complete", new { });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.ShouldNotBeNull();
        problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problem.Title.ShouldBe("Bad Request");
        problem.Detail.ShouldBe("Item already completed.");
        problem.Type.ShouldBe("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        problem.Instance.ShouldBeNull();
    }

    [Fact]
    public async Task Cannot_Complete_Deleted_Todo_Item()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var item = new CreateTodoItemModel { Text = "Something" };

        using var createdResponse = await client.PostAsJsonAsync("/api/items", item);
        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createdResponse.Headers.Location.ShouldNotBeNull();

        var itemUri = createdResponse.Headers.Location;

        using var deletedResponse = await client.DeleteAsync(itemUri);
        deletedResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Act
        using var response = await client.PostAsJsonAsync(itemUri + "/complete", new { });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.ShouldNotBeNull();
        problem.Status.ShouldBe(StatusCodes.Status404NotFound);
        problem.Title.ShouldBe("Not Found");
        problem.Detail.ShouldBe("Item not found.");
        problem.Type.ShouldBe("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        problem.Instance.ShouldBeNull();
    }

    [Fact]
    public async Task Cannot_Delete_Todo_Item_Multiple_Times()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var item = new CreateTodoItemModel { Text = "Something" };

        using var createdResponse = await client.PostAsJsonAsync("/api/items", item);
        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createdResponse.Headers.Location.ShouldNotBeNull();

        var itemUri = createdResponse.Headers.Location;

        using var deletedResponse = await client.DeleteAsync(itemUri);
        deletedResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Act
        using var response = await client.DeleteAsync(itemUri);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.ShouldNotBeNull();
        problem.Status.ShouldBe(StatusCodes.Status404NotFound);
        problem.Title.ShouldBe("Not Found");
        problem.Detail.ShouldBe("Item not found.");
        problem.Type.ShouldBe("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        problem.Instance.ShouldBeNull();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var options = new WebApplicationFactoryClientOptions()
        {
            AllowAutoRedirect = true,
            BaseAddress = Fixture.ClientOptions.BaseAddress,
            HandleCookies = true,
        };

        var client = Fixture.CreateClient(options);

        var parameters = Array.Empty<KeyValuePair<string?, string?>>();
        using var content = new FormUrlEncodedContent(parameters);

        // Go through the sign-in flow, which will set
        // the authentication cookie on the HttpClient.
        using var response = await client.PostAsync("/signin", content);
        response.IsSuccessStatusCode.ShouldBeTrue();

        return client;
    }
}
