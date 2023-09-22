//using HotPotato.Core.Proxy;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Moq;
//using System;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Xunit;

//namespace HotPotato.AspNetCore.Middleware
//{
//	public class HotPotatoMiddlewareTest
//	{
//		private const string RemoteEndpointKey = "RemoteEndpoint";
//		private const string AValidEndpoint = "http://foo";
//		private const string SpecLocationKey = "SpecLocation";
//		private const string AValidSpecLocation = "https://yukon.gold.potato.com/projects/TATO/repos/foo/raw/test/bar.yaml";

//		[Fact]
//		public void Constructor_NullProxy_Throws()
//		{
//			Assert.Throws<ArgumentNullException>("proxy", () => new HotPotatoMiddleware(null, null, Mock.Of<IConfiguration>(), Mock.Of<ILogger<HotPotatoMiddleware>>()));
//		}

//		[Fact]
//		public void Constructor_NullConfiguration_Throws()
//		{
//			Assert.Throws<ArgumentNullException>("configuration", () => new HotPotatoMiddleware(null, Mock.Of<IProxy>(), null, Mock.Of<ILogger<HotPotatoMiddleware>>()));
//		}

//		[Fact]
//		public void Constructor_NullLogger_Throws()
//		{
//			Assert.Throws<ArgumentNullException>("log", () => new HotPotatoMiddleware(null, Mock.Of<IProxy>(), Mock.Of<IConfiguration>(), null));
//		}

//		[Fact]
//		public void Constructor_MissingRemoteEndpointKey_Throws()
//		{
//			Assert.Throws<InvalidOperationException>(() => new HotPotatoMiddleware(null, Mock.Of<IProxy>(), Mock.Of<IConfiguration>(), Mock.Of<ILogger<HotPotatoMiddleware>>()));
//		}

//		[Fact]
//		public void Constructor_MissingSpecLocationKey_Throws()
//		{
//			Mock<IConfiguration> configMock = new Mock<IConfiguration>();
//			configMock.SetupGet(x => x[RemoteEndpointKey]).Returns(AValidEndpoint);
//			Assert.Throws<InvalidOperationException>(() => new HotPotatoMiddleware(null, Mock.Of<IProxy>(), configMock.Object, Mock.Of<ILogger<HotPotatoMiddleware>>()));
//		}

//		[Fact]
//		public async Task Invoke_CallsProxy()
//		{
//			Mock<IProxy> proxyMock = new Mock<IProxy>();
//			Mock<IConfiguration> configMock = new Mock<IConfiguration>();
//			configMock.SetupGet(x => x[RemoteEndpointKey]).Returns(AValidEndpoint);
//			configMock.SetupGet(x => x[SpecLocationKey]).Returns(AValidSpecLocation);
//			Mock<HttpContext> contextMock = new Mock<HttpContext>();
//			var response = Mock.Of<HttpResponse>();
//			var request = Mock.Of<HttpRequest>();
//			contextMock.SetupGet(x => x.Request).Returns(request);
//			contextMock.SetupGet(x => x.Response).Returns(response);

//			HotPotatoMiddleware subject = new HotPotatoMiddleware(
//				null, 
//				proxyMock.Object, 
//				configMock.Object, 
//				Mock.Of<ILogger<HotPotatoMiddleware>>());

//			await subject.Invoke(contextMock.Object);

//			proxyMock.Verify(x => x.ProcessAsync(AValidEndpoint, request, response));
//		}

//		[Fact]
//		public async Task Invoke_Throws_SetsInternalServerError()
//		{
//			Mock<IProxy> proxyMock = new Mock<IProxy>();
//			proxyMock.Setup(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<HttpRequest>(), It.IsAny<HttpResponse>()))
//				.Throws(new Exception("FAIL"));
//			Mock<IConfiguration> configMock = new Mock<IConfiguration>();
//			configMock.SetupGet(x => x[RemoteEndpointKey]).Returns(AValidEndpoint);
//			configMock.SetupGet(x => x[SpecLocationKey]).Returns(AValidSpecLocation);
//			Mock<ILogger<HotPotatoMiddleware>> loggerMock = new Mock<ILogger<HotPotatoMiddleware>>();
//			Mock<HttpContext> contextMock = new Mock<HttpContext>();
//			contextMock.SetupGet(x => x.Request).Returns(Mock.Of<HttpRequest>());
//			contextMock.SetupGet(x => x.Response).Returns(Mock.Of<HttpResponse>());

//			HotPotatoMiddleware subject = new HotPotatoMiddleware(
//				null,
//				proxyMock.Object,
//				configMock.Object,
//				loggerMock.Object);

//			await subject.Invoke(contextMock.Object);

//			Assert.Equal((int)HttpStatusCode.InternalServerError, contextMock.Object.Response.StatusCode);
//		}

//		[Fact]
//		public async Task Invoke_ThrowsHttpRequestException_SetsBadGateway()
//		{
//			Mock<IProxy> proxyMock = new Mock<IProxy>();
//			proxyMock.Setup(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<HttpRequest>(), It.IsAny<HttpResponse>()))
//				.Throws(new HttpRequestException("FAIL"));
//			Mock<IConfiguration> configMock = new Mock<IConfiguration>();
//			configMock.SetupGet(x => x[RemoteEndpointKey]).Returns(AValidEndpoint);
//			configMock.SetupGet(x => x[SpecLocationKey]).Returns(AValidSpecLocation);
//			Mock<ILogger<HotPotatoMiddleware>> loggerMock = new Mock<ILogger<HotPotatoMiddleware>>();
//			Mock<HttpContext> contextMock = new Mock<HttpContext>();
//			contextMock.SetupGet(x => x.Request).Returns(Mock.Of<HttpRequest>());
//			contextMock.SetupGet(x => x.Response).Returns(Mock.Of<HttpResponse>());

//			HotPotatoMiddleware subject = new HotPotatoMiddleware(
//				null,
//				proxyMock.Object,
//				configMock.Object,
//				loggerMock.Object);

//			await subject.Invoke(contextMock.Object);

//			Assert.Equal((int)HttpStatusCode.BadGateway, contextMock.Object.Response.StatusCode);
//		}
//	}
//}
