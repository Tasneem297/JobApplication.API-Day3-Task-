using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities.Identity;
using JobApplication.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobApplication.Infrastructure.Repositories;
public class AuthRepository:IAuthRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _UserManager;

    public AuthRepository(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _UserManager = userManager;
    }

    public async Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserRolesAndPermissions(ApplicationUser user, CancellationToken cancellationToken)
    {
        var userRoles = await _UserManager.GetRolesAsync(user);

        var userPermissions = await (from r in _context.Roles
                                     join p in _context.RoleClaims
                                     on r.Id equals p.RoleId
                                     where userRoles.Contains(r.Name!)
                                     select p.ClaimValue!)
                                     .Distinct()
                                     .ToListAsync(cancellationToken);

        return (userRoles, userPermissions);
    }
}
