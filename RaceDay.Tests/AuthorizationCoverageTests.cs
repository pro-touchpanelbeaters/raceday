using Microsoft.AspNetCore.Authorization;
using RaceDay.Api.Controllers;

namespace RaceDay.Tests;

public class AuthorizationCoverageTests
{
    [Fact]
    public void Users_Controller_Requires_Authentication()
    {
        Assert.NotNull(typeof(UsersController).GetCustomAttributes(typeof(AuthorizeAttribute), true).SingleOrDefault());
    }

    [Fact]
    public void Organiser_Endpoints_Are_Role_Protected()
    {
        AssertRole<EventsController>(nameof(EventsController.Create), "Organiser");
        AssertRole<EventsController>(nameof(EventsController.Update), "Organiser");
        AssertRole<EventsController>(nameof(EventsController.Delete), "Organiser");
        AssertRole<CategoriesController>(nameof(CategoriesController.Create), "Organiser");
        AssertRole<CategoriesController>(nameof(CategoriesController.Update), "Organiser");
        AssertRole<CategoriesController>(nameof(CategoriesController.Delete), "Organiser");
        AssertRole<ResultsController>(nameof(ResultsController.Create), "Organiser");
        AssertRole<ResultsController>(nameof(ResultsController.Update), "Organiser");
    }

    [Fact]
    public void Participant_Endpoints_Are_Role_Protected()
    {
        AssertRole<EnrolmentsController>(nameof(EnrolmentsController.Create), "Participant");
        AssertRole<EnrolmentsController>(nameof(EnrolmentsController.Mine), "Participant");
        AssertRole<EnrolmentsController>(nameof(EnrolmentsController.Cancel), "Participant");
        AssertRole<ResultsController>(nameof(ResultsController.Mine), "Participant");
    }

    private static void AssertRole<T>(string methodName, string expectedRole)
    {
        var method = typeof(T).GetMethod(methodName)!;
        var attribute = method.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>().Single();
        Assert.Equal(expectedRole, attribute.Roles);
    }
}
