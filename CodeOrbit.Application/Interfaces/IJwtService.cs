using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Domain.Entities;

namespace CodeOrbit.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
