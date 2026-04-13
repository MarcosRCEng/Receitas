using CommonTestUtilities.Requests;
using CommonTestUtilities.Tokens;
using FluentAssertions;
using MyRecipeBook.Exceptions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Recipe.Update;
public class UpdateRecipeTest : MyRecipeBookClassFixture
{
    private const string METHOD = "recipe";

    private readonly Guid _userIdentifier;
    private readonly string _recipeId;

    public UpdateRecipeTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _userIdentifier = factory.GetUserIdentifier();
        _recipeId = factory.GetRecipeId();
    }

    [Fact]
    public async Task Success()
    {
        var request = RequestRecipeJsonBuilder.Build();
        request.Title = "Updated recipe title";
        request.Instructions[0].Step = 3;
        request.Instructions[1].Step = 1;
        request.Instructions[2].Step = 2;

        var token = JwtTokenGeneratorBuilder.Build().Generate(_userIdentifier);

        var response = await DoPut($"{METHOD}/{_recipeId}", request, token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await DoGet($"{METHOD}/{_recipeId}", token);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await getResponse.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        responseData.RootElement.GetProperty("title").GetString().Should().Be(request.Title);
        responseData.RootElement.GetProperty("instructions").EnumerateArray().Select(instruction => instruction.GetProperty("step").GetInt32()).Should().Equal([1, 2, 3]);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Title_Empty(string culture)
    {
        var request = RequestRecipeJsonBuilder.Build();
        request.Title = string.Empty;

        var token = JwtTokenGeneratorBuilder.Build().Generate(_userIdentifier);

        var response = await DoPut($"{METHOD}/{_recipeId}", request, token, culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("RECIPE_TITLE_EMPTY", new CultureInfo(culture));

        errors.Should().HaveCount(1).And.Contain(c => c.GetString()!.Equals(expectedMessage));
    }
}
