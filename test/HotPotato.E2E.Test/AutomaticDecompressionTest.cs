using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using HotPotato.Core.Http;
using HotPotato.Core.Http.Default;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace HotPotato.E2E.Test
{
	[Collection("Host")]
	public class AutomaticDecompressionTest
	{
		private IWebHost host;
        
		private const string ApiLocation = "http://localhost:5001";
		private const string Endpoint = "/endpoint";
		private const string ProxyEndpoint = "http://localhost:3232/endpoint";
		private const string GetMethodCall = "GET";
		private const string OKResponseMessage = "OK";
		private const string ApplicationJsonContentType = "application/json";
		private const string ContentType = "Content-Type";
		private const string ContentEncoding = "Content-Encoding";
		private const string GZipEncoding = "gzip";
		private const string DeflateEncoding = "deflate";

		private const string ExpectedBody = @"{
				'Email': 'james@example.com',
				'Active': true,
				'CreatedDate': '2013-01-20T00:00:00Z',
				'Roles': [
					'User',
					'Admin'
				]}";

		public AutomaticDecompressionTest(HostFixture fixture)
		{
			host = fixture.Host;
		}

		[SkippableFact]
		public async Task HotPotato_ShouldAutomaticallyDecompressGZip()
		{
			var servicePro = host.Services;

			using (var server = FluentMockServer.Start(ApiLocation))
			{
				server
					.Given(
						Request.Create()
							.WithPath(Endpoint)
							.UsingGet()
					)
					.RespondWith(
						Response.Create()
							.WithStatusCode(200)
							.WithHeader(ContentType, ApplicationJsonContentType)
							.WithHeader(ContentEncoding, GZipEncoding)
							.WithBody(CompressGZipContent(ExpectedBody).ReadAsByteArrayAsync().Result)
					);

				HotPotatoClient client = (HotPotatoClient)servicePro.GetService<IHotPotatoClient>();

				HttpMethod method = new HttpMethod(GetMethodCall);

				using (HotPotatoRequest req = new HotPotatoRequest(method, new System.Uri(ProxyEndpoint)))
				{
					req.SetContent(CompressGZipContent(ExpectedBody).ReadAsByteArrayAsync().Result, ApplicationJsonContentType);
					IHotPotatoResponse res = await client.SendAsync(req);
					Assert.Equal(ExpectedBody, res.ToBodyString());
				}
			}
		}

		[SkippableFact]
		public async Task HotPotato_ShouldAutomaticallyDecompressDeflate()
		{
			var servicePro = host.Services;

			using (var server = FluentMockServer.Start(ApiLocation))
			{
				server
					.Given(
						Request.Create()
							.WithPath(Endpoint)
							.UsingGet()
					)
					.RespondWith(
						Response.Create()
							.WithStatusCode(200)
							.WithHeader(ContentType, ApplicationJsonContentType)
							.WithHeader(ContentEncoding, DeflateEncoding)
							.WithBody(CompressDeflateContent(ExpectedBody).ReadAsByteArrayAsync().Result, ApplicationJsonContentType)
					);


				HotPotatoClient client = (HotPotatoClient)servicePro.GetService<IHotPotatoClient>();

				HttpMethod method = new HttpMethod(GetMethodCall);

				using (HotPotatoRequest req = new HotPotatoRequest(method, new System.Uri(ProxyEndpoint)))
				{
					req.SetContent(CompressDeflateContent(ExpectedBody).ReadAsByteArrayAsync().Result, ApplicationJsonContentType);
					IHotPotatoResponse res = await client.SendAsync(req);
					Assert.Equal(ExpectedBody, res.ToBodyString());
				}
			}
		}

		private static HttpContent CompressGZipContent(string content)
		{
			MemoryStream compressedStream = new MemoryStream();
			using (MemoryStream contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				using (GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
				{
					contentStream.CopyTo(gzipStream);
				}
			}

			ByteArrayContent compressedContent = new ByteArrayContent(compressedStream.ToArray());
			compressedContent.Headers.Add("Content-Encoding", "gzip");
			return compressedContent;
		}

		private static HttpContent CompressDeflateContent(string content)
		{
			MemoryStream compressedStream = new MemoryStream();
			using (MemoryStream contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress))
				{
					contentStream.CopyTo(deflateStream);
				}
			}

			ByteArrayContent compressedContent = new ByteArrayContent(compressedStream.ToArray());
			compressedContent.Headers.Add("Content-Encoding", "deflate");
			return compressedContent;
		}
	}
}
