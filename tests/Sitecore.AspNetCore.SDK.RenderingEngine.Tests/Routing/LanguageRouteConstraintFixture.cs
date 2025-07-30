using Shouldly;
using Microsoft.AspNetCore.Routing;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.RenderingEngine.Routing;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Tests.Routing;

public class LanguageRouteConstraintFixture
{
    [Theory]
    [AutoNSubstituteData]
    public void Match_WhenCalled_ShouldHandleNoCulture(LanguageRouteConstraint stu)
    {
        bool match = stu.Match(null, null, "path", [], RouteDirection.IncomingRequest);

        match.ShouldBeFalse();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Match_WhenCalled_ShouldDetectCulture(LanguageRouteConstraint stu)
    {
        RouteValueDictionary values = new()
        {
            { "culture", "da" }
        };

        bool match = stu.Match(null, null, "path", values, RouteDirection.IncomingRequest);

        match.ShouldBeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Match_WhenCalled_ShouldDetectWrongCulture(LanguageRouteConstraint stu)
    {
        RouteValueDictionary values = new()
        {
            { "culture", "css" }
        };

        bool match = stu.Match(null, null, "path", values, RouteDirection.IncomingRequest);

        match.ShouldBeFalse();
    }
}
