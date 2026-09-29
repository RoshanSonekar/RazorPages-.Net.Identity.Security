using Microsoft.AspNetCore.Authorization;

namespace WebAppRazor_Security.Authorisation
{
		public class HRManagerProbationRequirement : IAuthorizationRequirement
		{
			public int ProbationMonths { get; }
			public HRManagerProbationRequirement(int probationInMonths)
			{
				ProbationMonths = probationInMonths;
			}
		}

		public class HRManagerProbationRequirementHandler : AuthorizationHandler<HRManagerProbationRequirement>
		{
			protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HRManagerProbationRequirement requirement)
			{
				if (!context.User.HasClaim(x => x.Type == "EmploymentDate"))
					return Task.CompletedTask;

				if (DateTime.TryParse(context.User.FindFirst(x => x.Type == "EmploymentDate")?.Value, out DateTime employmentDate))
				{
					var period = DateTime.Now - employmentDate;
					if (period.Days > 30 * requirement.ProbationMonths)
						context.Succeed(requirement);
				}

				return Task.CompletedTask;
			}
		}
	}