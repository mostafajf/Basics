using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
namespace Basics.Bussiness
{
    public class MinimumAgeRequirement : IAuthorizationRequirement
    {
        public int Age { get; set; }
        public MinimumAgeRequirement(int age)
        {
            Age = age;
        }
    }
    //Identity Based
    public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
        {
            var user = context.User;
            if (!user.HasClaim(x => x.Type == ClaimTypes.DateOfBirth))
            {
                return Task.CompletedTask;
            }
            var dateOfBirth = DateTime.Parse(user.FindFirst(ClaimTypes.DateOfBirth).Value);
            var span = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth > DateTime.Today.AddYears(-span))
            {
                span--;
            }
            if (span > requirement.Age)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}

