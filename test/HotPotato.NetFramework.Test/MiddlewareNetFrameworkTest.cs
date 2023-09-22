//using HotPotato.Core.Proxy;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;

//namespace HotPotato.AspNetCore.Middleware
//{
//	internal class MiddlewareNetFrameworkTest
//	{
//		private const string RemoteEndpointKey = "RemoteEndpoint";
//		private const string AValidEndpoint = "http://foo";
//		private const string SpecLocationKey = "SpecLocation";
//		private const string AValidSpecLocation = "https://yukon.gold.potato.com/projects/TATO/repos/foo/raw/test/bar.yaml";

//		[Test]
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
//	}
//}
