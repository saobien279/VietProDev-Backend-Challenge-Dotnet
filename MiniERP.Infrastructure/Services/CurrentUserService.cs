using MiniERP.Application.Interfaces.Services;
using System;

namespace MiniERP.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        // Hiện tại chưa có Auth, trả về null. Sau này có Auth sẽ lấy từ HttpContextAccessor.
        public Guid? UserId => null;
    }
}
