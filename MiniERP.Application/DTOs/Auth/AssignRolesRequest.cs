using System.Collections.Generic;

namespace MiniERP.Application.DTOs.Auth
{
    public class AssignRolesRequest
    {
        public List<string> Roles { get; set; } = new();
    }
}
