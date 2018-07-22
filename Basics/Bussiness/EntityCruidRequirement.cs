using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Basics.Models;
using Microsoft.EntityFrameworkCore;

namespace Basics.Bussiness
{
    public class CruidOperations
    {
        public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement { Name = nameof(Create) };
        public static OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement { Name = nameof(Read) };
        public static OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement { Name = nameof(Update) };
        public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement { Name = nameof(Delete) };
    }
    //resource Based
    public class EntityCruidHandler : AuthorizationHandler<OperationAuthorizationRequirement, EntityBase>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, EntityBase resource)
        {
            if (requirement == CruidOperations.Create)
            {
                //if (resource is cocereteType)
                //{

                //}
                if (context.User.IsInRole("Administartor"))
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement == CruidOperations.Read)
            {
                if (context.User.Identity.IsAuthenticated)
                {
                    context.Succeed(requirement);
                }
            }
            // and so on 

            //or we can use acl records
            //if user.aclsrecords.where(s=>s.operation=requiremnet.name&&s.entityid=resource.id) return context.succesed
            return Task.CompletedTask;
        }
    }
    //identity Based
    public class PermisionRequirement : IAuthorizationRequirement
    {
        // we also could use OperationAuthorizationRequirement
        public string Permision { get; set; }
        public PermisionRequirement(string permision)
        {
            Permision = permision;
        }
    }
    public class PermisionRequirementHandler : AuthorizationHandler<PermisionRequirement>
    {
        UserManager<ApplicationUser> UserManager;
        RoleManager<IdentityRole> RoleManager;
        public PermisionRequirementHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermisionRequirement requirement)
        {
            ApplicationUser user = UserManager.FindByNameAsync(context.User.Identity.Name).GetAwaiter().GetResult();
            List<string> userRoles = UserManager.GetRolesAsync(user).GetAwaiter().GetResult().ToList();
            var claims = from role in RoleManager.Roles
                         where userRoles.Contains(role.Name)
                         select RoleManager.GetClaimsAsync(role).Result;
            if (context.User.FindFirst(requirement.Permision) != null || claims.Any(c => c.GetType().ToString() == requirement.Permision))
            {  
                //add permisionrecords as claim to user or user roles in admin area
                context.Succeed(requirement);
            }
            //if user.permisions(or user.role.permisions).where(s=>s.name=requiremnet.Permision) return context.succesed
            return Task.CompletedTask;
        }
    }
}
