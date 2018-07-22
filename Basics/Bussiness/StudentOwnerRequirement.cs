using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Basics.Models;
using Microsoft.AspNetCore.Authorization;
namespace Basics.Bussiness
{
    //ResourceBased
    //we can generalizaie resource owner with add owner id to entitybase type
    public class StudentOwnerRequirement : IAuthorizationRequirement
    {
    }
    public class StudentOwnerHandler : AuthorizationHandler<StudentOwnerRequirement, Student>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, StudentOwnerRequirement requirement, Student resource)
        {
            if (context.User.FindFirst(c => c.Type == ClaimTypes.Name).Value.Contains(resource.Name))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
    public class EntityBase
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
    }
    public class EntityOwnerHandler : AuthorizationHandler<StudentOwnerRequirement, EntityBase>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, StudentOwnerRequirement requirement, EntityBase resource)
        {
            if (context.User.FindFirst(c => c.Type == ClaimTypes.Sid).Value == resource.OwnerId.ToString())
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
