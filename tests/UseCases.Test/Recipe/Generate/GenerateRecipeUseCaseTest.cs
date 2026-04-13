using CommonTestUtilities.Dtos;
using CommonTestUtilities.OpenAI;
using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Recipe.Generate;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;
using Xunit;

namespace UseCases.Test.Recipe.Generate;
public class GenerateRecipeUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var dto = GeneratedRecipeDtoBuilder.Build();

        var request = RequestGenerateRecipeJsonBuilder.Build();

        var useCase = CreateUseCase(dto);

        var result = await useCase.Execute(request);

        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.CookingTime.Should().Be((MyRecipeBook.Communication.Enums.CookingTime)dto.CookingTime);
        result.Difficulty.Should().Be((MyRecipeBook.Communication.Enums.Difficulty)dto.Difficulty);
    }

    [Theory]
    [InlineData(MyRecipeBook.Domain.Enums.Difficulty.Low)]
    [InlineData(MyRecipeBook.Domain.Enums.Difficulty.Medium)]
    [InlineData(MyRecipeBook.Domain.Enums.Difficulty.High)]
    public async Task Should_Map_Difficulty_From_Domain_To_Response(MyRecipeBook.Domain.Enums.Difficulty difficulty)
    {
        var dto = GeneratedRecipeDtoBuilder.Build() with
        {
            Difficulty = difficulty
        };

        var request = RequestGenerateRecipeJsonBuilder.Build();

        var useCase = CreateUseCase(dto);

        var result = await useCase.Execute(request);

        result.Difficulty.Should().Be((MyRecipeBook.Communication.Enums.Difficulty)difficulty);
    }

    [Fact]
    public async Task Error_Duplicated_Ingredients()
    {
        var dto = GeneratedRecipeDtoBuilder.Build();

        var request = RequestGenerateRecipeJsonBuilder.Build(count: 4);
        request.Ingredients.Add(request.Ingredients[0]);

        var useCase = CreateUseCase(dto);

        var act = async () => await useCase.Execute(request);

        (await act.Should().ThrowAsync<ValidationException>())
            .Where(e => e.GetErrorMessages().Count == 1 &&
                e.GetErrorMessages().Contains(ResourceMessagesException.DUPLICATED_INGREDIENTS_IN_LIST));
    }

    private static GenerateRecipeUseCase CreateUseCase(GeneratedRecipeDto dto)
    {
        var generateRecipeAI = GenerateRecipeAIBuilder.Build(dto);

        return new GenerateRecipeUseCase(generateRecipeAI);
    }
}
