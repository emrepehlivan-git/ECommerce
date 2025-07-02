using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;

namespace ECommerce.Application.Features.Users.V1.Commands;

public sealed record UpdateUserBirthdayCommand(Guid UserId, DateTime? Birthday) : IRequest<Result>, ITransactionalRequest;

public sealed class UpdateUserBirthdayCommandHandler(
    IUserService userService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<UpdateUserBirthdayCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(UpdateUserBirthdayCommand command, CancellationToken cancellationToken)
    {
       return await userService.UpdateBirthdayAsync(command.UserId, command.Birthday);
    }
} 