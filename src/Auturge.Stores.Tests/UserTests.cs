using Auturge.Identifiers;
using Auturge.Stores.Tests.TestObjects;

namespace Auturge.Stores.Tests;

[TestFixture]
public class UserTests
{
    [Test]
    public void SYSTEM_Should_HaveNoCreator()
    {
        Assert.That(User.SYSTEM.CreatedBy, Is.Null);
        Assert.That(User.SYSTEM.LastUpdatedBy, Is.Null);
    }

    [Test]
    public void ADMIN_Should_BeCreatedBySystem()
    {
        Assert.That(User.ADMIN.CreatedBy, Is.SameAs(User.SYSTEM));
    }

    [Test]
    public void SystemPrincipals_Should_ReportAsSystemObjects()
    {
        Assert.That(User.SYSTEM.IsSystemObject, Is.True);
        Assert.That(User.ADMIN.IsSystemObject, Is.True);
    }

    [Test]
    public void RegularUser_Should_DefaultToAdminCreator_And_NotBeASystemObject()
    {
        var user = new User("regular");

        Assert.That(user.CreatedBy, Is.SameAs(User.ADMIN));
        Assert.That(user.IsSystemObject, Is.False);
    }

    [Test]
    public void RegularUser_Should_AcceptAnExplicitCreator()
    {
        var manager = new User(Flake.NewFlake(), "manager");

        var report = new User(Flake.NewFlake(), "report", manager);

        Assert.That(report.CreatedBy, Is.SameAs(manager));
    }
}
