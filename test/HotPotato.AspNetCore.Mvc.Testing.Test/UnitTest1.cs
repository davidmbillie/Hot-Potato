namespace HotPotato.AspNetCore.Mvc.Testing;

public class Tests
{
    private const string specLocation = "https://raw.githubusercontent.com/HylandSoftware/Hot-Potato/master/test/RawPotatoSpec.yaml";
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task Test1()
    {
        var builder = new HotPotatoWebApplicationBuilder<Test.Api.Startup>(specLocation);
        var client = builder.CreateClient();
        var response = await client.GetAsync("/orders");

    }
}
