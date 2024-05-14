using Contract.Services;
using Domain.DTOs.UserDTO;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces.Services;
using Domain.ResponseMessage;
using Infrastructure.Service;
using Infrastructure.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EventManagerTest
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ICurrentUserService> _mockCurrentUserService;
        private Mock<IEmailService> _mockEmailService;
        private Mock<ILogger<UserService>> _mockLogger;
        private Mock<IHashService> _mockHashService;
        private IUserService _userService;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockHashService = new Mock<IHashService>();
            _userService = new UserService(
                _mockUnitOfWork.Object,
                _mockCurrentUserService.Object,
                _mockEmailService.Object,
                _mockLogger.Object,
                _mockHashService.Object);

        }

        [Test]
        public async Task Register_ValidUser_Success()
        {
            // Arrange
            var userForCreate = new UserForCreate { Username = "testuser", Password = "password", Email = "test@example.com" };
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>()))
                .Returns(Enumerable.Empty<User>());

            _mockHashService.Setup(h => h.HashPassword(It.IsAny<string>()))
                .Returns(new PasswordHashResult { Salt = "", HashString = ""});

            // Act
            await _userService.Register(userForCreate);

            // Assert
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void Register_InvalidUser_Fail()
        {
            //Arrange
            var userForCreate = new UserForCreate { Username = "testuser", Password = "password", Email = "test@example.com" };
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>()))
                .Returns(new[] { new User { Username = "testuser", Email = "test@example.com" } });

            //Act
            var ex = Assert.ThrowsAsync<AlreadyExistException>(async () => await _userService.Register(userForCreate));

            //Assert
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.UserAlreadyExist));
        }

        [Test]
        public void Login_ValidCredentials_Success()
        {
            
            //Arrange
            var userForLogin = new UserForLogin { Username = "user1", Password = "password" };

            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>()))
                .Returns(new[] { new User { Username = "user1", HashValue = "test", SaltValue = "salt1" } });
            
            _mockHashService.Setup(h => h.HashPassword(userForLogin.Password, It.IsAny<string>())).Returns("test");

            //Act
            var result = _userService.Login(userForLogin);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Login_InvalidUser_ThrowNotFoundException()
        {
            //Arrange
            var userForLogin = new UserForLogin { Username = "user1", Password = "password" };
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(
                It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>()))
                .Returns(Enumerable.Empty<User>());

            //Act
            var ex = Assert.Throws<NotFoundException>(() => _userService.Login(userForLogin));

            //Assert
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.UserNotFound));
        }

        [Test]
        public void Login_InvalidPassword_ThrowInvalidPasswordException()
        {
            //Arrange
            var userForLogin = new UserForLogin { Username = "user1", Password = "password123" };

            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>()))
                .Returns(new[] { new User { Username = "user1", HashValue = "test", SaltValue = "salt1" } });
            _mockHashService.Setup(h => h.HashPassword(It.IsAny<string>(), It.IsAny<string>())).Returns("testHahahaha");
            //Act
            var ex = Assert.Throws<Exception>(() => _userService.Login(userForLogin));

            //Assert
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.InvalidPassword));
        }

        [Test]
        public void ForgotPassword_InvalidUser_ThrowNotFoundException()
        {
            //Arrange
            var userForFotgotPass = new UserForForgotPassword { Email = "test@gmail.com" };
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(
                It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>()))
                .Returns(Enumerable.Empty<User>());

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _userService.ForgotPassword(userForFotgotPass));

            //Assert
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.UserNotFound));
        }

        [Test]
        public async Task ForgotPassword_ValidUser_Success()
        {
            //Arrange
            var userForFotgotPass = new UserForForgotPassword { Email = "test@gmail.com" };
            var userInDb = new User { Username = "user1", HashValue = "test", SaltValue = "salt1" };
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(
                It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>()))
                .Returns(new[] { userInDb });

            _mockHashService.Setup(h => h.GenerateOTP()).Returns("123456");

            _mockEmailService.Setup(e => e.SendEmailWith3rdEmailJs(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

            //Act
            await _userService.ForgotPassword(userForFotgotPass);

            //Assert
            // Assert that the user repository's update method was called with the correct user object
            _mockUnitOfWork.Verify(u => u.UserRepository.Update(It.IsAny<User>()), Times.Once);

            // Assert that SaveChangesAsync was called on the unit of work
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

            // Assert that SendEmailWith3rdEmailJs was called with the correct parameters
            _mockEmailService.Verify(e => e.SendEmailWith3rdEmailJs(userInDb.Email, userInDb.Username, "Your OTP is: 123456"), Times.Once);
        }

        [Test]
        public async Task ResetPassword_ValidInput_ShouldUpdateUserPasswordAndClearOTP()
        {
            // Arrange
            var userForResetPassword = new UserForResetPassword
            {
                Email = "test@example.com",
                Otp = "123456", // Assuming this is the correct OTP
                NewPassword = "newpassword"
            };

            var userInDb = new User
            {
                Email = "test@example.com",
                Otp = "123456" // Matching OTP
            };

            var hashResult = new PasswordHashResult
            {
                Salt = "salt",
                HashString = "hashedPassword"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), true))
                .Returns(new List<User> { userInDb });

            _mockHashService.Setup(h => h.HashPassword(userForResetPassword.NewPassword))
                .Returns(hashResult);

            // Act
            await _userService.ResetPassword(userForResetPassword);

            // Assert
            Assert.That(userInDb.SaltValue, Is.EqualTo("salt"));
            Assert.That(userInDb.HashValue, Is.EqualTo("hashedPassword"));
            Assert.IsNull(userInDb.Otp);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void ResetPassword_UserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var userForResetPassword = new UserForResetPassword
            {
                Email = "nonexistent@example.com",
                Otp = "123456",
                NewPassword = "newpassword"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), true))
                .Returns(new List<User>()); // Empty list means user not found

            // Act and Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _userService.ResetPassword(userForResetPassword));
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.UserNotFound));
        }

        [Test]
        public void ResetPassword_IncorrectOTP_ShouldThrowException()
        {
            // Arrange
            var userForResetPassword = new UserForResetPassword
            {
                Email = "test@example.com",
                Otp = "wrongOTP", // Incorrect OTP
                NewPassword = "newpassword"
            };

            var userInDb = new User
            {
                Email = "test@example.com",
                Otp = "123456" // Correct OTP
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), true))
                .Returns(new List<User> { userInDb });

            // Act and Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _userService.ResetPassword(userForResetPassword));
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.InvalidOtp));
        }

        [Test]
        public void ResetPassword_UpdateUserError_ShouldThrowException()
        {
            // Arrange
            var userForResetPassword = new UserForResetPassword
            {
                Email = "test@example.com",
                Otp = "123456", // Assuming this is the correct OTP
                NewPassword = "newpassword"
            };

            var userInDb = new User
            {
                Email = "test@example.com",
                Otp = "123456" // Matching OTP
            };

            var hashResult = new PasswordHashResult
            {
                Salt = "salt",
                HashString = "hashedPassword"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), true))
                .Returns(new List<User> { userInDb });

            _mockHashService.Setup(h => h.HashPassword(userForResetPassword.NewPassword))
                .Returns(hashResult);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("Update error"));

            // Act and Assert
            var exception = Assert.ThrowsAsync<Exception>(() => _userService.ResetPassword(userForResetPassword));
            Assert.That(exception.Message, Is.EqualTo("Update error"));
        }

        [Test]
        public void GetEmailById_UserFound_ShouldReturnEmail()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), true))
                .Returns(new List<User> { user });

            // Act
            var result = _userService.GetEmailById(1);

            // Assert
            Assert.That(result, Is.EqualTo("test@example.com"));
        }

        [Test]
        public void GetEmailById_UserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), true))
                .Returns(new List<User>());

            // Act and Assert
            var ex = Assert.Throws<NotFoundException>(() => _userService.GetEmailById(1));
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.UserNotFound));
        }

        [Test]
        public void GetCurrentUser_UserFound_ShouldReturnUserForRead()
        {
            // Arrange
            var username = "testuser";
            var user = new User
            {
                Id = 1,
                Username = username
            };

            _mockCurrentUserService.Setup(c => c.GetCurrentUser()).Returns(username);
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), true))
                .Returns(new List<User> { user });

            // Act
            var result = _userService.GetCurrentUser();

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Username, Is.EqualTo(username));
        }

        [Test]
        public void GetCurrentUser_UserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var username = "testuser";

            _mockCurrentUserService.Setup(c => c.GetCurrentUser()).Returns(username);
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), true))
                .Returns(new List<User>());

            // Act and Assert
            var ex = Assert.Throws<NotFoundException>(() => _userService.GetCurrentUser());
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.UserNotFound));
        }

        [Test]
        public void GetUser_ValidUsername_ReturnUserForRead()
        {
            // Arrange
            var username = "testuser";
            var expectedUserForRead = new UserForRead { Username = "testuser", Email = "test@example.com" };
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>()))
                .Returns(new[] { new User { Username = "testuser", Email = "test@example.com" } });

            // Act
            var result = _userService.GetUser(username);

            // Assert
            Assert.That(result.Username, Is.EqualTo(expectedUserForRead.Username));
            Assert.That(result.Email, Is.EqualTo(expectedUserForRead.Email));
        }

        [Test]
        public void GetUser_InvalidUsername_ThrowException()
        {
            // Arrange
            var username = "testuser";

            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(It.IsAny<Expression<Func<User, bool>>>(), true))
                .Returns(new List<User>());

            // Act and Assert
            var ex = Assert.Throws<NotFoundException>(() => _userService.GetUser(username));
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.UserNotFound));
        }

        [Test]
        public async Task GetUserById_UserFound_ShouldReturnUserForRead()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                Id = userId,
                Username = "testuser"
            };

            _mockUnitOfWork.Setup(u => u.UserRepository.GetById(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserById(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(userId));
        }

        [Test]
        public void GetUserById_UserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var userId = 1;

            _mockUnitOfWork.Setup(u => u.UserRepository.GetById(userId)).ReturnsAsync((User)null);

            // Act and Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _userService.GetUserById(userId));
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.UserNotFound));
        }

        [Test]
        public async Task UpdateCurrentUser_UsernameAndPasswordProvided_ShouldUpdateUser()
        {
            // Arrange
            var username = "testuser";
            var password = "newpassword";
            var userForUpdate = new UserForUpdate
            {
                Username = username,
                Password = password
            };

            var userInDb = new User
            {
                Username = username,
                SaltValue = "oldsalt",
                HashValue = "oldhash"
            };

            _mockCurrentUserService.Setup(u => u.GetCurrentUser()).Returns(username);
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(
                It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>())).Returns(new[] { userInDb });

            _mockHashService.Setup(h => h.HashPassword(password)).Returns(new PasswordHashResult { Salt = "newsalt", HashString = "newhash" });

            // Act
            await _userService.UpdateCurrentUser(userForUpdate);

            // Assert
            Assert.That(userInDb.Username, Is.EqualTo(username));
            Assert.That(userInDb.SaltValue, Is.EqualTo("newsalt"));
            Assert.That(userInDb.HashValue, Is.EqualTo("newhash"));
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateCurrentUser_UsernameOnlyProvided_ShouldUpdateUsername()
        {
            // Arrange
            var username = "testuser";
            var userForUpdate = new UserForUpdate
            {
                Username = username
            };

            var userInDb = new User
            {
                Username = "oldusername"
            };

            _mockCurrentUserService.Setup(u => u.GetCurrentUser()).Returns(username);
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(
                It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>())).Returns(new[] { userInDb });

            // Act
            await _userService.UpdateCurrentUser(userForUpdate);

            // Assert
            Assert.That(userInDb.Username, Is.EqualTo(username));
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateCurrentUser_PasswordOnlyProvided_ShouldUpdatePassword()
        {
            // Arrange
            var username = "testuser";
            var password = "newpassword";
            var userForUpdate = new UserForUpdate
            {
                Password = password
            };

            var userInDb = new User
            {
                Username = username,
                SaltValue = "oldsalt",
                HashValue = "oldhash"
            };

            _mockCurrentUserService.Setup(u => u.GetCurrentUser()).Returns(username);
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(
                It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>())).Returns(new[] { userInDb });

            _mockHashService.Setup(h => h.HashPassword(password)).Returns(new PasswordHashResult { Salt = "newsalt", HashString = "newhash" });

            // Act
            await _userService.UpdateCurrentUser(userForUpdate);

            // Assert
            Assert.That(userInDb.SaltValue, Is.EqualTo("newsalt"));
            Assert.That(userInDb.HashValue, Is.EqualTo("newhash"));
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateCurrentUser_EmailOnlyProvided_ShouldUpdateEmail()
        {
            // Arrange
            var username = "testuser";
            var email = "newemail@example.com";
            var userForUpdate = new UserForUpdate
            {
                Email = email
            };

            var userInDb = new User
            {
                Username = username,
                Email = "oldemail@example.com"
            };

            _mockCurrentUserService.Setup(u => u.GetCurrentUser()).Returns(username);
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(
                It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>())).Returns(new[] { userInDb });

            // Act
            await _userService.UpdateCurrentUser(userForUpdate);

            // Assert
            Assert.AreEqual(email, userInDb.Email);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void UpdateCurrentUser_UserNotFound_ShouldThrowException()
        {
            // Arrange
            var username = "testuser";
            var userForUpdate = new UserForUpdate
            {
                Username = username
            };
            _mockUnitOfWork.Setup(u => u.UserRepository.FindByCondition(
                It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<bool>())).Returns(new List<User>());

            // Act and Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _userService.UpdateCurrentUser(userForUpdate));
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.UserNotFound));
        }

        [Test]
        public async Task DeleteUser_ExistingId_Success()
        {
            // Arrange
            var userId = 1;
            _mockUnitOfWork.Setup(u => u.UserRepository.GetById(userId)).ReturnsAsync(new User());

            // Act
            await _userService.DeleteUser(userId);

            // Assert
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteUser_InvalidId_ThrowException()
        {
            // Arrange
            var userId = 1;
            _mockUnitOfWork.Setup(u => u.UserRepository.GetById(userId)).ReturnsAsync((User)null);

            // Act and Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _userService.DeleteUser(userId));
            Assert.That(ex.Message, Is.EqualTo(ErrorMessage.UserNotFound));
        }
    }
}
