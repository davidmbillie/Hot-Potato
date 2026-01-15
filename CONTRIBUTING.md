# Contributing

We invite anyone interested in API conformance testing in the open-source community to contribute to our repositories. We ask that any change be discussed via a [GitHub Issue](https://github.com/davidmbillie/Hot-Potato/issues). When preparing a contribution to our repository, please consider the following contribution guidelines.

## Code of Conduct

* Using welcoming and inclusive language
* Being respectful of differing viewpoints and experiences
* Gracefully accepting constructive criticism
* Focusing on what is best for the community
* Showing empathy towards other community members


## Testing

We expect all pull requests to have some amount of automated testing. Moreso, this repository uses a trunk based branching model, so it is imperative that all changes be appropriately and completely tested before merging into master. Any pull request without automated tests may be declined immediately.

All of our test projects use the [XUnit](https://xunit.net/) test framework, [Moq](https://github.com/moq/moq) for mocking, [WireMock.Net](http://wiremock.org/) for HTTP server mocking in our system tests.

### Unit Tests

Our unit tests reside in projects that match the system under test with a `*.Test` suffix. So for `HotPotato.Core.csproj` the unit test project is `HotPotato.Core.Test.csproj`. When writing unit tests, try to isolate the system under test as best as possible, preferrably using mocks. Our team can provide assistance where needed.

### Integration Tests

Our integration test project tends to test more complicated integrated scenarios such as our validation logic using our proxy processor logic. If tests become too complicated or need multiple dependencies, place them in the `HotPotato.Integration.Test` project.

### System Tests

Our system tests are meant to act as a form of documentation for the multiple ways the proxy can be used to validate conformance to an API spec. They consist of a made-up API with a specification, a set of tests, and the HotPotato proxy set up as a reverse proxy. There are 3 suites that test valid tests, missing specification components, and invalid specification components which exist in `HotPotato.System.HappyPath.Test.csproj`, `HotPotato.System.NotInSpec.Test.csproj`, and `HotPotato.System.NonConformant.Test.csproj` respectively.

### Local Tests

To run tests locally against an external remote endpoint, you will need to provide the API's RemoteEndpoint and SpecLocation to `src\HotPotato.AspNetCore.Host\appsettings.json`. Like the example below:

```diff
{
  "RemoteEndpoint": "https://indikatorer-api.naturvardsverket.se/",
  "HttpClientSettings": {
    "IgnoreClientHttpsCertificateValidationErrors": "false"
  },
  "SpecLocation": "https://raw.githubusercontent.com/greentechdev/greentechdev.github.io/master/environmental_indicators_api.yaml",
  "ForwardProxy": {
    "Enabled": "false",
    "ProxyAddress": "http://localhost:8888",
    "BypassOnLocal": "false"
  },
  "exclude": [
    "**/bin",
    "**/bower_components",
    "**/jspm_packages",
    "**/node_modules",
    "**/obj",
    "**/platforms"
  ],
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  }
}
```

Once the API that needs to be tested is configured, you'll want to append the endpoints (e.g localhost:3232/omrade/api/v1/indicators/) through something like Postman.

Noted: As with any sort of Hot Potato testing, you'll be replacing the host name with localhost:3232.

To run these tests locally through a Docker container, you can do a Docker build from within the root folder of your Hot Potato solution, while passing passing in an arbitrary number for IMAGE_VERSION, like so:

```sh
docker build --tag hot-potato:4.8 --build-arg IMAGE_VERSION=4.8 .
```

Now to test, you may pass the API's REMOTE_ENDPOINT and SPEC_LOCATION through the command line. Connecting Docker with your localhost requires some nontrivial network setup, so we recommend using an external API, like so:

```sh
docker network create hp

docker run --rm -d --network hp --name Conformance -p 3232:3232 -e HttpClientSettings__IgnoreClientHttpsCertificateValidationErrors=true -e REMOTE_ENDPOINT=https://indikatorer-api.naturvardsverket.se/ -e SPEC_LOCATION=https://raw.githubusercontent.com/greentechdev/greentechdev.github.io/master/environmental_indicators_api.yaml hot-potato:4.8
```

## Pull requests

When coding and testing is complete, a pull request (PR) can be created to merge. We will try to respond to the request within three business days.

* Make sure the entire Test Automation team is added to the PR (this should be done by default when creating the PR).
* Request will be declined immediately if there are no tests for added code
* Make sure there are no merge conflicts
* Make sure all tests are passing
* Make sure build is passing
* Update readme/documentation if necessary

## Style guide

* Try to follow [SOLID](https://en.wikipedia.org/wiki/SOLID) principles 
* Try to limit external dependencies when possible
* External dependencies should be isolated in a way that test doubles can be used 

