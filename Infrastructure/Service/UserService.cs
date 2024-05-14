using Contract.Services;
using Domain.DTOs.UserDTO;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces.Services;
using Domain.ResponseMessage;
using Infrastructure.Extensions;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Infrastructure.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;
        private readonly IHashService _hashService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            ILogger<UserService> logger,
            IHashService hashService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _emailService = emailService;
            _logger = logger;
            _hashService = hashService;
        }

        public async Task Register(UserForCreate userForCreate)
        {
            var userInDb = _unitOfWork.UserRepository
                .FindByCondition(u => u.Username == userForCreate.Username, true);

            if (userInDb.Count() != 0)
            {
                _logger.LogError("User with {0} already exists!", userForCreate.Username);
                throw new AlreadyExistException(ErrorMessage.UserAlreadyExist);
            }

            var hashResult = _hashService.HashPassword(userForCreate.Password);

            User newUser = new User
            {
                Username = userForCreate.Username,
                SaltValue = hashResult.Salt,
                HashValue = hashResult.HashString,
                Email = userForCreate.Email,
                Role = "User"
            };

            _unitOfWork.UserRepository.Add(newUser);
            await _unitOfWork.SaveChangesAsync();
        }

        public bool Login(UserForLogin userForLogin)
        {
            var userInDb = _unitOfWork.UserRepository
                .FindByCondition(u => u.Username == userForLogin.Username, true)
                .FirstOrDefault();

            if (userInDb == null)
            {
                _logger.LogError("User with {0} not found!", userForLogin.Username);
                throw new NotFoundException(ErrorMessage.UserNotFound);
            }

            var hashValueString = _hashService.HashPassword(userForLogin.Password, userInDb.SaltValue);

            if (hashValueString != userInDb.HashValue)
            {
                _logger.LogError("Password incorrect!");
                throw new Exception(ErrorMessage.InvalidPassword);
            }
            return true;
        }

        public async Task ForgotPassword(UserForForgotPassword userForForgotPassword)
        {
            var userInDb = _unitOfWork.UserRepository
                .FindByCondition(u => u.Email == userForForgotPassword.Email, true)
                .FirstOrDefault();

            if (userInDb == null)
            {
                _logger.LogError("User with {0} not found!", userForForgotPassword.Email);
                throw new NotFoundException(ErrorMessage.UserNotFound);
            }

            string OTP = _hashService.GenerateOTP();

            userInDb.Otp = OTP;
            _unitOfWork.UserRepository.Update(userInDb);
            await _unitOfWork.SaveChangesAsync();

            string emailBody = $"Your OTP is: {OTP}";

            await _emailService.SendEmailWith3rdEmailJs(userInDb.Email, userInDb.Username, emailBody);
        }

        public async Task ResetPassword(UserForResetPassword userForResetPassword)
        {
            var userInDb = _unitOfWork.UserRepository
                .FindByCondition(u => u.Email == userForResetPassword.Email, true)
                .FirstOrDefault();

            if (userInDb == null)
            {
                _logger.LogError("User with {0} not found!", userForResetPassword.Email);
                throw new NotFoundException(ErrorMessage.UserNotFound);
            }

            if (userForResetPassword.Otp != userInDb.Otp)
            {
                _logger.LogError("OTP incorrect!");
                throw new Exception(ErrorMessage.InvalidOtp);
            }

            var hashResult = _hashService.HashPassword(userForResetPassword.NewPassword);

            userInDb.SaltValue = hashResult.Salt;
            userInDb.HashValue = hashResult.HashString;
            userInDb.Otp = null;

            _unitOfWork.UserRepository.Update(userInDb);
            await _unitOfWork.SaveChangesAsync();
        }

        public string GetEmailById(int id)
        {
            User user = _unitOfWork.UserRepository
                .FindByCondition(u => u.Id == id, true)
                .FirstOrDefault();

            if (user == null)
            {
                throw new NotFoundException(ErrorMessage.UserNotFound);
            }

            return user.Email;
        }
        public UserForRead GetCurrentUser()
        {

            string username = _currentUserService.GetCurrentUser();

            var userInDb = _unitOfWork.UserRepository
                .FindByCondition(u => u.Username == username, true)
                .FirstOrDefault();

            if (userInDb == null)
            {
                throw new NotFoundException(ErrorMessage.UserNotFound);
            }

            return userInDb.ResponseToRead();
        }

        public UserForRead GetUser(string username)
        {
            var user = _unitOfWork.UserRepository
                .FindByCondition(u => u.Username == username, true)
                .FirstOrDefault();

            if (user == null)
            {
                throw new NotFoundException(ErrorMessage.UserNotFound);
            }

            return user.ResponseToRead();
        }

        public async Task<UserForRead> GetUserById(int id)
        {
            User user = await _unitOfWork.UserRepository.GetById(id);

            if (user == null)
            {
                throw new NotFoundException(ErrorMessage.UserNotFound);
            }

            return user.ResponseToRead();
        }

        //Update current user
        public async Task UpdateCurrentUser(UserForUpdate userForUpdate)
        {

            string username = _currentUserService.GetCurrentUser();

            var userInDb = _unitOfWork.UserRepository
                .FindByCondition(u => u.Username == username, true)
                .FirstOrDefault();

            if (userInDb == null)
            {
                throw new Exception(ErrorMessage.UserNotFound);
            }

            if (userForUpdate.Username != null)
            {
                userInDb.Username = userForUpdate.Username;
            }

            if (userForUpdate.Password != null)
            {
                var hashResult = _hashService.HashPassword(userForUpdate.Password);

                userInDb.SaltValue = hashResult.Salt;
                userInDb.HashValue = hashResult.HashString;
            }

            if (userForUpdate.Email != null)
            {
                userInDb.Email = userForUpdate.Email;
            }

            _unitOfWork.UserRepository.Update(userInDb);
            await _unitOfWork.SaveChangesAsync();
        }

        //Delete user
        public async Task DeleteUser(int id)
        {
            User userInDb = await _unitOfWork.UserRepository.GetById(id);
            if (userInDb == null)
            {
                throw new NotFoundException(ErrorMessage.UserNotFound);
            }
            _unitOfWork.UserRepository.Delete(userInDb);
            await _unitOfWork.SaveChangesAsync();
        }   
    }
}