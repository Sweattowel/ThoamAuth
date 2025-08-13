using Microsoft.AspNetCore.Authorization;

namespace ThoamAuth.ServerPoliciesAndSettings.Roles;

public class RoleClass
{
    public class NotRoleRequirement(params string[] roles) : IAuthorizationRequirement
    {
        public string[] Roles { get; } = roles;
    }
    
    public class NotRoleHandler : AuthorizationHandler<NotRoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NotRoleRequirement requirement)
        {
            foreach (var role in requirement.Roles)
            {
                if (context.User.IsInRole(role))
                {
                    AuthorizationFailureReason reason = new(this, "Incorrect Roles");
                    
                    context.Fail(reason);

                    return Task.CompletedTask;
                }
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}