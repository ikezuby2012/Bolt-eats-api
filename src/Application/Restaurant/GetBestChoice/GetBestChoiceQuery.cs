using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;


namespace Application.Restaurant.GetBestChoice;
public sealed record GetBestChoiceQuery(
    double MinRating = 3.5,
    int Limit = 10)
    : IQuery<IReadOnlyList<HomeMenuItemDto>>;
