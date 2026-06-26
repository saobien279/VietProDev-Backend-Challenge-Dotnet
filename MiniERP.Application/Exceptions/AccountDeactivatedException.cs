using System;

namespace MiniERP.Application.Exceptions
{
    public class AccountDeactivatedException : Exception
    {
        public AccountDeactivatedException(string message) : base(message)
        {
        }
    }
}
