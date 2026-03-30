using System.Net;
using Common.Domain.Interfaces;
using EmailService.Application.Features.EmailConfirmations.SendCode;
using EmailService.Domain.Interfaces;
using Moq;

namespace OLXKiller.Tests.EmailServiceTests;

public class SendVerificationCodeCommandHandlerTests
{
    private readonly Mock<IVerificationCodeGenerator> _codeGeneratorMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ICacheService<string>> _cacheServiceMock = new();

    private readonly SendVerificationCodeCommandHandler _handler;
    
    private const string TestEmail = "test@example.com";
    private const string TestCode = "123456";
    private const string HashedCode = "hashed_123456";
    
    public SendVerificationCodeCommandHandlerTests()
    {
        _handler = new SendVerificationCodeCommandHandler(
            _codeGeneratorMock.Object,
            _emailSenderMock.Object,
            _passwordHasherMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_CodeSentSuccessfully()
    {
        // Arrange
        var command = new SendVerificationCodeCommand(TestEmail);
        
        _codeGeneratorMock
            .Setup(x => x.Generate())
            .Returns(TestCode);
            
        _passwordHasherMock
            .Setup(x => x.HashPassword(TestCode))
            .Returns(HashedCode);
            
        _cacheServiceMock
            .Setup(x => x.SetAsync(
                TestEmail, 
                HashedCode, 
                TimeSpan.FromMinutes(3), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _emailSenderMock
            .Setup(x => x.SendMessageAsync(
                TestEmail, 
                "Verification Code", 
                $"Your verification code is: {TestCode}", 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        
        _codeGeneratorMock.Verify(x => x.Generate(), Times.Once);
        _passwordHasherMock.Verify(x => x.HashPassword(TestCode), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync(
            TestEmail, 
            HashedCode, 
            TimeSpan.FromMinutes(3), 
            CancellationToken.None), Times.Once);
        _emailSenderMock.Verify(x => x.SendMessageAsync(
            TestEmail, 
            "Verification Code", 
            $"Your verification code is: {TestCode}",
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_UsesGeneratedCode()
    {
        // Arrange
        var command = new SendVerificationCodeCommand(TestEmail);
        const string generatedCode = "654321";
        const string expectedHashedCode = "hashed_654321";
        
        _codeGeneratorMock
            .Setup(x => x.Generate())
            .Returns(generatedCode);
            
        _passwordHasherMock
            .Setup(x => x.HashPassword(generatedCode))
            .Returns(expectedHashedCode);
            
        _cacheServiceMock
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _emailSenderMock
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _passwordHasherMock.Verify(x => x.HashPassword(generatedCode), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync(
            TestEmail, 
            expectedHashedCode, 
            TimeSpan.FromMinutes(3), 
            CancellationToken.None), Times.Once);
        _emailSenderMock.Verify(x => x.SendMessageAsync(
            TestEmail, 
            "Verification Code", 
            $"Your verification code is: {generatedCode}",
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_SetsCacheWithCorrectExpiration()
    {
        // Arrange
        var command = new SendVerificationCodeCommand(TestEmail);
        
        TimeSpan? actualExpiration = null;
        
        _codeGeneratorMock
            .Setup(x => x.Generate())
            .Returns(TestCode);
            
        _passwordHasherMock
            .Setup(x => x.HashPassword(TestCode))
            .Returns(HashedCode);
            
        _cacheServiceMock
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, TimeSpan, CancellationToken>((_, _, expiration, _) => actualExpiration = expiration)
            .Returns(Task.CompletedTask);
            
        _emailSenderMock
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(actualExpiration);
        Assert.Equal(TimeSpan.FromMinutes(3), actualExpiration);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_CodeGenerationFails()
    {
        // Arrange
        var command = new SendVerificationCodeCommand(TestEmail);
        
        _codeGeneratorMock
            .Setup(x => x.Generate())
            .Throws(new Exception("Code generation failed"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Code generation failed", exception.Message);
        
        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_CacheFails()
    {
        // Arrange
        var command = new SendVerificationCodeCommand(TestEmail);
        
        _codeGeneratorMock
            .Setup(x => x.Generate())
            .Returns(TestCode);
            
        _passwordHasherMock
            .Setup(x => x.HashPassword(TestCode))
            .Returns(HashedCode);
            
        _cacheServiceMock
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Cache error", exception.Message);
        
        _emailSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_EmailSendingFails()
    {
        // Arrange
        var command = new SendVerificationCodeCommand(TestEmail);
        
        _codeGeneratorMock
            .Setup(x => x.Generate())
            .Returns(TestCode);
            
        _passwordHasherMock
            .Setup(x => x.HashPassword(TestCode))
            .Returns(HashedCode);
            
        _cacheServiceMock
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _emailSenderMock
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("SMTP error", exception.Message);
        
        _cacheServiceMock.Verify(x => x.SetAsync(
            TestEmail, 
            HashedCode, 
            TimeSpan.FromMinutes(3), 
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var command = new SendVerificationCodeCommand(TestEmail);
        var cancellationToken = new CancellationToken(true);
        
        _codeGeneratorMock
            .Setup(x => x.Generate())
            .Returns(TestCode);
            
        _passwordHasherMock
            .Setup(x => x.HashPassword(TestCode))
            .Returns(HashedCode);
            
        _cacheServiceMock
            .Setup(x => x.SetAsync(TestEmail, HashedCode, TimeSpan.FromMinutes(3), cancellationToken))
            .Returns(Task.CompletedTask);
            
        _emailSenderMock
            .Setup(x => x.SendMessageAsync(TestEmail, "Verification Code", $"Your verification code is: {TestCode}", cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        
        _cacheServiceMock.Verify(x => x.SetAsync(TestEmail, HashedCode, TimeSpan.FromMinutes(3), cancellationToken), Times.Once);
        _emailSenderMock.Verify(x => x.SendMessageAsync(TestEmail, "Verification Code", $"Your verification code is: {TestCode}", cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_SendsEmailWithCorrectContent()
    {
        // Arrange
        var command = new SendVerificationCodeCommand(TestEmail);
        var generatedCode = "ABC123";
        
        string? actualEmail = null;
        string? actualSubject = null;
        string? actualMessage = null;
        
        _codeGeneratorMock
            .Setup(x => x.Generate())
            .Returns(generatedCode);
            
        _passwordHasherMock
            .Setup(x => x.HashPassword(generatedCode))
            .Returns("hashed_ABC123");
            
        _cacheServiceMock
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _emailSenderMock
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((email, subject, message, _) =>
            {
                actualEmail = email;
                actualSubject = subject;
                actualMessage = message;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TestEmail, actualEmail);
        Assert.Equal("Verification Code", actualSubject);
        Assert.Equal($"Your verification code is: {generatedCode}", actualMessage);
    }

    [Fact]
    public async Task Handler_HashesCodeBeforeCaching()
    {
        // Arrange
        var command = new SendVerificationCodeCommand(TestEmail);
        const string plainCode = "654321";
        const string hashedCode = "hashed_654321";
        
        string? cachedValue = null;
        
        _codeGeneratorMock
            .Setup(x => x.Generate())
            .Returns(plainCode);
            
        _passwordHasherMock
            .Setup(x => x.HashPassword(plainCode))
            .Returns(hashedCode);
            
        _cacheServiceMock
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, TimeSpan, CancellationToken>((_, value, _, _) => cachedValue = value)
            .Returns(Task.CompletedTask);
            
        _emailSenderMock
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(hashedCode, cachedValue); 
        Assert.NotEqual(plainCode, cachedValue); 
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_ForDifferentEmailFormats()
    {
        var testCases = new[]
        {
            "user@example.com",
            "user.name@example.com",
            "user+tag@example.com",
            "user@sub.example.com",
            "user@example.co.uk"
        };

        foreach (var email in testCases)
        {
            // Arrange
            var command = new SendVerificationCodeCommand(email);
            
            _codeGeneratorMock
                .Setup(x => x.Generate())
                .Returns(TestCode);
                
            _passwordHasherMock
                .Setup(x => x.HashPassword(TestCode))
                .Returns(HashedCode);
                
            _cacheServiceMock
                .Setup(x => x.SetAsync(email, It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            _emailSenderMock
                .Setup(x => x.SendMessageAsync(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess, $"Failed for email: {email}");
            
            _codeGeneratorMock.Invocations.Clear();
            _passwordHasherMock.Invocations.Clear();
            _cacheServiceMock.Invocations.Clear();
            _emailSenderMock.Invocations.Clear();
        }
    }
}