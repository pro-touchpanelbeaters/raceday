using Microsoft.AspNetCore.Authorization;
using RaceDay.Api.Controllers;
using RaceDay.Api.Services;
using Xunit;

namespace RaceDay.Tests;

public class ControllerAuthorizationTests
{
    [Fact]
    public void Event_Create_Requires_Organiser_Role()
    {
        var method = typeof(EventsController).GetMethod(nameof(EventsController.Create));
        var attribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>().Single();
        Assert.Equal("Organiser", attribute.Roles);
    }

    [Fact]
    public void Enrolment_Create_Requires_Participant_Role()
    {
        var method = typeof(EnrolmentsController).GetMethod(nameof(EnrolmentsController.Create));
        var attribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>().Single();
        Assert.Equal("Participant", attribute.Roles);
    }

    [Fact]
    public void Result_Create_Requires_Organiser_Role()
    {
        var method = typeof(ResultsController).GetMethod(nameof(ResultsController.Create));
        var attribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>().Single();
        Assert.Equal("Organiser", attribute.Roles);
    }
}
