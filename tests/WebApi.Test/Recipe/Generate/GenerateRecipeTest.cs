using CommonTestUtilities.Requests;
using CommonTestUtilities.Tokens;
using FluentAssertions;
using MyRecipeBook.Exceptions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Recipe.Generate;

public class GenerateRecipeTest : MyRecipeBookClassFixture
{
    private const string METHOD = "recipe/generate";

    private readonly Guid _userIdentifier;
    private readonly MyRecipeBook.Domain.Dtos.GeneratedRecipeDto _generatedRecipe;

    public GenerateRecipeTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _userIdentifier = factory.GetUserIdentifier();
        _generatedRecipe = factory.GetGeneratedRecipe();
    }

    [Fact]
    public async Task Success()
    {
        var request = RequestGenerateRecipeJsonBuilder.Build();

        var token = JwtTokenGeneratorBuilder.Build().Generate(_userIdentifier);

        var response = await DoPost(METHOD, request, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        responseData.RootElement.GetProperty("title").GetString().Should().Be(_generatedRecipe.Title);
        responseData.RootElement.GetProperty("cookingTime").GetInt32().Should().Be((int)_generatedRecipe.CookingTime);
        responseData.RootElement.GetProperty("difficulty").GetInt32().Should().Be((int)_generatedRecipe.Difficulty);
        responseData.RootElement.GetProperty("ingredients").EnumerateArray().Select(item => item.GetString()).Should().Equal(_generatedRecipe.Ingredients);
        responseData.RootElement.GetProperty("instructions").EnumerateArray().Should().HaveCount(_generatedRecipe.Instructions.Count);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Duplicated_Ingredients(string culture)
    {
        var request = RequestGenerateRecipeJsonBuilder.Build(count: 4);
        request.Ingredients.Add(request.Ingredients[0]);

        var token = JwtTokenGeneratorBuilder.Build().Generate(_userIdentifier);

        var response = await DoPost(METHOD, request, token, culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("DUPLICATED_INGREDIENTS_IN_LIST", new CultureInfo(culture));

        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
