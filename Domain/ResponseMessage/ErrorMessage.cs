using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ResponseMessage
{
    public static class ErrorMessage
    {
        public static string NotAuthenticated = "User is not authenticated";

        public static string SomethingWrong = "Something wrong, please try again!";

        public static string EventNotFound = "Event not found";

        public static string Unauthorized = "User is not unauthorized!";

        public static string NotOwner = "You are not owner of this event";

        public static string EventNotExists = "Event not found!";

        public static string UserAlreadyExist = "User already exist!";

        public static string UserNotFound = "User not found!";

        public static string InvalidPassword = "Invalid password!";

        public static string InvalidOtp = "Invalid OTP!";
    }
}
