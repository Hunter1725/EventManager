using Domain.DTOs.UserDTO;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services
{
    public interface IUserService
    {
        Task Register(UserForCreate userForCreate);
        bool Login(UserForLogin userForLogin);
        string GetEmailById(int id);
        UserForRead GetCurrentUser();
        UserForRead GetUser(string username);
        Task<UserForRead> GetUserById(int id);
        Task UpdateCurrentUser(UserForUpdate userForUpdate);
        Task DeleteUser(int id);
        Task ResetPassword(UserForResetPassword userForResetPassword);
        Task ForgotPassword(UserForForgotPassword userForForgotPassword);
    }
}
