using AutoMapper;
using MyRecipeBook.Communication.Enums;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.ValueObjects;
using Sqids;

namespace MyRecipeBook.Application.Services.AutoMapper;

public class AutoMapping : Profile
{
    private readonly SqidsEncoder<long> _idEnconder;

    public AutoMapping(SqidsEncoder<long> idEnconder)
    {
        _idEnconder = idEnconder;

        RequestToDomain();
        DomainToResponse();
    }

    private void RequestToDomain()
    {
        CreateMap<RequestRecipeJson, Domain.Entities.Recipe>()
            .ForMember(dest => dest.Title, opt => opt.Ignore())
            .ForMember(dest => dest.CookingTime, opt => opt.Ignore())
            .ForMember(dest => dest.Difficulty, opt => opt.Ignore())
            .ForMember(dest => dest.Instructions, opt => opt.Ignore())
            .ForMember(dest => dest.Ingredients, opt => opt.Ignore())
            .ForMember(dest => dest.DishTypes, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIdentifier, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());

        CreateMap<RequestInstructionJson, Domain.Entities.Instruction>()
            .ConstructUsing(source => Domain.Entities.Instruction.Create(source.Step, source.Text));
    }

    private void DomainToResponse()
    {
        CreateMap<Email, string>().ConvertUsing(source => source.Value);
        CreateMap<RecipeTitle, string>().ConvertUsing(source => source.Value);

        CreateMap<Domain.Entities.User, ResponseUserProfileJson>();

        CreateMap<Domain.Entities.Recipe, ResponseRegiteredRecipeJson>()
            .ForMember(dest => dest.Id, config => config.MapFrom(source => _idEnconder.Encode(source.Id)));

        CreateMap<Domain.Entities.Recipe, ResponseShortRecipeJson>()
            .ForMember(dest => dest.Id, config => config.MapFrom(source => _idEnconder.Encode(source.Id)))
            .ForMember(dest => dest.AmountIngredients, config => config.MapFrom(source => source.Ingredients.Count));

        CreateMap<Domain.Entities.Recipe, ResponseRecipeJson>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(source => _idEnconder.Encode(source.Id)))
            .ForMember(dest => dest.DishTypes, opt => opt.MapFrom(source => source.DishTypes.Select(r => r.Type)));

        CreateMap<Domain.Entities.Ingredient, ResponseIngredientJson>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(source => _idEnconder.Encode(source.Id)));

        CreateMap<Domain.Entities.Instruction, ResponseInstructionJson>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(source => _idEnconder.Encode(source.Id)));
    }
}
