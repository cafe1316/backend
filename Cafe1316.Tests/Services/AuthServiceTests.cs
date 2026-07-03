using Cafe1316.Application.Interfaces;
using Cafe1316.Application.Services;
using Cafe1316.Application.Settings;
using Cafe1316.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace Cafe1316.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            new Mock<IUserRepository>().Object,
            new Mock<IJwtService>().Object,
            Options.Create(new GoogleAuthSettings
            {
                ClientId = "cafe1316-test-client.apps.googleusercontent.com"
            }));
    }

    [Fact]
    public async Task GoogleLoginAsync_MissingToken_ThrowsBadRequestException()
    {
        Func<Task> act = () => _sut.GoogleLoginAsync(string.Empty);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*required*");
    }

    [Fact]
    public async Task GoogleLoginAsync_InvalidToken_ThrowsUnauthorizedException()
    {
        Func<Task> act = () => _sut.GoogleLoginAsync("not-a-valid-google-id-token");

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*invalid or expired*");
    }
}
