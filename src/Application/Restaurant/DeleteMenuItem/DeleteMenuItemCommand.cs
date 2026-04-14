using Application.Abstractions.Messaging;

namespace Application.Restaurant.DeleteMenuItem;

public sealed record DeleteMenuItemCommand(Guid Id) : ICommand;
