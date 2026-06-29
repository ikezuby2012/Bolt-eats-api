using Application.Abstractions.Messaging;
using Application.Auth.Dto;
using Application.Auth.Login;
using Application.Auth.RefreshTokenAsync;
using Application.Auth.Register;
using Application.Auth.ResendOtp;
using Application.Auth.VerifyUser;
using Application.Users.Dto;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

public class Auth : IEndpoint
{
    internal sealed record LoginRequest(string Email, string Password);
    internal sealed record RegisterRequest(string Email, string FirstName, string LastName, string Phone, string Password);
    internal sealed record VerifyRequest(string Email, string Otp);
    internal sealed record RefreshTokenRequest(string accesstoken, string refreshToken);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (LoginRequest request, ICommandHandler<LoginUserCommand, LoginSuccessDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(request.Email, request.Password);

            Result<LoginSuccessDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<LoginSuccessDto>.Success(value, $"User loggedin successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Auth);

        ///
        ///<summary>
        ///
        /// </summary>
        app.MapPost("auth/register", async ([FromBody] RegisterRequest request, ICommandHandler<RegisterUserCommand, CreatedUserDto> handler, CancellationToken cancellationToken) =>
        {
            Result<CreatedUserDto> result;

            var command = new RegisterUserCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Password);

            result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<CreatedUserDto>.Success(value, $"User registered successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Auth);


        ///
        ///<summary>
        ///
        /// </summary>
        app.MapPost("auth/verify-user", async (VerifyRequest request, ICommandHandler<VerifyUserCommand, LoginSuccessDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new VerifyUserCommand(request.Email, request.Otp);
            Result<LoginSuccessDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<LoginSuccessDto>.Success(value, $"User verified successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Auth);

        ///
        ///<summary>
        ///</summary>
        app.MapGet("auth/resend-otp", async ([FromQuery] string email, ICommandHandler<ResendOtpCommand, Guid> handler, CancellationToken cancellationToken) =>
        {
            var command = new ResendOtpCommand(email);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<Guid>.Success(value, $"OTP sent to user mail box successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Auth);

        app.MapPost("auth/refresh-token", async ([FromBody] RefreshTokenRequest body, ICommandHandler<RefreshTokenCommand, LoginSuccessDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new RefreshTokenCommand(body.accesstoken, body.refreshToken);

            Result<LoginSuccessDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<LoginSuccessDto>.Success(value, $"User loggedin successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Auth);
    }
}
