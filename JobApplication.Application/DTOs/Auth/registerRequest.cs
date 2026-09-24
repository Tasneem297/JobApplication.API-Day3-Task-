using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobApplication.Application.DTOs.Auth;
public record registerRequest(
    string Email,
    string Password,
    string UserName,
    string FirstName,
    string LastName,
     string PhoneNumber,
     string Gender,
     string UserType
    );

