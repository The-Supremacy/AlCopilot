using AlCopilot.CustomerProfile.Contracts.DTOs;
using Mediator;

namespace AlCopilot.CustomerProfile.Contracts.Queries;

public sealed record GetCustomerProfileQuery() : IRequest<CustomerProfileDto>;
